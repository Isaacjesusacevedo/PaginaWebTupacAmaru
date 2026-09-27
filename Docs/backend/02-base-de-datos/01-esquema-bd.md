# Esquema de Base de Datos

## Diagrama Entidad-Relación

```
┌─────────────────┐       ┌─────────────────┐
│  ADMINISTRADORES │       │    CARRERAS     │
├─────────────────┤       ├─────────────────┤
│ PK Id           │       │ PK Id           │
│ Nombre          │       │ Nombre          │
│ Apellido        │       │ DuracionAnios   │
│ Email (UQ)      │       │ Turno           │
│ PasswordHash    │       │ Modalidad       │
│ Role            │       │ Horario         │
│ Activo (bit)    │       │ Estado          │
│ FechaCreacion   │       └────────┬────────┘
└─────────────────┘                │
                                   │ 1:N
                                   ▼
┌─────────────────┐       ┌─────────────────┐
│    ALUMNOS      │       │   PROFESORES    │
├─────────────────┤       ├─────────────────┤
│ PK Id           │       │ PK Id           │
│ Nombre          │       │ Nombre          │
│ Apellido        │       │ Apellido        │
│ Email           │       │ Email (UQ)      │
│ DNI (UQ)        │       │ Telefono        │
│ FechaNacimiento │       │ Especialidad    │
│ Direccion       │       └─────────────────┘
│ Nacionalidad    │
│ FechaInscripcion│
│ Telefono        │
│ TituloSecundario│
│ Turno           │
│ FK CarreraId ───┘
└─────────────────┘

┌─────────────────┐
│  FORMULARIOS    │
├─────────────────┤
│ PK Id           │
│ Nombre          │
│ Estado          │ (Borrador/Abierto/Cerrado)
│ FechaApertura   │
│ FechaCierre     │
│ Descripcion     │
│ FechaCreacion   │
└─────────────────┘
```

## Tablas Detalladas

### 1. Administradores

| Columna | Tipo | Null | Default | Constraints | Descripción |
|---------|------|------|---------|-------------|-------------|
| Id | INT | NO | IDENTITY(1,1) | PK, CLUSTERED | Identificador único |
| Nombre | NVARCHAR(100) | NO | | | Nombre(s) |
| Apellido | NVARCHAR(100) | NO | | | Apellido(s) |
| Email | NVARCHAR(255) | NO | | UNIQUE, INDEX | Login único |
| PasswordHash | NVARCHAR(255) | NO | | | BCrypt hash (work factor 12) |
| Role | NVARCHAR(50) | NO | 'Admin' | | 'Admin' / 'SuperAdmin' |
| Activo | BIT | NO | 1 | | Soft delete flag |
| FechaCreacion | DATETIME2 | NO | SYSDATETIME() | | Auditoría |

**Índices**:
- PK: `Id` (clustered)
- UNIQUE NONCLUSTERED: `Email`
- NONCLUSTERED: `IX_Administradores_Email` (búsqueda login)

### 2. Carreras

| Columna | Tipo | Null | Default | Constraints | Descripción |
|---------|------|------|---------|-------------|-------------|
| Id | INT | NO | IDENTITY(1,1) | PK | Identificador |
| Nombre | NVARCHAR(200) | NO | | | Nombre carrera |
| DuracionAnios | INT | NO | | CHECK (1-10) | Años de duración |
| Turno | NVARCHAR(50) | YES | NULL | | Mañana/Tarde/Noche |
| Modalidad | NVARCHAR(50) | YES | NULL | | Presencial/Virtual/Híbrida |
| Horario | NVARCHAR(100) | YES | NULL | | Ej: "Lun-Vie 18-22hs" |
| Estado | NVARCHAR(50) | NO | 'Activa' | | 'Activa'/'Inactiva' |
| FechaCreacion | DATETIME2 | NO | SYSDATETIME() | | Auditoría |

### 3. Alumnos

