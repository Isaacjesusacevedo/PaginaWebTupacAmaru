# Información Académica, Listado e Inscripción

> Alcance de esta documentación: listado de alumnos, información académica (catálogo +
> registros por alumno) e inscripción pública. No cubre Alumno/Carrera/Administrador/
> Profesor/Formulario existentes salvo donde se integran con esta funcionalidad.
> Última actualización: 2026-10-02.

---

## 1. Qué se agregó

Antes, un alumno tenía un único campo libre `TituloSecundario` en la tabla `Alumnos`. Ahora
la información académica es un catálogo de tipos (`Inf_Academica`) y registros por alumno
(`Inf_Academica_Est`), con reglas propias de validación y estado. Esto permite:

- Un catálogo configurable de tipos de información académica (Título, Título en trámite,
  Constancia de materias adeudadas, Constancia de alumno regular), habilitable/deshabilitable.
- Que un alumno tenga varios registros académicos (uno por tipo), cada uno con su propio
  estado (`TRAMITE` / `MANO` / `PAUSA`) y fecha de emisión.
- Un listado consolidado que, por alumno, muestra el registro académico más relevante
  (por prioridad de tipo y luego por fecha) y la edad calculada.
- Una inscripción pública que crea el alumno y su información académica en una sola operación
  transaccional (si falla el guardado académico, se revierte el alta del alumno).

Se mantuvo intacta la arquitectura N-Tier existente (AD → BR → API) y el patrón de Stored
Procedures invocados desde EF Core.

---

## 2. Base de datos

Script: [`Docs/sql/04_InfAcademica.sql`](./sql/04_InfAcademica.sql) (idempotente, se puede
ejecutar varias veces sin duplicar nada).

### Tablas nuevas

**`Inf_Academica`** (catálogo)

| Columna | Tipo | Notas |
|---|---|---|
| `ID_Inf_Aca` | `INT IDENTITY` | PK |
| `Inf_Aca_Descripcion` | `NVARCHAR(100)` | Nombre del tipo (ej. "Título") |
| `Inf_Aca_Fecha` | `DATE` | Fecha del tipo en el catálogo |
| `Inf_Aca_Estado` | `VARCHAR(15)` | `HABILITADO` / `DESHABILITADO`, default `DESHABILITADO` |

Datos iniciales (estado `HABILITADO`): *Título*, *Título en trámite*,
*Constancia de materias adeudadas*, *Constancia de alumno regular*.

**`Inf_Academica_Est`** (registro académico por alumno)

| Columna | Tipo | Notas |
|---|---|---|
| `ID_Inf_Academica_Est` | `INT IDENTITY` | PK |
| `ID_Inf_Aca` | `INT` | FK → `Inf_Academica` |
| `ID_Est` | `INT` | FK → `Alumnos(Id)`, `ON DELETE CASCADE` |
| `Fecha_Emision` | `DATE` | Real o estimada (si el estado es `TRAMITE`) |
| `Titulo_Secundario` | `NVARCHAR(100)` NULL | Solo se completa si el tipo es "Título" |
| `Institucion` | `NVARCHAR(150)` NULL | Solo se completa si el tipo es "Título en trámite" |
| `Estado_Titulo` | `VARCHAR(15)` | `TRAMITE` / `MANO` / `PAUSA` |

UNIQUE `(ID_Inf_Aca, ID_Est)`: un alumno no puede tener dos registros del mismo tipo.

### Stored Procedures (solo Data Access, cero validaciones)

- `Inf_Academica_{List, GetById, Insert, Update, Delete}`
- `Inf_Academica_Est_{List, GetById, GetByAlumno, Insert, Update, Delete, Exists, ExistsByCombination, ExistsByInfAca}`
  - `ExistsByCombination` tiene un `@ExcludeId` opcional para que la BR pueda validar
    duplicados al editar sin chocar contra el propio registro.
  - `Inf_Academica_Est_Update` **no recibe `ID_Est`**: el alumno de un registro académico
    es inmutable incluso a nivel de Stored Procedure, no solo por regla de negocio.
