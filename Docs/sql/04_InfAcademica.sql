/*
 * Script de Información Académica e Inscripción
 * Instituto Superior Docente Túpac Amaru
 * Arquitectura: EF Core + Stored Procedures (SOLO Data Access)
 *
 * REGLAS DE ORO:
 * - SPs SOLO hacen Data Access (SELECT/INSERT/UPDATE/DELETE)
 * - CERO validaciones de negocio en SPs
 * - Validaciones de negocio QUEDAN en los Servicios (BR)
 * - SPs usan SET NOCOUNT ON al inicio
 * - SPs de INSERT usan SCOPE_IDENTITY() para devolver ID
 * - Script idempotente: puede ejecutarse varias veces sin duplicar nada
 */

USE InstitutoDB;
GO

-- ============================================================================
-- TABLAS
-- ============================================================================

IF OBJECT_ID('dbo.Inf_Academica', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inf_Academica (
        ID_Inf_Aca          INT IDENTITY(1,1) PRIMARY KEY,
        Inf_Aca_Descripcion NVARCHAR(100) NOT NULL,
        Inf_Aca_Fecha       DATE NOT NULL,
        Inf_Aca_Estado      VARCHAR(15) NOT NULL DEFAULT 'DESHABILITADO'
    );
END;
GO

IF OBJECT_ID('dbo.Inf_Academica_Est', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inf_Academica_Est (
        ID_Inf_Academica_Est INT IDENTITY(1,1) PRIMARY KEY,
        ID_Inf_Aca            INT NOT NULL,
        ID_Est                INT NOT NULL,
        Fecha_Emision         DATE NOT NULL,
        Titulo_Secundario     NVARCHAR(100) NULL,
        Institucion           NVARCHAR(150) NULL,
        Estado_Titulo         VARCHAR(15) NOT NULL,
        CONSTRAINT FK_InfAcademicaEst_InfAcademica FOREIGN KEY (ID_Inf_Aca)
            REFERENCES dbo.Inf_Academica(ID_Inf_Aca),
        CONSTRAINT FK_InfAcademicaEst_Alumnos FOREIGN KEY (ID_Est)
            REFERENCES dbo.Alumnos(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_InfAcademicaEst_InfAca_Est UNIQUE (ID_Inf_Aca, ID_Est)
    );
END;
GO

-- ============================================================================
-- DATOS INICIALES DEL CATÁLOGO
-- ============================================================================

IF NOT EXISTS (SELECT 1 FROM dbo.Inf_Academica WHERE Inf_Aca_Descripcion = 'Título')
BEGIN
    INSERT INTO dbo.Inf_Academica (Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado)
    VALUES ('Título', CAST(GETDATE() AS DATE), 'HABILITADO');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Inf_Academica WHERE Inf_Aca_Descripcion = 'Título en trámite')
BEGIN
    INSERT INTO dbo.Inf_Academica (Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado)
    VALUES ('Título en trámite', CAST(GETDATE() AS DATE), 'HABILITADO');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Inf_Academica WHERE Inf_Aca_Descripcion = 'Constancia de materias adeudadas')
BEGIN
    INSERT INTO dbo.Inf_Academica (Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado)
    VALUES ('Constancia de materias adeudadas', CAST(GETDATE() AS DATE), 'HABILITADO');
END;

IF NOT EXISTS (SELECT 1 FROM dbo.Inf_Academica WHERE Inf_Aca_Descripcion = 'Constancia de alumno regular')
BEGIN
    INSERT INTO dbo.Inf_Academica (Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado)
    VALUES ('Constancia de alumno regular', CAST(GETDATE() AS DATE), 'HABILITADO');
END;
GO

-- ============================================================================
-- SPs: INF_ACADEMICA (catálogo)
-- ============================================================================

CREATE OR ALTER PROCEDURE Inf_Academica_List
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID_Inf_Aca, Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado
    FROM dbo.Inf_Academica
    ORDER BY ID_Inf_Aca;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_GetById
    @ID_Inf_Aca INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID_Inf_Aca, Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado
    FROM dbo.Inf_Academica
    WHERE ID_Inf_Aca = @ID_Inf_Aca;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Insert
    @Inf_Aca_Descripcion NVARCHAR(100),
    @Inf_Aca_Fecha DATE,
    @Inf_Aca_Estado VARCHAR(15) = 'DESHABILITADO'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Inf_Academica (Inf_Aca_Descripcion, Inf_Aca_Fecha, Inf_Aca_Estado)
    VALUES (@Inf_Aca_Descripcion, @Inf_Aca_Fecha, @Inf_Aca_Estado);
    SELECT SCOPE_IDENTITY() AS ID_Inf_Aca;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Update
    @ID_Inf_Aca INT,
    @Inf_Aca_Descripcion NVARCHAR(100),
    @Inf_Aca_Fecha DATE,
    @Inf_Aca_Estado VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Inf_Academica
    SET Inf_Aca_Descripcion = @Inf_Aca_Descripcion,
        Inf_Aca_Fecha = @Inf_Aca_Fecha,
        Inf_Aca_Estado = @Inf_Aca_Estado
    WHERE ID_Inf_Aca = @ID_Inf_Aca;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Delete
    @ID_Inf_Aca INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Inf_Academica WHERE ID_Inf_Aca = @ID_Inf_Aca;
END;
GO

-- ============================================================================
-- SPs: INF_ACADEMICA_EST (información académica por alumno)
-- ============================================================================

CREATE OR ALTER PROCEDURE Inf_Academica_Est_List
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID_Inf_Academica_Est, ID_Inf_Aca, ID_Est, Fecha_Emision, Titulo_Secundario, Institucion, Estado_Titulo
    FROM dbo.Inf_Academica_Est
    ORDER BY ID_Inf_Academica_Est;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Est_GetById
    @ID_Inf_Academica_Est INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID_Inf_Academica_Est, ID_Inf_Aca, ID_Est, Fecha_Emision, Titulo_Secundario, Institucion, Estado_Titulo
    FROM dbo.Inf_Academica_Est
    WHERE ID_Inf_Academica_Est = @ID_Inf_Academica_Est;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Est_GetByAlumno
    @ID_Est INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID_Inf_Academica_Est, ID_Inf_Aca, ID_Est, Fecha_Emision, Titulo_Secundario, Institucion, Estado_Titulo
    FROM dbo.Inf_Academica_Est
    WHERE ID_Est = @ID_Est
    ORDER BY ID_Inf_Academica_Est;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Est_Insert
    @ID_Inf_Aca INT,
    @ID_Est INT,
    @Fecha_Emision DATE,
    @Titulo_Secundario NVARCHAR(100) = NULL,
    @Institucion NVARCHAR(150) = NULL,
    @Estado_Titulo VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Inf_Academica_Est (ID_Inf_Aca, ID_Est, Fecha_Emision, Titulo_Secundario, Institucion, Estado_Titulo)
    VALUES (@ID_Inf_Aca, @ID_Est, @Fecha_Emision, @Titulo_Secundario, @Institucion, @Estado_Titulo);
    SELECT SCOPE_IDENTITY() AS ID_Inf_Academica_Est;
END;
GO

-- El alumno (ID_Est) no se recibe ni se actualiza: una vez creado el registro, su alumno es inmutable.
CREATE OR ALTER PROCEDURE Inf_Academica_Est_Update
    @ID_Inf_Academica_Est INT,
    @ID_Inf_Aca INT,
    @Fecha_Emision DATE,
    @Titulo_Secundario NVARCHAR(100) = NULL,
    @Institucion NVARCHAR(150) = NULL,
    @Estado_Titulo VARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Inf_Academica_Est
    SET ID_Inf_Aca = @ID_Inf_Aca,
        Fecha_Emision = @Fecha_Emision,
        Titulo_Secundario = @Titulo_Secundario,
        Institucion = @Institucion,
        Estado_Titulo = @Estado_Titulo
    WHERE ID_Inf_Academica_Est = @ID_Inf_Academica_Est;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Est_Delete
    @ID_Inf_Academica_Est INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Inf_Academica_Est WHERE ID_Inf_Academica_Est = @ID_Inf_Academica_Est;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Est_Exists
    @ID_Inf_Academica_Est INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM dbo.Inf_Academica_Est WHERE ID_Inf_Academica_Est = @ID_Inf_Academica_Est
    ) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- @ExcludeId permite omitir el propio registro al validar duplicados en una actualización.
