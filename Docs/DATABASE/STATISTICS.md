# Estadísticas

> Basado en `InstitutoDbContext`, scripts SQL y snapshot de datos.

---

## Resumen General

| Métrica | Valor |
|---------|-------|
| Total tablas | 7 |
| Total columnas (BD) | 56 |
| Total registros (base) | 17 |
| Total registros (académica) | 4 (solo catálogo) |
| FKs definidas | 3 |
| Índices UNIQUE | 9 |
| Índices UNIQUE parciales | 1 (`UQ_Administradores_Email_Activo`) |
| Stored Procedures | ~40 (base + académicas) |
| Entidades C# | 7 (+ 1 abstracta `Persona` + 1 DTO `ListadoItem`) |
| Repositorios | 7 |
| Servicios BR | 8 |
| Constantes/Validadores | 2 (`InfAcademicaConstants`, `InfAcademicaValidator`) |

---

## Desglose por Tabla

| Tabla | Columnas BD | Propiedades C# | Registros | Índices UNIQUE | FKs Salientes |
|-------|-------------|----------------|-----------|----------------|---------------|
| Administradores | 9 | 9 (hereda 4 de Persona) | 4 | 2 (1 parcial) | 0 |
| Alumnos | 14 | 15 (hereda 4 + 1 computed `Edad`) | 4 | 2 | 1 (CarreraId) |
| Carreras | 8 | 8 | 4 | 1 | 0 |
| Formularios | 7 | 7 | 2 | 1 | 0 |
| Profesores | 7 | 7 (hereda 4 de Persona) | 3 | 1 | 0 |
| Inf_Academica | 4 | 4 | 4 | 1 | 0 |
| Inf_Academica_Est | 7 | 7 | 0 | 1 compuesto | 2 (InfAcademicaId, AlumnoId) |

---

## Columnas por Tabla (Detalle)

### Administradores (9)
`Id, Nombre, Apellido, Email, PasswordHash, PasswordTemp, Role, Activo, FechaCreacion`

### Alumnos (14)
`Id, Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad, FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion`

### Carreras (8)
`Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion`

### Formularios (7)
`Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion`

### Profesores (7)
`Id, Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion`

### Inf_Academica (4)
`ID_Inf_Aca, Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado`

### Inf_Academica_Est (7)
`ID_Inf_Academica_Est, ID_Inf_Aca, ID_Est, Fecha_Emision, Titulo_Secundario, Institucion, Estado_Titulo`

---

## Foreign Keys

| # | FK | Tipo |
|---|-----|------|
| 1 | `FK_Alumnos_Carreras` | 1:N (Restrict) |
| 2 | `FK_InfAcademicaEst_InfAcademica` | 1:N (No Action) |
| 3 | `FK_InfAcademicaEst_Alumnos` | 1:N (CASCADE) |

---

## Índices UNIQUE

| # | Tabla | Índice | Columnas | Tipo |
|---|-------|--------|----------|------|
| 1 | Administradores | PK | Id | PK |
| 2 | Administradores | UQ_Administradores_Email_Activo | Email | Parcial (WHERE Activo=1) |
| 3 | Alumnos | PK | Id | PK |
| 4 | Alumnos | UQ_Alumnos_Email | Email | Único |
| 5 | Alumnos | UQ_Alumnos_DNI | DNI | Único |
| 6 | Carreras | PK | Id | PK |
| 7 | Profesores | PK | Id | PK |
| 8 | Profesores | UQ_Profesores_Email | Email | Único |
| 9 | Formularios | PK | Id | PK |
| 10 | Inf_Academica | PK | ID_Inf_Aca | PK |
| 11 | Inf_Academica_Est | PK | ID_Inf_Academica_Est | PK |
| 12 | Inf_Academica_Est | UQ_InfAcademicaEst_InfAca_Est | (ID_Inf_Aca, ID_Est) | Compuesto |

---

## Stored Procedures por Categoría

| Categoría | Cantidad | Prefijo |
|-----------|----------|---------|
| Carreras | 6 | `sp_Carreras_` |
| Alumnos | 9 | `sp_Alumnos_` |
| Administradores | 10 | `sp_Administradores_` |
| Profesores | 7 | `sp_Profesores_` |
| Formularios | 6 | `sp_Formularios_` |
| Listado | 1 | `sp_Listado_` |
| Inf_Academica (catálogo) | 5 | `Inf_Academica_` |
| Inf_Academica_Est (registros) | 8 | `Inf_Academica_Est_` |
| **Total** | **~52** | |

> Nota: El conteo exacto puede variar si hay SPs duplicados entre scripts. La versión final en BD tiene ~40 únicas porque `sp_Listado_GetAll` se sobrescribe.

---

## Entidades C# en `Instituto.AD.Models`

| Clase | Hereda de | Propiedades | Mapeada a BD |
|-------|-----------|-------------|--------------|
| `Persona` | (abstract) | Id, Nombre, Apellido, Email | No (base) |
| `Administrador` | Persona | + Role, PasswordHash, PasswordTemp, Activo, FechaCreacion | Sí |
| `Alumno` | Persona | + DNI, FechaNacimiento, Direccion, Nacionalidad, FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion, **Edad** (computed) | Sí |
| `Profesor` | Persona | + Telefono, Especialidad, FechaCreacion | Sí |
| `Carrera` | — | Id, Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion | Sí |
| `Formulario` | — | Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion | Sí |
| `InfAcademica` | — | Id, Descripcion, Fecha, Estado | Sí |
| `InfAcademicaEst` | — | Id, InfAcademicaId, AlumnoId, FechaEmision, TituloSecundario, Institucion, EstadoTitulo | Sí |
| `ListadoItem` | — | 11 props (resultado SP) | No (DTO de SP) |

---

## Servicios en `Instituto.BR.Services`

| Servicio | Interfaz | Responsabilidad |
|----------|----------|-----------------|
| `AdministradorService` | `IAdministradorService` | CRUD admins, auth, soft delete |
| `AlumnoService` | `IAlumnoService` | CRUD alumnos |
| `CarreraService` | `ICarreraService` | CRUD carreras |
| `ProfesorService` | `IProfesorService` | CRUD profesores |
| `FormularioService` | `IFormularioService` | CRUD formularios |
| `InfAcademicaService` | `IInfAcademicaService` | CRUD catálogo académico |
| `InfAcademicaEstService` | `IInfAcademicaEstService` | CRUD registros académicos + validaciones |
| `InscripcionService` | `IInscripcionService` | Inscripción pública (alumno + académicos) |
| `ListadoService` | `IListadoService` | Listado agrupado con prioridad académica |

---

## DTOs en `Instituto.BR.DTOs`

| DTO | Uso |
|-----|-----|
| `InscripcionDto` | Input inscripción pública |
| `InscripcionResultDto` | Output inscripción (AlumnoId) |
| `AlumnoListadoDto` | Output listado (1 fila por alumno) |
| `InfAcademicaDto` | Input/Output catálogo |
| `InfAcademicaEstDto` | Input/Output registros |
| `ServiceResult<T>` | Wrapper resultado operaciones |