- `sp_Listado_GetAll` — **reescrito**. Antes hacía `Alumnos LEFT JOIN Carreras` y calculaba la
  edad en SQL. Ahora hace `Alumnos LEFT JOIN Carreras LEFT JOIN Inf_Academica_Est LEFT JOIN
  Inf_Academica`, devuelve una fila por cada registro académico del alumno (o una fila con
  columnas académicas en `NULL` si no tiene ninguno), y **no calcula edad ni decide qué fila
  mostrar** — eso lo hace la BR.

---

## 3. Capa AD (`Instituto.AD`)

- **Entidades** [`Models/InfAcademica.cs`](../Instituto.AD/Models/InfAcademica.cs) y
  [`Models/InfAcademicaEst.cs`](../Instituto.AD/Models/InfAcademicaEst.cs): propiedades en
  inglés/PascalCase normal (`Id`, `Descripcion`, `InfAcademicaId`, `AlumnoId`, ...), mapeadas
  con `HasColumnName` a las columnas reales en
  [`Data/InstitutoDbContext.cs`](../Instituto.AD/Data/InstitutoDbContext.cs) (solo se
  agregaron los 2 `DbSet` nuevos y sus bloques de `OnModelCreating`, nada existente se tocó).
- **`ListadoItem`** ([`Models/ListadoItem.cs`](../Instituto.AD/Models/ListadoItem.cs)):
  adaptado a las columnas del nuevo `sp_Listado_GetAll` — se quitó `Edad` (se calcula ahora en
  la BR) y se agregaron `FechaNacimiento`, `TipoAcademico`, `TituloSecundario`, `FechaEmision`,
  `EstadoTitulo`.
- **[`SpInvoker.cs`](../Instituto.AD/SpInvoker.cs)**: helper reutilizable (métodos de
  extensión sobre `InstitutoDbContext`) para invocar SPs desde EF Core sin repetir código:
  - `QueryEntityAsync<T>` — `FromSqlRaw` + `ToListAsync()`, nunca `FirstOrDefaultAsync()` sobre
    el `IQueryable`; el filtrado puntual se hace en memoria después de materializar.
  - `ExistsAsync` — ejecuta el SP de `Exists` (devuelve 1/0) y lo traduce a `bool`.
  - `ScalarInsertAsync` — abre la conexión con `Database.OpenConnectionAsync()`, ejecuta un
    único `ExecuteScalarAsync()` y la cierra siempre en un `finally` con `CloseConnectionAsync()`.
  - `NonQueryAsync` — `ExecuteSqlRawAsync` para `UPDATE`/`DELETE`.
- **Repositorios e interfaces nuevos** (en archivos propios, no en los compartidos
  `IRepositories.cs`): [`Interfaces/IInfAcademicaRepositories.cs`](../Instituto.AD/Interfaces/IInfAcademicaRepositories.cs),
  [`Repositories/InfAcademicaRepository.cs`](../Instituto.AD/Repositories/InfAcademicaRepository.cs),
  [`Repositories/InfAcademicaEstRepository.cs`](../Instituto.AD/Repositories/InfAcademicaEstRepository.cs).
  No validan ni normalizan nada, solo ejecutan los SP a través del `SpInvoker`.
- **`ListadoRepository.cs`**: se eliminó un método privado muerto (`MapReaderToListadoItem`,
  nunca se llamaba) que había quedado referenciando la columna `Edad` ya eliminada.

---

## 4. Capa BR (`Instituto.BR`)

### Reglas centralizadas

- [`InfAcademicaConstants.cs`](../Instituto.BR/InfAcademicaConstants.cs): los 4 nombres de
  tipo exactos, los 3 estados (`TRAMITE`/`MANO`/`PAUSA`) y los 2 estados de catálogo
  (`HABILITADO`/`DESHABILITADO`).
- [`InfAcademicaValidator.cs`](../Instituto.BR/InfAcademicaValidator.cs): lógica de
  validación compartida entre `InfAcademicaEstService` e `InscripcionService` (para no
  duplicarla en los dos lugares donde se guarda un registro académico):
  - Estado por defecto si no viene informado: `TRAMITE` para "Título en trámite", `MANO`
    para el resto.
  - Estado permitido (uno de los 3 válidos).
  - Fecha de emisión obligatoria y no futura, **salvo** que el estado sea `TRAMITE` (ahí es
    una fecha estimada y puede ser futura).
  - Campos por tipo: "Título" exige `TituloSecundario` (≤100 car.) y descarta `Institucion`;
    "Título en trámite" exige `Institucion` (≤150 car.) y descarta `TituloSecundario`;
    cualquier otro tipo descarta ambos.

