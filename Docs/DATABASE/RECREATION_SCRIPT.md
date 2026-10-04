# Script para Recrear la Base desde Cero

> **Basado en `InstitutoDbContext.OnModelCreating` + scripts SQL oficiales**

---

## Orden de Ejecución

1. **Crear la base** (DDL completo abajo — tablas base + académicas + índices).
2. `Docs/sql/sp_stored_procedures.sql` — SPs base (incluye `sp_Listado_GetAll` versión antigua).
3. `Docs/sql/04_InfAcademica.sql` — **Sobrescribe** `sp_Listado_GetAll` con versión nueva + SPs académicas.

> El flag **`-I`** en `sqlcmd` activa `QUOTED_IDENTIFIER ON`, requerido por el índice UNIQUE filtrado de `Administradores`.

---

## DDL Completo (Todas las Tablas)

```sql
-- ============================================================
-- CREAR BASE
-- ============================================================
CREATE DATABASE InstitutoDB;
GO
USE InstitutoDB;
GO

-- ============================================================
-- TABLAS BASE
-- ============================================================

CREATE TABLE dbo.Carreras (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(200) NOT NULL,
    DuracionAnios INT NOT NULL,
    Turno         NVARCHAR(50) NULL,
    Modalidad     NVARCHAR(50) NULL,
    Horario       NVARCHAR(100) NULL,
    Estado        NVARCHAR(50) NULL DEFAULT 'Activa',
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE dbo.Administradores (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Apellido      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(150) NOT NULL,
    PasswordHash  NVARCHAR(255) NOT NULL,
    PasswordTemp  NVARCHAR(255) NULL,
    Role          NVARCHAR(50) NOT NULL DEFAULT 'Admin',
    Activo        BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE UNIQUE INDEX UQ_Administradores_Email_Activo
ON dbo.Administradores(Email)
WHERE Activo = 1;

CREATE TABLE dbo.Alumnos (
    Id               INT IDENTITY(1,1) PRIMARY KEY,
    Nombre           NVARCHAR(100) NOT NULL,
    Apellido         NVARCHAR(100) NOT NULL,
    Email            NVARCHAR(150) NOT NULL,
    DNI              INT NOT NULL,
    FechaNacimiento  DATETIME NOT NULL,
    Direccion        NVARCHAR(200) NULL,
    Nacionalidad     NVARCHAR(100) NULL,
    FechaInscripcion DATETIME NULL DEFAULT GETDATE(),
    Telefono         NVARCHAR(50) NULL,
    TituloSecundario NVARCHAR(200) NULL,
    Turno            NVARCHAR(50) NULL,
    CarreraId        INT NOT NULL,
    FechaCreacion    DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Alumnos_Carreras
        FOREIGN KEY (CarreraId) REFERENCES dbo.Carreras(Id)
);

CREATE UNIQUE INDEX UQ_Alumnos_Email ON dbo.Alumnos(Email);
CREATE UNIQUE INDEX UQ_Alumnos_DNI ON dbo.Alumnos(DNI);

CREATE TABLE dbo.Profesores (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Apellido      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(150) NOT NULL,
    Telefono      NVARCHAR(50) NULL,
    Especialidad  NVARCHAR(100) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE UNIQUE INDEX UQ_Profesores_Email ON dbo.Profesores(Email);

CREATE TABLE dbo.Formularios (
    Id            INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(200) NOT NULL,
    Estado        NVARCHAR(50) NOT NULL DEFAULT 'Borrador',
    FechaApertura DATETIME NOT NULL,
    FechaCierre   DATETIME NOT NULL,
    Descripcion   NVARCHAR(500) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);

-- ============================================================
-- TABLAS ACADÉMICAS (también en 04_InfAcademica.sql)
-- ============================================================

CREATE TABLE dbo.Inf_Academica (
    ID_Inf_Aca          INT IDENTITY(1,1) PRIMARY KEY,
    Inf_Aca_Descripcion NVARCHAR(100) NOT NULL,
    Inf_Aca_Fecha       DATE NOT NULL,
    Inf_Aca_Estado      VARCHAR(15) NOT NULL DEFAULT 'DESHABILITADO'
);

CREATE TABLE dbo.Inf_Academica_Est (
    ID_Inf_Academica_Est INT IDENTITY(1,1) PRIMARY KEY,
    ID_Inf_Aca           INT NOT NULL,
    ID_Est               INT NOT NULL,
    Fecha_Emision        DATE NOT NULL,
    Titulo_Secundario    NVARCHAR(100) NULL,
    Institucion          NVARCHAR(150) NULL,
    Estado_Titulo        VARCHAR(15) NOT NULL,
    CONSTRAINT FK_InfAcademicaEst_InfAcademica
        FOREIGN KEY (ID_Inf_Aca) REFERENCES dbo.Inf_Academica(ID_Inf_Aca),
    CONSTRAINT FK_InfAcademicaEst_Alumnos
        FOREIGN KEY (ID_Est) REFERENCES dbo.Alumnos(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_InfAcademicaEst_InfAca_Est
        UNIQUE (ID_Inf_Aca, ID_Est)
);

GO

-- ============================================================
-- USUARIO SQL
-- ============================================================
CREATE LOGIN instituto_user
WITH PASSWORD = 'Instituto2026', CHECK_POLICY = OFF, CHECK_EXPIRATION = OFF;
GO
CREATE USER instituto_user FOR LOGIN instituto_user;
ALTER ROLE db_owner ADD MEMBER instituto_user;
GO
```

