# Comandos Útiles de Mantenimiento

> Basados en la implementación real de repositorios y servicios.

---

## Administradores

**Ver todos los admins (activos e inactivos):**
```sql
SELECT Id, Nombre, Apellido, Email, Role, Activo, FechaCreacion
FROM dbo.Administradores
ORDER BY Id;
```

**Solo admins activos (los que ve el frontend / SP GetAll):**
```sql
SELECT Id, Nombre, Apellido, Email, Role
FROM dbo.Administradores
WHERE Activo = 1
ORDER BY Id;
```

**Ver admin por email (incluye PasswordHash para auth):**
```sql
SELECT Id, Nombre, Apellido, Email, Role, Activo, FechaCreacion, PasswordHash, PasswordTemp
FROM dbo.Administradores
WHERE Email = 'admin@tupac.edu.ar';
```

**Restaurar un admin soft-deleted:**
```sql
UPDATE dbo.Administradores SET Activo = 1 WHERE Id = 16;
```

**Borrar definitivamente los inactivos:**
```sql
DELETE FROM dbo.Administradores WHERE Activo = 0;
```

**Verificar índices UNIQUE de una tabla:**
```sql
SELECT i.name, i.is_unique, i.has_filter, i.filter_definition
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID('dbo.Administradores') AND i.is_unique = 1;
```

---

## Alumnos

**Ver alumno con carrera (join):**
```sql
SELECT a.Id, a.Nombre, a.Apellido, a.Email, a.DNI, a.Turno,
       c.Nombre AS Carrera, c.Id AS CarreraId
FROM dbo.Alumnos a
LEFT JOIN dbo.Carreras c ON a.CarreraId = c.Id
ORDER BY a.Id;
```

**Verificar unicidad Email/DNI antes de insertar:**
```sql
-- Email
SELECT COUNT(*) FROM dbo.Alumnos WHERE Email = 'nuevo@email.com';
-- DNI
SELECT COUNT(*) FROM dbo.Alumnos WHERE DNI = 12345678;
```

---

## Información Académica

**Ver todo el catálogo académico:**
```sql
SELECT * FROM dbo.Inf_Academica ORDER BY ID_Inf_Aca;
```

**Ver catálogo solo habilitados (lo que usa la inscripción pública):**
```sql
SELECT * FROM dbo.Inf_Academica 
WHERE Inf_Aca_Estado = 'HABILITADO' 
ORDER BY ID_Inf_Aca;
```

**Ver registros académicos de un alumno (con descripción del tipo):**
```sql
SELECT iae.*, ia.Inf_Aca_Descripcion
FROM dbo.Inf_Academica_Est iae
INNER JOIN dbo.Inf_Academica ia ON ia.ID_Inf_Aca = iae.ID_Inf_Aca
WHERE iae.ID_Est = 1
ORDER BY iae.Fecha_Emision DESC;
```

**Ver qué tipos tienen alumnos asignados (para saber cuáles NO se pueden borrar):**
```sql
SELECT ia.ID_Inf_Aca, ia.Inf_Aca_Descripcion, COUNT(iae.ID_Inf_Academica_Est) AS Total
FROM dbo.Inf_Academica ia
LEFT JOIN dbo.Inf_Academica_Est iae ON iae.ID_Inf_Aca = ia.ID_Inf_Aca
GROUP BY ia.ID_Inf_Aca, ia.Inf_Aca_Descripcion
ORDER BY ia.ID_Inf_Aca;
```

**Ver alumnos SIN información académica:**
```sql
SELECT a.Id, a.Nombre, a.Apellido, a.DNI
FROM dbo.Alumnos a
LEFT JOIN dbo.Inf_Academica_Est iae ON iae.ID_Est = a.Id
WHERE iae.ID_Inf_Academica_Est IS NULL
ORDER BY a.Id;
```

**Ver duplicados potenciales (mismo tipo + mismo alumno):**
```sql
SELECT ID_Inf_Aca, ID_Est, COUNT(*) AS Duplicados
FROM dbo.Inf_Academica_Est
GROUP BY ID_Inf_Aca, ID_Est
HAVING COUNT(*) > 1;
```

---

## Carreras

**Ver carreras con conteo de alumnos:**
```sql
SELECT c.Id, c.Nombre, c.Estado, COUNT(a.Id) AS TotalAlumnos
FROM dbo.Carreras c
LEFT JOIN dbo.Alumnos a ON a.CarreraId = c.Id
GROUP BY c.Id, c.Nombre, c.Estado
ORDER BY c.Id;
```

**Carreras sin alumnos (se pueden borrar):**
```sql
SELECT c.Id, c.Nombre
FROM dbo.Carreras c
LEFT JOIN dbo.Alumnos a ON a.CarreraId = c.Id
WHERE a.Id IS NULL;
```

---

## Profesores