### Servicios nuevos

- [`InfAcademicaService.cs`](../Instituto.BR/Services/InfAcademicaService.cs) — CRUD del
  catálogo. Valida descripción (obligatoria, ≤100), fecha obligatoria y estado
  `HABILITADO`/`DESHABILITADO`. No permite borrar un tipo que ya tiene algún alumno asignado
  (`ExistsByInfAcademicaAsync`).
- [`InfAcademicaEstService.cs`](../Instituto.BR/Services/InfAcademicaEstService.cs) — CRUD de
  los registros académicos por alumno. Orden de validación: el tipo existe y está
  `HABILITADO` → el alumno existe → estado permitido → fecha de emisión → campos según el
  tipo → no repetir (tipo + alumno). En `UpdateAsync` fuerza
  `entity.AlumnoId = existente.AlumnoId` sin importar lo que venga en el request: el alumno
  de un registro académico nunca cambia.
- [`InscripcionService.cs`](../Instituto.BR/Services/InscripcionService.cs) —
  `InscribirAsync(InscripcionDto)`: resuelve cada tipo informado contra el catálogo, valida
  **todo** el bloque académico primero (incluyendo que no se repita un tipo dentro del mismo
  envío), recién ahí crea el alumno reusando el `IAlumnoService` existente, guarda cada
  registro académico y, si algo falla en ese paso, **borra el alumno recién creado** y
  devuelve el error (no queda un alumno sin su información académica).
- Ambos servicios de escritura capturan `SqlException` 2627/2601 (violación de índice único)
  y lo traducen a un mensaje de duplicado, igual que el resto de los servicios del proyecto.

### Modificados (explícitamente pedido)

- `AlumnoListadoDto` (en `DTOs/CommonDtos.cs`): se agregaron `TipoAcademico`, `EstadoTitulo`,
  `FechaEmision`, `TituloSecundario` manteniendo los campos existentes.
- [`ListadoService.cs`](../Instituto.BR/Services/ListadoService.cs): agrupa las filas del
  nuevo `sp_Listado_GetAll` por `AlumnoId` y elige una sola por alumno, por prioridad de tipo
  (**Título > Título en trámite > constancias**) y luego por fecha de emisión más reciente.
  Calcula la `Edad` (años cumplidos a hoy) a partir de `FechaNacimiento`. Solo informa
  `TituloSecundario` cuando el tipo elegido es "Título".

### DTOs e interfaces nuevas

En archivos propios, nunca en los compartidos (`CommonDtos.cs`/`IServices.cs`, salvo el
cambio a `AlumnoListadoDto` ya descripto):
[`DTOs/InscripcionDtos.cs`](../Instituto.BR/DTOs/InscripcionDtos.cs) (`InscripcionDto`,
`InformacionAcademicaItemDto`, `InscripcionResultDto`) e
[`Interfaces/IInfAcademicaServices.cs`](../Instituto.BR/Interfaces/IInfAcademicaServices.cs).

---

## 5. API nueva: `Instituto.MinimalAPI.Academica`

Proyecto Minimal API independiente, agregado a `Instituto.sln`, puerto **5128**. Un archivo
por endpoint en [`Endpoints/`](../Instituto.MinimalAPI.Academica/Endpoints/).

| Método | Ruta | Auth | Descripción |
|---|---|---|---|
| GET | `/health` | público | Estado de la API y conexión a la BD |
| GET | `/api/listado` | **público** | Listado consolidado con info académica (ver sección 4) |
| GET | `/api/inf-academica` | JWT admin | Catálogo completo |
| GET | `/api/inf-academica/{id}` | JWT admin | Un tipo del catálogo (404 si no existe) |
| POST | `/api/inf-academica` | JWT admin | Crea un tipo (201) |
| PUT | `/api/inf-academica/{id}` | JWT admin | Actualiza un tipo |
| DELETE | `/api/inf-academica/{id}` | JWT admin | Elimina un tipo (204) |
| GET | `/api/inf-academica-alumnos?alumnoId=` | JWT admin | Registros académicos (todos o por alumno) |
| POST | `/api/inf-academica-alumnos` | JWT admin | Crea un registro para un alumno existente (201) |
| PUT | `/api/inf-academica-alumnos/{id}` | JWT admin | Actualiza un registro (el alumno no cambia) |
| DELETE | `/api/inf-academica-alumnos/{id}` | JWT admin | Elimina un registro (204) |
| POST | `/api/inscripcion` | público | Inscribe alumno + información académica, responde `{ alumnoId }` (201) |

