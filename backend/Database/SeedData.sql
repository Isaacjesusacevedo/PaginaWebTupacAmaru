USE InstitutoDB;
GO

-- =============================================
-- LIMPIAR TABLAS (orden inverso por FKs)
-- =============================================
DELETE FROM Formularios;
DELETE FROM Alumnos;
DELETE FROM Profesores;
DELETE FROM Carreras;
DELETE FROM Administradores WHERE Id > 1; -- Mantener el admin original (Id=1)
GO

-- Resetear identity seeds
DBCC CHECKIDENT ('Administradores', RESEED, 1);
DBCC CHECKIDENT ('Carreras', RESEED, 0);
DBCC CHECKIDENT ('Alumnos', RESEED, 0);
DBCC CHECKIDENT ('Profesores', RESEED, 0);
DBCC CHECKIDENT ('Formularios', RESEED, 0);
GO

-- =============================================
-- 1. ADMINISTRADORES (10 total, password: Password123)
-- =============================================
INSERT INTO Administradores (Nombre, Apellido, Email, PasswordHash, Role, Activo, FechaCreacion) VALUES
('Isa', 'Admin', 'isa@test.com', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'SuperAdmin', 1, SYSDATETIME()),
('María', 'González', 'maria.gonzalez@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Admin', 1, SYSDATETIME()),
('Carlos', 'Rodríguez', 'carlos.rodriguez@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Admin', 1, SYSDATETIME()),
('Laura', 'Martínez', 'laura.martinez@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Admin', 1, SYSDATETIME()),
('Roberto', 'López', 'roberto.lopez@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'SuperAdmin', 1, SYSDATETIME()),
('Ana', 'Fernández', 'ana.fernandez@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Admin', 1, SYSDATETIME()),
('Diego', 'Sánchez', 'diego.sanchez@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Admin', 1, SYSDATETIME()),
('Sofía', 'Romero', 'sofia.romero@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Admin', 1, SYSDATETIME()),
('Javier', 'Torres', 'javier.torres@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Admin', 1, SYSDATETIME()),
('Lucía', 'Díaz', 'lucia.diaz@tupac.edu.ar', '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewdBPj/RK.PZvO.S', 'Admin', 1, SYSDATETIME());
GO

-- =============================================
-- 2. CARRERAS (10 inserts - con acentos correctos UTF-8)
-- =============================================
INSERT INTO Carreras (Nombre, DuracionAnios, Turno, Modalidad, Horario, Estado, FechaCreacion) VALUES
(N'Tecnicatura Superior en Desarrollo de Software', 3, N'Noche', N'Presencial', N'18:30 a 22:30', N'Activa', SYSDATETIME()),
(N'Tecnicatura Superior en Enfermería', 3, N'Mañana', N'Presencial', N'08:00 a 13:00', N'Activa', SYSDATETIME()),
(N'Tecnicatura Superior en Administración de Empresas', 3, N'Tarde', N'Mixta', N'14:00 a 18:30', N'Activa', SYSDATETIME()),
(N'Profesorado en Educación Inicial', 4, N'Mañana', N'Presencial', N'08:00 a 13:30', N'Activa', SYSDATETIME()),
(N'Tecnicatura Superior en Turismo', 3, N'Tarde', N'Virtual', N'15:00 a 20:00', N'Activa', SYSDATETIME()),
(N'Tecnicatura Superior en Marketing Digital', 2, N'Noche', N'Virtual', N'19:00 a 22:00', N'Activa', SYSDATETIME()),
(N'Tecnicatura Superior en Recursos Humanos', 2, N'Mañana', N'Mixta', N'09:00 a 13:00', N'Activa', SYSDATETIME()),
(N'Profesorado en Educación Primaria', 4, N'Mañana', N'Presencial', N'08:00 a 13:30', N'Activa', SYSDATETIME()),
(N'Tecnicatura Superior en Diseño Gráfico', 3, N'Tarde', N'Presencial', N'14:00 a 18:30', N'Activa', SYSDATETIME()),
(N'Tecnicatura Superior en Seguridad Informática', 3, N'Noche', N'Virtual', N'18:30 a 22:30', N'Activa', SYSDATETIME());
GO

-- =============================================
-- 3. ALUMNOS (10 inserts - con acentos correctos)
-- =============================================
INSERT INTO Alumnos (Nombre, Apellido, Email, DNI, FechaNacimiento, Direccion, Nacionalidad, FechaInscripcion, Telefono, TituloSecundario, Turno, CarreraId, FechaCreacion) VALUES
(N'Juan', N'Pérez', N'juan.perez@alumno.tupac.edu.ar', 35123456, '1998-05-15', N'Av. Rivadavia 1234', N'Argentina', SYSDATETIME(), N'11-5555-1111', N'Bachiller en Ciencias Naturales', N'Noche', 1, SYSDATETIME()),
(N'María', N'García', N'maria.garcia@alumno.tupac.edu.ar', 36234567, '1999-08-22', N'Calle Corrientes 567', N'Argentina', SYSDATETIME(), N'11-5555-2222', N'Bachiller en Economía', N'Mañana', 2, SYSDATETIME()),
(N'Pedro', N'López', N'pedro.lopez@alumno.tupac.edu.ar', 37345678, '2000-01-10', N'Av. Santa Fe 890', N'Argentina', SYSDATETIME(), N'11-5555-3333', N'Bachiller en Informática', N'Tarde', 3, SYSDATETIME()),
(N'Ana', N'Martínez', N'ana.martinez@alumno.tupac.edu.ar', 38456789, '1997-11-03', N'Calle Lavalle 456', N'Argentina', SYSDATETIME(), N'11-5555-4444', N'Bachiller en Humanidades', N'Mañana', 4, SYSDATETIME()),
(N'Luis', N'Rodríguez', N'luis.rodriguez@alumno.tupac.edu.ar', 39567890, '1998-03-28', N'Av. Callao 789', N'Argentina', SYSDATETIME(), N'11-5555-5555', N'Bachiller en Turismo', N'Tarde', 5, SYSDATETIME()),
(N'Carla', N'Fernández', N'carla.fernandez@alumno.tupac.edu.ar', 40678901, '1999-07-19', N'Calle Sarmiento 234', N'Argentina', SYSDATETIME(), N'11-5555-6666', N'Bachiller en Marketing', N'Noche', 6, SYSDATETIME()),
(N'Sergio', N'González', N'sergio.gonzalez@alumno.tupac.edu.ar', 41789012, '2000-02-14', N'Av. Belgrano 567', N'Argentina', SYSDATETIME(), N'11-5555-7777', N'Bachiller en Administración', N'Mañana', 7, SYSDATETIME()),
(N'Valentina', N'Pérez', N'valentina.perez@alumno.tupac.edu.ar', 42890123, '1998-09-05', N'Calle Mitre 890', N'Argentina', SYSDATETIME(), N'11-5555-8888', N'Bachiller en Educación', N'Mañana', 8, SYSDATETIME()),
(N'Facundo', N'López', N'facundo.lopez@alumno.tupac.edu.ar', 43901234, '1999-12-20', N'Av. Pueyrredón 123', N'Argentina', SYSDATETIME(), N'11-5555-9999', N'Bachiller en Arte', N'Tarde', 9, SYSDATETIME()),
(N'Camila', N'Torres', N'camila.torres@alumno.tupac.edu.ar', 44012345, '1997-06-30', N'Calle San Martín 456', N'Argentina', SYSDATETIME(), N'11-5555-0000', N'Bachiller en Ciencias', N'Noche', 10, SYSDATETIME());
GO

-- =============================================
-- 4. PROFESORES (10 inserts - con acentos correctos)
-- =============================================
INSERT INTO Profesores (Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion) VALUES
(N'Dr. Ricardo', N'Morales', N'ricardo.morales@tupac.edu.ar', N'11-4444-1111', N'Programación y Algoritmos', SYSDATETIME()),
(N'Dra. Patricia', N'Silva', N'patricia.silva@tupac.edu.ar', N'11-4444-2222', N'Enfermería Clínica', SYSDATETIME()),
(N'Mg. Fernando', N'Rojas', N'fernando.rojas@tupac.edu.ar', N'11-4444-3333', N'Administración Estratégica', SYSDATETIME()),
(N'Prof. Gabriela', N'Herrera', N'gabriela.herrera@tupac.edu.ar', N'11-4444-4444', N'Pedagogía y Didáctica', SYSDATETIME()),
(N'Lic. Martín', N'Castro', N'martin.castro@tupac.edu.ar', N'11-4444-5555', N'Turismo Sostenible', SYSDATETIME()),
(N'Mg. Claudia', N'Vargas', N'claudia.vargas@tupac.edu.ar', N'11-4444-5555', N'Marketing Digital y Redes Sociales', SYSDATETIME()),
(N'Dr. Alejandro', N'Mendoza', N'alejandro.mendoza@tupac.edu.ar', N'11-4444-7777', N'Gestión de Talento Humano', SYSDATETIME()),
(N'Prof. Natalia', N'Ortiz', N'natalia.ortiz@tupac.edu.ar', N'11-4444-8888', N'Psicología Educativa', SYSDATETIME()),
(N'Lic. Esteban', N'Ramírez', N'esteban.ramirez@tupac.edu.ar', N'11-4444-9999', N'Diseño Visual y UX', SYSDATETIME()),
(N'Ing. Sofía', N'Jiménez', N'sofia.jimenez@tupac.edu.ar', N'11-4444-0000', N'Ciberseguridad y Redes', SYSDATETIME());
GO

-- =============================================
-- 5. FORMULARIOS (10 inserts - con acentos correctos UTF-8)
-- =============================================
INSERT INTO Formularios (Nombre, Estado, FechaApertura, FechaCierre, Descripcion, FechaCreacion) VALUES
(N'Inscripción a Carreras 2026 - 1er Cuatrimestre', N'Abierto', '2025-12-01', '2026-03-31', N'Formulario oficial de inscripción para el primer cuatrimestre 2026. Incluye todas las carreras disponibles.', SYSDATETIME()),
(N'Solicitud de Beca Estudiantil 2026', N'Abierto', '2026-01-15', '2026-04-30', N'Formulario para solicitud de becas académicas, deportivas y socioeconómicas.', SYSDATETIME()),
(N'Reinscripción Anual 2026', N'Borrador', '2026-02-01', '2026-03-15', N'Formulario de reinscripción para alumnos regulares. Se habilita en febrero.', SYSDATETIME()),
(N'Solicitud de Certificado de Estudios', N'Abierto', '2026-01-01', '2026-12-31', N'Formulario para solicitar certificados de estudios en curso, analíticos y títulos.', SYSDATETIME()),
(N'Encuesta de Satisfacción Estudiantil 2026', N'Cerrado', '2025-11-01', '2025-12-20', N'Encuesta anual de satisfacción sobre servicios académicos y administrativos.', SYSDATETIME()),
(N'Inscripción a Talleres Extracurriculares 2026', N'Abierto', '2026-03-01', '2026-05-31', N'Formulario de inscripción a talleres de idiomas, deportes, arte y tecnología.', SYSDATETIME()),
(N'Solicitud de Prácticas Profesionales', N'Abierto', '2026-02-15', '2026-06-30', N'Formulario para gestionar convenios de prácticas profesionalizantes en empresas.', SYSDATETIME()),
(N'Evaluación Docente por Estudiantes 2026', N'Borrador', '2026-05-01', '2026-06-30', N'Evaluación anónima de docentes por parte de estudiantes. Se habilita en mayo.', SYSDATETIME()),
(N'Solicitud de Equivalencias y Reconocimientos', N'Abierto', '2026-01-01', '2026-12-31', N'Formulario para solicitar equivalencias de materias cursadas en otras instituciones.', SYSDATETIME()),
(N'Inscripción a Jornadas de Ingreso 2026', N'Abierto', '2025-10-01', '2026-02-28', N'Formulario de inscripción a las jornadas de ingreso obligatorias para nuevos ingresantes.', SYSDATETIME());
GO

PRINT 'Datos de ejemplo insertados correctamente (10 por tabla) con encoding UTF-8 correcto.';
GO