CREATE OR ALTER PROCEDURE Inf_Academica_Est_ExistsByCombination
    @ID_Inf_Aca INT,
    @ID_Est INT,
    @ExcludeId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM dbo.Inf_Academica_Est
        WHERE ID_Inf_Aca = @ID_Inf_Aca
          AND ID_Est = @ID_Est
          AND (@ExcludeId IS NULL OR ID_Inf_Academica_Est <> @ExcludeId)
    ) THEN 1 ELSE 0 END AS Exists;
END;
GO

CREATE OR ALTER PROCEDURE Inf_Academica_Est_ExistsByInfAca
    @ID_Inf_Aca INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CASE WHEN EXISTS(
        SELECT 1 FROM dbo.Inf_Academica_Est WHERE ID_Inf_Aca = @ID_Inf_Aca
    ) THEN 1 ELSE 0 END AS Exists;
END;
GO

-- ============================================================================
-- TABLA: LISTADO (reescritura — JOIN Alumnos + Carreras + Inf_Academica_Est + Inf_Academica)
-- Una fila por cada registro académico del alumno (o una fila con NULLs si no tiene ninguno).
-- No calcula edad ni decide qué fila mostrar: eso es responsabilidad de la BR.
-- ============================================================================

CREATE OR ALTER PROCEDURE sp_Listado_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        a.Id AS AlumnoId,
        a.Nombre + ' ' + a.Apellido AS NombreCompleto,
        a.DNI,
        a.Email,
        c.Nombre AS Carrera,
        a.Turno,
        a.FechaNacimiento,
        ia.Inf_Aca_Descripcion AS TipoAcademico,
        iae.Titulo_Secundario AS TituloSecundario,
        iae.Fecha_Emision AS FechaEmision,
        iae.Estado_Titulo AS EstadoTitulo
    FROM dbo.Alumnos a
    LEFT JOIN dbo.Carreras c ON a.CarreraId = c.Id
    LEFT JOIN dbo.Inf_Academica_Est iae ON iae.ID_Est = a.Id
    LEFT JOIN dbo.Inf_Academica ia ON ia.ID_Inf_Aca = iae.ID_Inf_Aca
    ORDER BY a.Id;
END;
GO

-- ============================================================================
-- FIN DEL SCRIPT
-- ============================================================================

PRINT 'Información académica: tablas, catálogo y Stored Procedures creados/actualizados correctamente.';
GO