Formato de respuesta uniforme: `{ isSuccess, message, data }`. Los errores de negocio de la
BR responden `400` con el mensaje tal cual; cualquier excepción no controlada la loguea el
handler global y responde `500` con un mensaje genérico (nunca expone el detalle).

### Configuración necesaria antes de levantarla

- **`Jwt:Key`** (mínimo 32 caracteres) — **no está en los archivos de configuración**, la API
  falla al iniciar si falta o es corta. Configurarla con:
  ```bash
  cd Instituto.MinimalAPI.Academica
  dotnet user-secrets set "Jwt:Key" "una-clave-de-desarrollo-de-al-menos-32-caracteres"
  ```
  o con la variable de entorno `Jwt__Key`.
- `Jwt:Issuer` / `Jwt:Audience` ya están en `appsettings.json` (`InstitutoAcademicaApi`).
- `Cors:AllowedOrigins` ya incluye `http://localhost:5173` (puerto por defecto de Vite).
- Connection string: igual que la API original (`appsettings.Development.json` con el
  usuario `instituto_user` / `Instituto2026`, o LocalDB en `appsettings.json`).

**Importante**: la API de autenticación todavía no existe en el proyecto, así que no hay
forma de emitir un JWT real con rol Admin/SuperAdmin todavía. Los endpoints
admin (`/api/inf-academica*`, `/api/inf-academica-alumnos*`) están protegidos pero no se
pudieron probar con un token real — `POST /api/inscripcion`, `GET /health` y **`GET /api/listado`**
son verificables sin login.

---

## 6. Frontend (`Frontend/`)

- [`src/composables/useAcademicaApi.js`](../Frontend/src/composables/useAcademicaApi.js)
  (nuevo, no se tocó `useApiFetch.js`): apunta a `VITE_API_ACADEMICA_URL` (default
  `http://localhost:5128`), toma el token Bearer de `sessionStorage` (misma clave que usa el
  resto del sitio), desenvuelve `{ isSuccess, data }` y lanza el `message` cuando falla.
- `.env.local` (no versionado): `VITE_API_ACADEMICA_URL=http://localhost:5128`.
- [`src/views/Listados/ListadoView.vue`](../Frontend/src/views/Listados/ListadoView.vue):
  ahora consume `/api/listado` vía `useAcademicaApi`. 7 columnas con la clase existente
  `table-cols-default` (no se tocó `table.css`): Alumno, DNI, Edad, Carrera, Turno,
  Información académica (tipo · estado legible *En trámite/En mano/En pausa* · fecha), y
  Título secundario (`—` si no hay).
- [`src/views/Listados/InscripciónView.vue`](../Frontend/src/views/Listados/InscripciónView.vue):
  - Etiqueta "Fecha de egreso del secundario" → **"Fecha de emisión del título (o
    constancia)"**, reutilizada también para las dos constancias.
  - "Título" y "Título en trámite" siguen siendo excluyentes entre sí.
  - Cada constancia tildada (materias adeudadas / alumno regular) pide ahora su propia fecha
    de emisión.
  - Un único `POST /api/inscripcion` con el payload `{ nombre, apellido, email, dni,
    fechaNacimiento, direccion, nacionalidad, telefono, turno, carreraId,
    informacionAcademica: [{ tipo, fechaEmision, tituloSecundario?, institucion? }] }`.
  - La edad se calcula fija al **30/06/2026** (antes usaba la fecha de hoy).
  - La carga de carreras no se tocó (sigue con `fetch` directo a `VITE_API_URL`, la API
    original en el puerto 5127).

---