**Ver todos los profesores:**
```sql
SELECT Id, Nombre, Apellido, Email, Telefono, Especialidad, FechaCreacion
FROM dbo.Profesores
ORDER BY Id;
```

**Verificar email único:**
```sql
SELECT COUNT(*) FROM dbo.Profesores WHERE Email = 'profesor@email.com';
```

---

## Formularios

**Ver formularios con estado:**
```sql
SELECT Id, Nombre, Estado, FechaApertura, FechaCierre, Descripcion
FROM dbo.Formularios
ORDER BY Id;
```

**Formularios abiertos actualmente:**
```sql
SELECT * FROM dbo.Formularios
WHERE Estado = 'Abierto' 
  AND FechaApertura <= GETDATE() 
  AND FechaCierre >= GETDATE();
```

---

## Listado General (SP)

**Ejecutar la SP del listado (versión nueva con info académica):**
```sql
EXEC sp_Listado_GetAll;
```

**Resultado: N filas por alumno (una por registro académico).**
Procesar en C# con `ListadoService` para agrupar y elegir prioridad.

**Nota:** El endpoint `GET /api/listado` en la **API Académica (puerto 5128)** es **público (AllowAnonymous)**. Se puede consumir directamente desde el frontend sin JWT usando `useAcademicaApi.academicaGet('/api/listado')`.

---

## Verificación General

**Verificar SPs creadas:**
```sql
SELECT name FROM sys.procedures
WHERE name LIKE 'sp_%' OR name LIKE 'Inf_Academica%'
ORDER BY name;
```

**Verificar versión de sp_Listado_GetAll:**
```sql
SELECT OBJECT_DEFINITION(OBJECT_ID('sp_Listado_GetAll'));
-- Debe contener: LEFT JOIN dbo.Inf_Academica_Est, LEFT JOIN dbo.Inf_Academica
-- NO debe contener: Edad, FLOOR(DATEDIFF
```

**Contar registros por tabla:**
```sql
SELECT 
    'Carreras' AS Tabla, COUNT(*) AS Registros FROM Carreras
UNION ALL SELECT 'Administradores', COUNT(*) FROM Administradores
UNION ALL SELECT 'Alumnos', COUNT(*) FROM Alumnos
UNION ALL SELECT 'Profesores', COUNT(*) FROM Profesores
UNION ALL SELECT 'Formularios', COUNT(*) FROM Formularios
UNION ALL SELECT 'Inf_Academica', COUNT(*) FROM Inf_Academica
UNION ALL SELECT 'Inf_Academica_Est', COUNT(*) FROM Inf_Academica_Est
ORDER BY Tabla;
```

---

## Limpieza de Datos de Prueba (Pre-Producción)

```sql
-- Borrar admins de prueba (soft delete inactivos)
DELETE FROM dbo.Administradores WHERE Activo = 0;

-- Borrar alumnos de prueba
DELETE FROM dbo.Alumnos WHERE DNI IN (40123456, 41234567, 42345678, 43456789);

-- Borrar profesores de prueba
DELETE FROM dbo.Profesores WHERE Email LIKE '%@tupac.edu.ar';

-- Borrar formularios de prueba
DELETE FROM dbo.Formularios WHERE Nombre LIKE 'Inscripción 2026%';

-- Borrar registros académicos de prueba (si los hay)
DELETE FROM dbo.Inf_Academica_Est;

-- Resetear identity (opcional, solo si BD vacía)
-- DBCC CHECKIDENT ('Carreras', RESEED, 0);
-- DBCC CHECKIDENT ('Alumnos', RESEED, 0);
-- etc.
```

---

## Debugging: Ver Qué SP Ejecuta el Código

**AlumnoRepository.GetByIdAsync:**
```sql
EXEC sp_Alumnos_GetById @Id = 1;
```

**AdministradorRepository.GetByEmailAsync:**
```sql
EXEC sp_Administradores_GetByEmail @Email = 'admin@tupac.edu.ar';
```

**InfAcademicaEstRepository.GetByAlumnoAsync:**
```sql
EXEC Inf_Academica_Est_GetByAlumno @ID_Est = 1;
```

**ListadoRepository.GetListadoAsync:**
```sql
EXEC sp_Listado_GetAll;
```

**Inscripción pública (flujo completo):**
```sql
-- 1. Crear alumno
EXEC sp_Alumnos_Create @Nombre='Test', @Apellido='User', @Email='test@test.com', 
    @DNI=99999999, @FechaNacimiento='2000-01-01', @CarreraId=1;

-- 2. Insertar registros académicos (repetir por cada tipo)
EXEC Inf_Academica_Est_Insert @ID_Inf_Aca=1, @ID_Est=999, @Fecha_Emision='2024-01-01', 
    @Titulo_Secundario='Bachiller', @Estado_Titulo='MANO';
```