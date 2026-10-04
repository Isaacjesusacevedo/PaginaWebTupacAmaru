# Inventario de Tablas

| Tabla | Columnas | Registros | FKs Salientes | FKs Entrantes | Índices Únicos |
|-------|----------|-----------|---------------|---------------|----------------|
| Administradores | 9 | 4 | 0 | 0 | 2 |
| Alumnos | 14 | 4 | 1 | 1 | 2 |
| Carreras | 8 | 4 | 0 | 1 | 1 |
| Formularios | 7 | 2 | 0 | 0 | 1 |
| Profesores | 7 | 3 | 0 | 0 | 2 |
| **Inf_Academica** | 4 | 4 | 0 | 1 | 1 |
| **Inf_Academica_Est** | 7 | 0 | 2 | 0 | 2 |

---

## Estructura Detallada por Tabla

### Administradores

**Entidad C#:** `Instituto.AD.Models.Administrador`

| Propiedad C# | Columna BD | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|--------------|------------|------|----------|----------|----------|-------------|---------|
| Id | Id | int | | NO | SÍ | PK | |
| Nombre | Nombre | nvarchar | 100 | NO | | | |
| Apellido | Apellido | nvarchar | 100 | NO | | | |
| Email | Email | nvarchar | 150 | NO | | | |
| PasswordHash | PasswordHash | nvarchar | 255 | NO | | | |
| PasswordTemp | PasswordTemp | nvarchar | 255 | SÍ | | | |
| Role | Role | nvarchar | 50 | NO | | | 'Admin' |
| Activo | Activo | bit | | NO | | | 1 |
| FechaCreacion | FechaCreacion | datetime | | NO | | | GETDATE() |

**Mapeo EF (OnModelCreating):**
- `ToTable("Administradores")`
- `HasKey(e => e.Id)`
- `Property(e => e.Id).ValueGeneratedOnAdd()`
- `Property(e => e.Nombre).IsRequired().HasMaxLength(100)`
- `Property(e => e.Apellido).IsRequired().HasMaxLength(100)`
- `Property(e => e.Email).IsRequired().HasMaxLength(150)`
- `Property(e => e.PasswordHash).IsRequired().HasMaxLength(255)`
- `Property(e => e.PasswordTemp).HasMaxLength(255)`
- `Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("Admin")`
- `Property(e => e.Activo).HasDefaultValue(true)`
- `Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()")`
- **Índice UNIQUE filtrado:** `HasIndex(e => e.Email).IsUnique().HasFilter("[Activo] = 1")`

**Índices UNIQUE:**
- `PK__Administ__...` — Primary Key sobre `Id`.
- `UQ_Administradores_Email_Activo` — UNIQUE parcial sobre `Email WHERE Activo = 1`.

**Notas:**
- Implementa **soft delete** (borrar = `Activo = 0`).
- El índice UNIQUE parcial permite reutilizar emails de registros inactivos.
- **Todas las SPs que devuelven `Administrador` deben incluir `PasswordTemp`** (aunque sea NULL), porque EF Core lo exige por estar mapeado en el `DbContext`.
- Hereda de `Persona` (Id, Nombre, Apellido, Email).

---

### Alumnos

**Entidad C#:** `Instituto.AD.Models.Alumno`

| Propiedad C# | Columna BD | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|--------------|------------|------|----------|----------|----------|-------------|---------|
| Id | Id | int | | NO | SÍ | PK | |
| Nombre | Nombre | nvarchar | 100 | NO | | | |
| Apellido | Apellido | nvarchar | 100 | NO | | | |
| Email | Email | nvarchar | 150 | NO | | UNIQUE | |
| DNI | DNI | int | | NO | | UNIQUE | |
| FechaNacimiento | FechaNacimiento | datetime | | NO | | | |
| Direccion | Direccion | nvarchar | 200 | SÍ | | | |
| Nacionalidad | Nacionalidad | nvarchar | 100 | SÍ | | | |
| FechaInscripcion | FechaInscripcion | datetime | | SÍ | | | GETDATE() |
| Telefono | Telefono | nvarchar | 50 | SÍ | | | |
| TituloSecundario | TituloSecundario | nvarchar | 200 | SÍ | | | |
| Turno | Turno | nvarchar | 50 | SÍ | | | |
| CarreraId | CarreraId | int | | NO | | **FK** → Carreras.Id | |
| FechaCreacion | FechaCreacion | datetime | | NO | | | GETDATE() |
| **Edad** (computed) | — | int | | — | | — | `(DateTime.Now - FechaNacimiento).TotalDays / 365.25` |

