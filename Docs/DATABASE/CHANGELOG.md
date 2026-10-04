# Historial de Cambios de la BD

| Fecha | Cambio | Autor |
|-------|--------|-------|
| 2026-09-27 | Creación de la BD con 5 tablas base | Equipo |
| 2026-09-27 | Habilitado **Mixed Mode Auth** en SQL Server | Equipo |
| 2026-09-27 | Creado usuario SQL `instituto_user` con `db_owner` | Equipo |
| 2026-09-27 | Implementado **soft delete** en Administradores | Equipo |
| 2026-09-27 | Reemplazado UNIQUE total por **UNIQUE parcial** (`WHERE Activo = 1`) | Equipo |
| 2026-09-27 | Datos de prueba insertados (4 carreras, 4 alumnos, 3 profesores, 2 formularios) | Equipo |
| 2026-09-28 | Documentación actualizada (diagrama ASCII, contraseñas, estructura interfaces) | Equipo |
| 2026-10-02 | **Módulo Información Académica**: tablas `Inf_Academica` + `Inf_Academica_Est`, 14 SPs nuevas, `sp_Listado_GetAll` reescrito con join académico | Equipo |
| 2026-10-02 | **Fix bugs integración EF Core + SPs**: `PasswordTemp`, alias `Result`, `ToListAsync()` sobre EXEC, `@FechaInscripcion` duplicado, doble `ExecuteScalarAsync()` | Equipo |
| 2026-10-02 | Documentación consolidada: 7 tablas, 3 FKs, ~40 SPs, sección de Lecciones Aprendidas | Equipo |
| 2026-10-02 | Reorganización de `DATABASE.md` en carpeta `DATABASE/` con 11 archivos temáticos | Equipo |
| 2026-10-02 | **Actualización completa de documentación** desde código real (modelos, repos, servicios, SPs, DbContext) | Equipo |
| 2026-10-02 | Agregado mapeo EF detallado en TABLES.md (columnas, mapeos OnModelCreating) | Equipo |
| 2026-10-02 | Documentada discrepancia de `sp_Listado_GetAll` (2 versiones en scripts SQL) | Equipo |
| 2026-10-02 | Documentados patrones de repositorios (Patrón A: ADO.NET directo, Patrón B: SpInvoker) | Equipo |
| 2026-10-02 | Agregado SpInvoker.cs como helper centralizado para invocación SPs | Equipo |
| 2026-10-02 | Documentadas validaciones BR centralizadas en InfAcademicaValidator | Equipo |
| 2026-10-02 | Actualizado RECREATION_SCRIPT.md con DDL completo desde OnModelCreating | Equipo |
| 2026-10-02 | Actualizado STATISTICS.md con conteos reales de entidades, SPs, DTOs | Equipo |
| 2026-10-03 | **Fix Frontend**: Verificación de contraseña en EditarAdministradorView.vue usa admin logueado (useAuth.getAdmin()) en lugar del admin a editar | Equipo |
| 2026-10-03 | **API Académica**: Endpoint `GET /api/listado` ahora público (AllowAnonymous) para consumo directo desde frontend | Equipo |
| 2026-10-03 | **API Académica**: CORS configurado para AllowAnyOrigin en desarrollo | Equipo |
| 2026-10-03 | **Frontend**: `useAcademicaApi.js` agregado manejo 401/403 con redirect a login + inyección router | Equipo |
| 2026-10-03 | **Frontend**: `ListadoView.vue` consume `academicaGet('/api/listado')` (API académica 5128) | Equipo |

---

## Próximos Cambios Planificados

| Fecha Estimada | Cambio | Prioridad |
|----------------|--------|-----------|
| Próximo sprint | Corregir alias `AS Exists` → `AS Result` en todas las SPs `*_Exists*` | Media |
| Próximo sprint | Agregar `PasswordTemp` a `sp_Administradores_GetById` y `GetByEmail` | Alta |
| Próximo sprint | Navigation properties en EF para `InfAcademicaEst` → `Alumno` / `InfAcademica` | Media |
| Futuro | Migración a EF Core Migrations (desde scripts SQL manuales) | Baja |
| Futuro | Tests de integración para repositorios académicos | Media |

---

*Documento mantenido por el equipo de desarrollo — Instituto Superior Docente Túpac Amaru (2026)*