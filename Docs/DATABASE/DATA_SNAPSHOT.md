# Datos Actuales (Snapshot 2026-10-02)

## Carreras (4 registros)

| Id | Nombre | Duración | Turno | Modalidad | Horario | Estado |
|----|--------|----------|-------|-----------|---------|--------|
| 1 | Tecnicatura Superior en Desarrollo de Software | 3 años | Noche | Presencial | 18:30 a 22:30 | Activa |
| 2 | Tecnicatura Superior en Enfermería | 3 años | Mañana | Presencial | 08:00 a 13:00 | Activa |
| 3 | Tecnicatura Superior en Administración de Empresas | 3 años | Tarde | Mixta | 14:00 a 18:30 | Activa |
| 4 | Profesorado en Educación Inicial | 4 años | Mañana | Presencial | 08:00 a 13:30 | Activa |

---

## Administradores

| Id | Nombre | Apellido | Email | Role | Activo | Estado |
|----|--------|----------|-------|------|--------|--------|
| 16 | isaac | Acevedo Rengifo | admin@instituto.edu.ar | Admin | 0 | Soft delete |
| 17 | isaac | Acevedo Rengifo | admin@tupac.edu.ar | SuperAdmin | 1 | Activo |
| 20 | Tahiel | Cassata | isa@test.com | Admin | 0 | Soft delete |
| 21 | Tahiel | Cassata | isa@test.com | Admin | 1 | Activo |

> **Nota:** El email `isa@test.com` aparece 2 veces (Id 20 inactivo + Id 21 activo). Eso es posible gracias al índice UNIQUE parcial `WHERE Activo = 1`.

---

## Alumnos (4 registros)

| Id | Nombre | Apellido | DNI | Turno | CarreraId | Carrera |
|----|--------|----------|-----|-------|-----------|---------|
| 1 | Ana | Martínez | 40123456 | Noche | 1 | Desarrollo de Software |
| 2 | Pedro | López | 41234567 | Noche | 1 | Desarrollo de Software |
| 3 | Lucía | Fernández | 42345678 | Mañana | 2 | Enfermería |
| 4 | Diego | Sánchez | 43456789 | Tarde | 3 | Administración |

---

## Profesores (3 registros)

| Id | Nombre | Apellido | Email | Teléfono | Especialidad |
|----|--------|----------|-------|----------|--------------|
| 1 | Juan | Pérez | juan.perez@tupac.edu.ar | 3814567890 | Programación |
| 2 | María | González | maria.gonzalez@tupac.edu.ar | 3814567891 | Enfermería |
| 3 | Carlos | Rodríguez | carlos.rodriguez@tupac.edu.ar | 3814567892 | Matemáticas |

---

## Formularios (2 registros)

| Id | Nombre | Estado | FechaApertura | FechaCierre |
|----|--------|--------|---------------|-------------|
| 1 | Inscripción 2026 - Primer Cuatrimestre | Cerrado | 2026-09-27 | 2026-09-30 |
| 2 | Inscripción 2026 - Segundo Cuatrimestre | Borrador | 2026-06-01 | 2026-08-31 |

---

## Inf_Academica (catálogo, 4 tipos)

> **Nota:** Los IDs pueden variar si se recrea la BD. El código usa `InfAcademicaConstants` para referenciar por **Descripcion**, no por ID.

| ID_Inf_Aca | Inf_Aca_Descripcion | Inf_Aca_Estado | Constante C# |
|------------|---------------------|----------------|--------------|
| 1 | Título | HABILITADO | `InfAcademicaConstants.TipoTitulo` |
| 2 | Título en trámite | HABILITADO | `InfAcademicaConstants.TipoTituloEnTramite` |
| 3 | Constancia de materias adeudadas | HABILITADO | `InfAcademicaConstants.TipoConstanciaMateriasAdeudadas` |
| 4 | Constancia de alumno regular | HABILITADO | `InfAcademicaConstants.TipoConstanciaAlumnoRegular` |

---

## Inf_Academica_Est (registros por alumno)

*Sin registros al 2026-10-02. Se cargan vía inscripción pública o CRUD admin.*