**Mapeo EF (OnModelCreating):**
- `ToTable("Alumnos")`
- `HasKey(e => e.Id)`
- `Property(e => e.Id).ValueGeneratedOnAdd()`
- `Property(e => e.Nombre).IsRequired().HasMaxLength(100)`
- `Property(e => e.Apellido).IsRequired().HasMaxLength(100)`
- `Property(e => e.Email).IsRequired().HasMaxLength(150)`
- `Property(e => e.DNI).IsRequired()`
- `Property(e => e.FechaNacimiento).IsRequired()`
- `Property(e => e.Direccion).HasMaxLength(200)`
- `Property(e => e.Nacionalidad).HasMaxLength(100)`
- `Property(e => e.FechaInscripcion).HasDefaultValueSql("GETDATE()")`
- `Property(e => e.Telefono).HasMaxLength(50)`
- `Property(e => e.TituloSecundario).HasMaxLength(200)`
- `Property(e => e.Turno).HasMaxLength(50)`
- `Property(e => e.CarreraId).IsRequired()`
- `Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()")`
- **FK:** `HasOne<Carrera>().WithMany().HasForeignKey(a => a.CarreraId).OnDelete(DeleteBehavior.Restrict)`
- **Índices:** `HasIndex(e => e.Email).IsUnique()`, `HasIndex(e => e.DNI).IsUnique()`

**Relaciones:**
- FK_Alumnos_Carreras → Carreras(Id)
- FK_InfAcademicaEst_Alumnos ← Inf_Academica_Est (ON DELETE CASCADE)

**Notas:**
- `FechaInscripcion` se completa con `GETDATE()` si no se informa (tanto en SP como en EF).
- El campo `TituloSecundario` queda como legacy; el módulo de información académica lo reemplaza por un catálogo (`Inf_Academica`) + registros por alumno (`Inf_Academica_Est`).
- Propiedad computada `Edad` solo en C# (no mapeada a BD).
- Hereda de `Persona` (Id, Nombre, Apellido, Email).

---

### Carreras

**Entidad C#:** `Instituto.AD.Models.Carrera`

| Propiedad C# | Columna BD | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|--------------|------------|------|----------|----------|----------|-------------|---------|
| Id | Id | int | | NO | SÍ | PK | |
| Nombre | Nombre | nvarchar | 200 | NO | | | |
| DuracionAnios | DuracionAnios | int | | NO | | | |
| Turno | Turno | nvarchar | 50 | SÍ | | | |
| Modalidad | Modalidad | nvarchar | 50 | SÍ | | | |
| Horario | Horario | nvarchar | 100 | SÍ | | | |
| Estado | Estado | nvarchar | 50 | SÍ | | | 'Activa' |
| FechaCreacion | FechaCreacion | datetime | | NO | | | GETDATE() |

**Mapeo EF (OnModelCreating):**
- `ToTable("Carreras")`
- `HasKey(e => e.Id)`
- `Property(e => e.Id).ValueGeneratedOnAdd()`
- `Property(e => e.Nombre).IsRequired().HasMaxLength(200)`
- `Property(e => e.DuracionAnios).IsRequired()`
- `Property(e => e.Turno).HasMaxLength(50)`
- `Property(e => e.Modalidad).HasMaxLength(50)`
- `Property(e => e.Horario).HasMaxLength(100)`
- `Property(e => e.Estado).HasMaxLength(50).HasDefaultValue("Activa")`
- `Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()")`

**Nota:** No usa soft delete. Al eliminar, la FK de Alumnos impide la operación si hay alumnos asociados (`DeleteBehavior.Restrict`).

---

### Formularios

**Entidad C#:** `Instituto.AD.Models.Formulario`

| Propiedad C# | Columna BD | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|--------------|------------|------|----------|----------|----------|-------------|---------|
| Id | Id | int | | NO | SÍ | PK | |
| Nombre | Nombre | nvarchar | 200 | NO | | | |
| Estado | Estado | nvarchar | 50 | NO | | | 'Borrador' |
| FechaApertura | FechaApertura | datetime | | NO | | | |
| FechaCierre | FechaCierre | datetime | | NO | | | |
| Descripcion | Descripcion | nvarchar | 500 | SÍ | | | |
| FechaCreacion | FechaCreacion | datetime | | NO | | | GETDATE() |