| Columna | Tipo | Null | Default | Constraints | Descripción |
|---------|------|------|---------|-------------|-------------|
| Id | INT | NO | IDENTITY(1,1) | PK | Identificador |
| Nombre | NVARCHAR(100) | NO | | | Nombre(s) |
| Apellido | NVARCHAR(100) | NO | | | Apellido(s) |
| Email | NVARCHAR(255) | NO | | | Contacto |
| DNI | INT | NO | | UNIQUE, CHECK(7-8 dígitos) | Documento único |
| FechaNacimiento | DATE | NO | | | Para calcular edad |
| Direccion | NVARCHAR(200) | YES | NULL | | Opcional |
| Nacionalidad | NVARCHAR(100) | YES | NULL | | Opcional |
| FechaInscripcion | DATETIME2 | NO | SYSDATETIME() | | Auto en CREATE |
| Telefono | NVARCHAR(50) | YES | NULL | | Opcional |
| TituloSecundario | NVARCHAR(200) | YES | NULL | | Opcional |
| Turno | NVARCHAR(50) | YES | NULL | | Mañana/Tarde/Noche |
| CarreraId | INT | NO | | FK → Carreras.Id | Obligatorio |
| FechaCreacion | DATETIME2 | NO | SYSDATETIME() | | Auditoría |

**Índices**:
- PK: `Id`
- UNIQUE NONCLUSTERED: `DNI`
- NONCLUSTERED: `IX_Alumnos_CarreraId` (joins, filtros)
- NONCLUSTERED: `IX_Alumnos_DNI` (búsqueda por documento)

**Foreign Key**:
- `FK_Alumnos_Carreras`: `CarreraId` → `Carreras(Id)` ON DELETE NO ACTION

### 4. Profesores

| Columna | Tipo | Null | Default | Constraints | Descripción |
|---------|------|------|---------|-------------|-------------|
| Id | INT | NO | IDENTITY(1,1) | PK | Identificador |
| Nombre | NVARCHAR(100) | NO | | | Nombre(s) |
| Apellido | NVARCHAR(100) | NO | | | Apellido(s) |
| Email | NVARCHAR(255) | NO | | UNIQUE | Contacto único |
| Telefono | NVARCHAR(50) | YES | NULL | | Opcional |
| Especialidad | NVARCHAR(100) | YES | NULL | | Área de enseñanza |
| FechaCreacion | DATETIME2 | NO | SYSDATETIME() | | Auditoría |

**Índices**:
- PK: `Id`
- UNIQUE NONCLUSTERED: `Email`
- NONCLUSTERED: `IX_Profesores_Email` (búsqueda)

### 5. Formularios

| Columna | Tipo | Null | Default | Constraints | Descripción |
|---------|------|------|---------|-------------|-------------|
| Id | INT | NO | IDENTITY(1,1) | PK | Identificador |
| Nombre | NVARCHAR(200) | NO | | | Nombre formulario |
| Estado | NVARCHAR(20) | NO | 'Borrador' | CHECK ('Borrador','Abierto','Cerrado') | Estado flujo |
| FechaApertura | DATETIME2 | NO | | | Inicio período |
| FechaCierre | DATETIME2 | NO | | | Fin período |
| Descripcion | NVARCHAR(1000) | YES | NULL | | Detalle opcional |
| FechaCreacion | DATETIME2 | NO | SYSDATETIME() | | Auditoría |

**Estados válidos**: `Borrador`, `Abierto`, `Cerrado`

## Reglas de Negocio a Nivel BD

1. **Soft Delete**: `Administradores.Activo = 0` en lugar de DELETE físico
2. **Cascada**: NO hay ON DELETE CASCADE (integridad manual en servicios)
3. **Unicidad**: Email único en Administradores y Profesores; DNI único en Alumnos
4. **Checks**: `DuracionAnios 1-10`, `DNI 7-8 dígitos`, `Estado IN ('Borrador','Abierto','Cerrado')`
5. **Defaults**: `Estado='Activa'`, `Activo=1`, `FechaCreacion=SYSDATETIME()`
6. **Foreign Keys**: `Alumnos.CarreraId` → `Carreras.Id` (NO ACTION)

## Script de Creación Completo

Ver: [`02-script-creacion.md`](./02-script-creacion.md) o archivo `Instituto.API/Database/CreateDatabase.sql`