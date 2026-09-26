-- 1. Crear la base de datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InstitutoDB')
BEGIN
    CREATE DATABASE InstitutoDB;
END
GO

USE InstitutoDB;
GO

-- 2. Tabla Administradores
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Administradores' AND xtype = 'U')
BEGIN
    CREATE TABLE Administradores (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Apellido NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'Admin',
        Activo BIT NOT NULL DEFAULT 1,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
    
    CREATE INDEX IX_Administradores_Email ON Administradores(Email);
END
GO

-- 3. Tabla Carreras
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Carreras' AND xtype = 'U')
BEGIN
    CREATE TABLE Carreras (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(200) NOT NULL,
        DuracionAnios INT NOT NULL CHECK (DuracionAnios BETWEEN 1 AND 10),
        Turno NVARCHAR(50) NULL,
        Modalidad NVARCHAR(50) NULL,
        Horario NVARCHAR(100) NULL,
        Estado NVARCHAR(50) NOT NULL DEFAULT 'Activa',
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END
GO

-- 4. Tabla Alumnos
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Alumnos' AND xtype = 'U')
BEGIN
    CREATE TABLE Alumnos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Apellido NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        DNI INT NOT NULL UNIQUE CHECK (DNI BETWEEN 1000000 AND 99999999),
        FechaNacimiento DATE NOT NULL,
        Direccion NVARCHAR(200) NULL,
        Nacionalidad NVARCHAR(100) NULL,
        FechaInscripcion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        Telefono NVARCHAR(50) NULL,
        TituloSecundario NVARCHAR(200) NULL,
        Turno NVARCHAR(50) NULL,
        CarreraId INT NOT NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        
        CONSTRAINT FK_Alumnos_Carreras FOREIGN KEY (CarreraId) REFERENCES Carreras(Id)
    );
    
    CREATE INDEX IX_Alumnos_CarreraId ON Alumnos(CarreraId);
    CREATE INDEX IX_Alumnos_DNI ON Alumnos(DNI);
END
GO

-- 5. Tabla Profesores
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Profesores' AND xtype = 'U')
BEGIN
    CREATE TABLE Profesores (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(100) NOT NULL,
        Apellido NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        Telefono NVARCHAR(50) NULL,
        Especialidad NVARCHAR(100) NULL,
        FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
    
    CREATE INDEX IX_Profesores_Email ON Profesores(Email);
END
GO

PRINT 'Base de datos InstitutoDB y tablas creadas exitosamente.';