**Mapeo EF (OnModelCreating):**
- `ToTable("Formularios")`
- `HasKey(e => e.Id)`
- `Property(e => e.Id).ValueGeneratedOnAdd()`
- `Property(e => e.Nombre).IsRequired().HasMaxLength(200)`
- `Property(e => e.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Borrador")`
- `Property(e => e.FechaApertura).IsRequired()`
- `Property(e => e.FechaCierre).IsRequired()`
- `Property(e => e.Descripcion).HasMaxLength(500)`
- `Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()")`

**Estados válidos:** `Borrador`, `Abierto`, `Cerrado`.

---

### Profesores

**Entidad C#:** `Instituto.AD.Models.Profesor`

| Propiedad C# | Columna BD | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|--------------|------------|------|----------|----------|----------|-------------|---------|
| Id | Id | int | | NO | SÍ | PK | |
| Nombre | Nombre | nvarchar | 100 | NO | | | |
| Apellido | Apellido | nvarchar | 100 | NO | | | |
| Email | Email | nvarchar | 150 | NO | | UNIQUE | |
| Telefono | Telefono | nvarchar | 50 | SÍ | | | |
| Especialidad | Especialidad | nvarchar | 100 | SÍ | | | |
| FechaCreacion | FechaCreacion | datetime | | NO | | | GETDATE() |

**Mapeo EF (OnModelCreating):**
- `ToTable("Profesores")`
- `HasKey(e => e.Id)`
- `Property(e => e.Id).ValueGeneratedOnAdd()`
- `Property(e => e.Nombre).IsRequired().HasMaxLength(100)`
- `Property(e => e.Apellido).IsRequired().HasMaxLength(100)`
- `Property(e => e.Email).IsRequired().HasMaxLength(150)`
- `Property(e => e.Telefono).HasMaxLength(50)`
- `Property(e => e.Especialidad).HasMaxLength(100)`
- `Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()")`
- **Índice:** `HasIndex(e => e.Email).IsUnique()`

**Notas:**
- Hereda de `Persona` (Id, Nombre, Apellido, Email).

---

### Inf_Academica (catálogo de tipos)

**Entidad C#:** `Instituto.AD.Models.InfAcademica`

Catálogo configurable de tipos de información académica. Agregado en 2026-10.

| Propiedad C# | Columna BD | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|--------------|------------|------|----------|----------|----------|-------------|---------|
| Id | ID_Inf_Aca | int | | NO | SÍ | PK | |
| Descripcion | Inf_Aca_Descripcion | nvarchar | 100 | NO | | | |
| Fecha | Inf_Aca_Fecha | date | | NO | | | |
| Estado | Inf_Aca_Estado | varchar | 15 | NO | | | 'DESHABILITADO' |

**Mapeo EF (OnModelCreating):**
- `ToTable("Inf_Academica")`
- `HasKey(e => e.Id)`
- `Property(e => e.Id).HasColumnName("ID_Inf_Aca").ValueGeneratedOnAdd()`
- `Property(e => e.Descripcion).HasColumnName("Inf_Aca_Descripcion").IsRequired().HasMaxLength(100)`
- `Property(e => e.Fecha).HasColumnName("Inf_Aca_Fecha").IsRequired()`
- `Property(e => e.Estado).HasColumnName("Inf_Aca_Estado").IsRequired().HasMaxLength(15).HasDefaultValue("DESHABILITADO")`

**Valores permitidos de `Estado` (`Inf_Aca_Estado`):** `HABILITADO`, `DESHABILITADO` (definidos en `InfAcademicaConstants.CatalogoHabilitado` / `CatalogoDeshabilitado`).

**Datos iniciales (seeding en 04_InfAcademica.sql):**

| ID | Descripción | Estado |
|----|-------------|--------|
| 1 | Título | HABILITADO |
| 2 | Título en trámite | HABILITADO |
| 3 | Constancia de materias adeudadas | HABILITADO |
| 4 | Constancia de alumno regular | HABILITADO |

