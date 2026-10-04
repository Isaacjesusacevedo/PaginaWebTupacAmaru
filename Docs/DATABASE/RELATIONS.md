# Relaciones (Foreign Keys)

## FKs Definidas en BD (SQL)

| Nombre FK | Tabla Hija | Columna Hija | Tabla Padre | Columna Padre | ON DELETE | ON UPDATE | Definida En |
|-----------|------------|--------------|-------------|---------------|-----------|-----------|-------------|
| FK_Alumnos_Carreras | Alumnos | CarreraId | Carreras | Id | NO_ACTION (Restrict) | NO_ACTION | `sp_stored_procedures.sql` (DDL implícito) / `04_InfAcademica.sql` (tabla) |
| FK_InfAcademicaEst_InfAcademica | Inf_Academica_Est | ID_Inf_Aca | Inf_Academica | ID_Inf_Aca | NO_ACTION | NO_ACTION | `04_InfAcademica.sql` |
| FK_InfAcademicaEst_Alumnos | Inf_Academica_Est | ID_Est | Alumnos | Id | **CASCADE** | NO_ACTION | `04_InfAcademica.sql` |

---

## Mapeo en EF Core (`InstitutoDbContext.OnModelCreating`)

```csharp
// Alumnos → Carreras
entity.HasOne<Carrera>()
      .WithMany()
      .HasForeignKey(a => a.CarreraId)
      .OnDelete(DeleteBehavior.Restrict);  // NO_ACTION en BD

// InfAcademicaEst → InfAcademica (FK configurada solo en BD, no en EF navigation)
// InfAcademicaEst → Alumnos (FK configurada solo en BD, no en EF navigation)
```

**Nota:** Las FKs de `Inf_Academica_Est` **no tienen navigation properties** en las entidades C#. Se usan solo los IDs (`InfAcademicaId`, `AlumnoId`) y las validaciones de existencia se hacen en BR via repositorios.

---

## Diagrama de Relaciones

```
       
    CARRERAS                 ALUMNOS     
       
 PK Id                   PK Id           
 Nombre                  Nombre          
 DuracionAnios           Apellido        
 Turno                   Email           
 Modalidad               DNI             
 Horario                 FechaNacimiento 
 Estado                  Direccion       
 FechaCreacion           Nacionalidad    
        FechaInscripcion
          1:N             Telefono        
                          TituloSecundario
        Turno           
    (ninguna)            FK CarreraId 
        FechaCreacion   
                          
                                    1:N (CASCADE)
                                   
                    
                          Inf_Academica_Est     
                    
                     PK ID_Inf_Academica_Est    
                     FK ID_Inf_Aca ──────────┐ 
                     FK ID_Est (CASCADE)    │ 
                        Fecha_Emision        │ 
                        Titulo_Secundario    │ 
                        Institucion          │ 
                        Estado_Titulo        │ 
                    
                                                  
                                                  
                                    
                                       Inf_Academica      
                                       (catálogo)         
                                    
                                     PK ID_Inf_Aca        
                                        Inf_Aca_Descripcion
                                        Inf_Aca_Fecha     
                                        Inf_Aca_Estado    
                                    
```

---

## Descripción de Relaciones

### Carreras → Alumnos (1:N)
- **FK:** `FK_Alumnos_Carreras` en `Alumnos.CarreraId` → `Carreras.Id`
- Una carrera puede tener muchos alumnos
- Un alumno pertenece a una sola carrera
- **ON DELETE NO_ACTION / Restrict:** No se puede borrar una carrera si tiene alumnos asociados
- En EF: `DeleteBehavior.Restrict` (lanza excepción si se intenta borrar carrera con alumnos)

### Inf_Academica → Inf_Academica_Est (1:N)
- **FK:** `FK_InfAcademicaEst_InfAcademica` en `Inf_Academica_Est.ID_Inf_Aca` → `Inf_Academica.ID_Inf_Aca`
- Un tipo de información académica (catálogo) puede tener muchos registros
- Cada registro pertenece a un tipo del catálogo
- **ON DELETE NO_ACTION:** No se puede borrar un tipo si tiene registros (validado en BR via `Inf_Academica_Est_ExistsByInfAca`)