## 7. Cómo probar todo de punta a punta

1. Levantar SQL Server y ejecutar `Docs/sql/04_InfAcademica.sql` contra `InstitutoDB`.
   Verificar con `EXEC Inf_Academica_List;` y `EXEC sp_Listado_GetAll;`.
2. `cd Instituto.MinimalAPI.Academica && dotnet user-secrets set "Jwt:Key" "..."` (≥32
   caracteres) y `dotnet run` → `http://localhost:5128/swagger`.
3. `cd Frontend && npm install && npm run dev` → probar `/inscripcion` (público) de punta a
   punta y confirmar que el alumno y su información académica quedan guardados.
4. El listado y los endpoints admin necesitan un JWT con rol Admin/SuperAdmin que hoy nadie
   emite (no existe la API de auth todavía) — pendiente hasta que exista.

---

## 8. Pendiente / limitaciones conocidas

- No se pudo compilar (`dotnet build`) ni correr la API en este entorno: no había SDK de
  .NET instalado al momento de escribir este documento.
- SQL Server estaba detenido y sin permisos para iniciarlo desde esta sesión: el script SQL
  se escribió y quedó listo, pero no se ejecutó ni se verificó con `EXEC` en vivo.
- `npm run lint` del Frontend no corre por un problema **preexistente** del repo (no
  relacionado con este trabajo): `eslint.config.ts` depende de `@vue/eslint-config-typescript`,
  que nunca llegó a instalarse como dependencia. `npm run build` sí se probó y compiló sin
  errores con todos los archivos de este trabajo.
- No existe todavía una API de autenticación que emita JWT reales, así que los
  endpoints admin de información académica (`/api/inf-academica*`, `/api/inf-academica-alumnos*`)
  quedaron protegidos pero sin poder probarse con un token real.
- El listado `/api/listado` ya es **público** (AllowAnonymous) para que el frontend pueda
  consumirlo directamente desde la API académica (puerto 5128) usando `useAcademicaApi`.

---

## 9. Archivos nuevos / modificados

**Nuevos:**
- `Docs/sql/04_InfAcademica.sql`
- `Instituto.AD/Models/{InfAcademica.cs, InfAcademicaEst.cs}`, `Instituto.AD/SpInvoker.cs`,
  `Instituto.AD/Interfaces/IInfAcademicaRepositories.cs`,
  `Instituto.AD/Repositories/{InfAcademicaRepository.cs, InfAcademicaEstRepository.cs}`
- `Instituto.BR/{InfAcademicaConstants.cs, InfAcademicaValidator.cs}`,
  `Instituto.BR/DTOs/InscripcionDtos.cs`, `Instituto.BR/Interfaces/IInfAcademicaServices.cs`,
  `Instituto.BR/Services/{InfAcademicaService.cs, InfAcademicaEstService.cs, InscripcionService.cs}`
- `Instituto.MinimalAPI.Academica/` completo (csproj, `Program.cs`, `ApiResults.cs`,
  `appsettings*.json`, `Properties/launchSettings.json`, 12 archivos en `Endpoints/`)
- `Frontend/src/composables/useAcademicaApi.js`, `Frontend/.env.local`
- Este documento y `Docs/sql/04_InfAcademica.sql`

**Modificados** (dentro de alcance o explícitamente pedidos):
- `Instituto.AD/Data/InstitutoDbContext.cs` (solo agregados),
  `Instituto.AD/Models/ListadoItem.cs`, `Instituto.AD/Repositories/ListadoRepository.cs`
- `Instituto.BR/DTOs/CommonDtos.cs` (`AlumnoListadoDto`), `Instituto.BR/Services/ListadoService.cs`
- `Instituto.sln` (se agregó el proyecto nuevo a la solución)
- `Frontend/src/views/Listados/ListadoView.vue`, `Frontend/src/views/Listados/InscripciónView.vue`

**No tocados:** repositorios/servicios de Alumno, Carrera, Administrador, Profesor y
Formulario; `IRepositories.cs`; `IServices.cs` (salvo el cambio puntual a `AlumnoListadoDto`
ya descripto); `useApiFetch.js`; `table.css`; `Instituto.MinimalAPI/Program.cs`; ningún test
existente.