**Tipos constantes (InfAcademicaConstants):**
- `TipoTitulo = "Título"`
- `TipoTituloEnTramite = "Título en trámite"`
- `TipoConstanciaMateriasAdeudadas = "Constancia de materias adeudadas"`
- `TipoConstanciaAlumnoRegular = "Constancia de alumno regular"`

---

### Inf_Academica_Est (registros por alumno)

**Entidad C#:** `Instituto.AD.Models.InfAcademicaEst`

Registro de cada tipo académico que un alumno posee, con su estado y fecha.

| Propiedad C# | Columna BD | Tipo | Longitud | Nullable | Identity | Restricción | Default |
|--------------|------------|------|----------|----------|----------|-------------|---------|
| Id | ID_Inf_Academica_Est | int | | NO | SÍ | PK | |
| InfAcademicaId | ID_Inf_Aca | int | | NO | | **FK** → Inf_Academica | |
| AlumnoId | ID_Est | int | | NO | | **FK** → Alumnos (CASCADE) | |
| FechaEmision | Fecha_Emision | date | | NO | | | |
| TituloSecundario | Titulo_Secundario | nvarchar | 100 | SÍ | | | |
| Institucion | Institucion | nvarchar | 150 | SÍ | | | |
| EstadoTitulo | Estado_Titulo | varchar | 15 | NO | | | |

**Mapeo EF (OnModelCreating):**
- `ToTable("Inf_Academica_Est")`
- `HasKey(e => e.Id)`
- `Property(e => e.Id).HasColumnName("ID_Inf_Academica_Est").ValueGeneratedOnAdd()`
- `Property(e => e.InfAcademicaId).HasColumnName("ID_Inf_Aca").IsRequired()`
- `Property(e => e.AlumnoId).HasColumnName("ID_Est").IsRequired()`
- `Property(e => e.FechaEmision).HasColumnName("Fecha_Emision").IsRequired()`
- `Property(e => e.TituloSecundario).HasColumnName("Titulo_Secundario").HasMaxLength(100)`
- `Property(e => e.Institucion).HasColumnName("Institucion").HasMaxLength(150)`
- `Property(e => e.EstadoTitulo).HasColumnName("Estado_Titulo").IsRequired().HasMaxLength(15)`
- **Índice UNIQUE:** `HasIndex(e => new { e.InfAcademicaId, e.AlumnoId }).IsUnique()`

**Valores permitidos de `EstadoTitulo` (`Estado_Titulo`):** `TRAMITE`, `MANO`, `PAUSA` (definidos en `InfAcademicaConstants.EstadosValidos`).

**Reglas de campos por tipo** (validadas en BR `InfAcademicaValidator.ResolverCamposPorTipo`, no en BD):

| Tipo (Descripcion) | `Titulo_Secundario` | `Institucion` |
|---------------------|---------------------|---------------|
| Título | **Obligatorio** (≤100) | NULL (se ignora) |
| Título en trámite | NULL (se ignora) | **Obligatorio** (≤150) |
| Constancias (materias adeudadas, alumno regular) | NULL | NULL |

**Estados por defecto (InfAcademicaValidator.ResolverEstadoPorDefecto):**
- `Título en trámite` → `TRAMITE`
- Otros tipos → `MANO`

**Validaciones de fecha (InfAcademicaValidator.ValidarFechaEmision):**
- Fecha obligatoria (no puede ser default)
- En `TRAMITE` la fecha puede ser futura (estimada)
- En `MANO` / `PAUSA` la fecha no puede ser futura

**Índices UNIQUE:**
- `PK_Inf_Academica_Est` — Primary Key sobre `ID_Inf_Academica_Est`.
- `UQ_InfAcademicaEst_InfAca_Est` — UNIQUE `(ID_Inf_Aca, ID_Est)` → un alumno no puede repetir tipo.

**Notas:**
- `Inf_Academica_Est_Update` **no recibe `ID_Est` (AlumnoId)**: el alumno de un registro es inmutable (ni siquiera a nivel SP). En BR se fuerza `entity.AlumnoId = existente.AlumnoId` al actualizar.
- `ON DELETE CASCADE` en FK `ID_Est`: si se borra un alumno, se borran sus registros académicos automáticamente.
- El repositorio usa `SpInvoker` (helpers de `InstitutoDbContext`): `QueryEntityAsync`, `ScalarInsertAsync`, `NonQueryAsync`, `ExistsAsync`.