---

## Comandos `sqlcmd` para Ejecutar

```bash
# 1. Ejecutar SPs base
sqlcmd -S localhost -U instituto_user -P Instituto2026 -d InstitutoDB -I -i "Docs/sql/sp_stored_procedures.sql"

# 2. Ejecutar SPs y tablas de info académica (sobrescribe sp_Listado_GetAll)
sqlcmd -S localhost -U instituto_user -P Instituto2026 -d InstitutoDB -I -i "Docs/sql/04_InfAcademica.sql"
```

---

## Verificación Post-Recreación

```sql
-- Verificar tablas (deben ser 7)
SELECT name FROM sys.tables ORDER BY name;

-- Verificar tablas académicas
SELECT COUNT(*) FROM sys.tables WHERE name LIKE '%Inf_Academ%';

-- Verificar SPs (deben ser ~40)
SELECT COUNT(*) FROM sys.procedures;

-- Verificar catálogo académico (4 registros)
SELECT * FROM dbo.Inf_Academica ORDER BY ID_Inf_Aca;

-- Verificar sp_Listado_GetAll versión nueva
EXEC sp_Listado_GetAll;
-- Debe devolver columnas: AlumnoId, NombreCompleto, DNI, Email, Carrera, Turno, 
-- FechaNacimiento, TipoAcademico, TituloSecundario, FechaEmision, EstadoTitulo
-- (SIN columna Edad)
```

---

## Datos de Prueba (Seeding)

Los scripts SQL insertan automáticamente:

**`sp_stored_procedures.sql`** → No incluye seeding (solo SPs).

**`04_InfAcademica.sql`** → Inserta catálogo académico (4 tipos):
```sql
INSERT INTO dbo.Inf_Academica (Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado)
VALUES 
    ('Título', CAST(GETDATE() AS DATE), 'HABILITADO'),
    ('Título en trámite', CAST(GETDATE() AS DATE), 'HABILITADO'),
    ('Constancia de materias adeudadas', CAST(GETDATE() AS DATE), 'HABILITADO'),
    ('Constancia de alumno regular', CAST(GETDATE() AS DATE), 'HABILITADO');
```

**Datos base (carreras, alumnos, admins, profesores, formularios):**
Deben insertarse manualmente o via API. Ver [DATA_SNAPSHOT.md](./DATA_SNAPSHOT.md) para valores de referencia.

---

## Crear Primer Admin (Solo Dev)

```bash
curl -X POST http://localhost:5127/api/setup/admin \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Super","apellido":"Admin","email":"admin@tupac.edu.ar","password":"Tupac123","role":"SuperAdmin"}'
```

---

## Mapeo EF Core ↔ BD (Referencia Rápida)

| Entidad C# | Tabla BD | PK C# | PK BD | Columnas Mapeadas (C# → BD) |
|------------|----------|-------|-------|-----------------------------|
| `Carrera` | `Carreras` | `Id` | `Id` | Directas |
| `Administrador` | `Administradores` | `Id` | `Id` | Directas + `PasswordTemp` |
| `Alumno` | `Alumnos` | `Id` | `Id` | Directas + `FechaInscripcion` default |
| `Profesor` | `Profesores` | `Id` | `Id` | Directas |
| `Formulario` | `Formularios` | `Id` | `Id` | Directas |
| `InfAcademica` | `Inf_Academica` | `Id` | `ID_Inf_Aca` | `Id→ID_Inf_Aca`, `Descripcion→Inf_Aca_Descripcion`, `Fecha→Inf_Aca_Fecha`, `Estado→Inf_Aca_Estado` |
| `InfAcademicaEst` | `Inf_Academica_Est` | `Id` | `ID_Inf_Academica_Est` | `Id→ID_Inf_Academica_Est`, `InfAcademicaId→ID_Inf_Aca`, `AlumnoId→ID_Est`, `FechaEmision→Fecha_Emision`, `TituloSecundario→Titulo_Secundario`, `Institucion→Institucion`, `EstadoTitulo→Estado_Titulo` |