### Alumnos → Inf_Academica_Est (1:N con CASCADE)
- **FK:** `FK_InfAcademicaEst_Alumnos` en `Inf_Academica_Est.ID_Est` → `Alumnos.Id`
- Un alumno puede tener muchos registros académicos
- Cada registro pertenece a un solo alumno
- **ON DELETE CASCADE:** Al borrar un alumno, se borran sus registros académicos automáticamente en BD
- **Inmutable:** El `AlumnoId` (ID_Est) no se puede cambiar tras crear el registro (SP `Inf_Academica_Est_Update` no lo recibe, BR lo fuerza)

---

## Validaciones de Integridad en Business Rules

| Validación | Dónde | Método |
|------------|-------|--------|
| Carrera existe al crear alumno | `AlumnoService.CreateAsync` | `_carreraRepository.ExistsAsync(carreraId)` |
| Tipo académico existe y está habilitado | `InfAcademicaEstService.CreateAsync` / `InscripcionService.InscribirAsync` | `_infAcademicaRepository.GetByIdAsync` + `Estado == HABILITADO` |
| Alumno existe al crear registro académico | `InfAcademicaEstService.CreateAsync` | `_alumnoRepository.ExistsAsync(alumnoId)` |
| No duplicar tipo por alumno | `InfAcademicaEstService.CreateAsync` / `UpdateAsync` | `_repository.ExistsByCombinationAsync(infAcademicaId, alumnoId, excludeId?)` |
| No borrar tipo con registros | `InfAcademicaService.DeleteAsync` | `_infAcademicaEstRepository.ExistsByInfAcademicaAsync(id)` |
| Alumno inmutable en update | `InfAcademicaEstService.UpdateAsync` | `entity.AlumnoId = existente.AlumnoId` |

---

## Consultas Útiles para Verificar Relaciones

```sql
-- Ver FKs en BD
SELECT 
    fk.name AS FK_Name,
    tp.name AS Tabla_Padre,
    cp.name AS Columna_Padre,
    th.name AS Tabla_Hija,
    ch.name AS Columna_Hija,
    fk.delete_referential_action_desc AS On_Delete,
    fk.update_referential_action_desc AS On_Update
FROM sys.foreign_keys fk
INNER JOIN sys.tables tp ON fk.referenced_object_id = tp.object_id
INNER JOIN sys.columns cp ON fk.referenced_object_id = cp.object_id AND fk.key_index_id = cp.column_id
INNER JOIN sys.tables th ON fk.parent_object_id = th.object_id
INNER JOIN sys.columns ch ON fk.parent_object_id = ch.object_id AND fk.parent_column_id = ch.column_id
WHERE tp.name IN ('Carreras', 'Alumnos', 'Inf_Academica', 'Inf_Academica_Est')
ORDER BY fk.name;
```

```sql
-- Ver integridad: carreras sin alumnos (se pueden borrar)
SELECT c.Id, c.Nombre, COUNT(a.Id) AS TotalAlumnos
FROM Carreras c
LEFT JOIN Alumnos a ON a.CarreraId = c.Id
GROUP BY c.Id, c.Nombre
HAVING COUNT(a.Id) = 0;
```

```sql
-- Ver integridad: tipos académicos con registros (NO se pueden borrar)
SELECT ia.ID_Inf_Aca, ia.Inf_Aca_Descripcion, COUNT(iae.ID_Inf_Academica_Est) AS TotalRegistros
FROM Inf_Academica ia
LEFT JOIN Inf_Academica_Est iae ON iae.ID_Inf_Aca = ia.ID_Inf_Aca
GROUP BY ia.ID_Inf_Aca, ia.Inf_Aca_Descripcion
HAVING COUNT(iae.ID_Inf_Academica_Est) > 0;
```

```sql
-- Ver alumnos con sus registros académicos
SELECT a.Id, a.Nombre, a.Apellido, 
       ia.Inf_Aca_Descripcion AS Tipo,
       iae.Estado_Titulo,
       iae.Fecha_Emision
FROM Alumnos a
LEFT JOIN Inf_Academica_Est iae ON iae.ID_Est = a.Id
LEFT JOIN Inf_Academica ia ON ia.ID_Inf_Aca = iae.ID_Inf_Aca
ORDER BY a.Id, ia.Inf_Aca_Descripcion;
```