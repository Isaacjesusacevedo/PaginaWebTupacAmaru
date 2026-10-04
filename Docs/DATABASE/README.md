# Base de Datos — InstitutoDB

> Última actualización: 2026-10-02
> Motor: SQL Server 2022
> Instancia: `localhost` (default)
> Usuario: `instituto_user` / `Instituto2026`
> Módulo Información Académica documentado en [`INFORMACION_ACADEMICA.md`](../INFORMACION_ACADEMICA.md)

---

## Índice de Documentación

| Archivo | Descripción |
|---------|-------------|
| [`TABLES.md`](./TABLES.md) | Inventario de tablas y estructura detallada por tabla |
| [`RELATIONS.md`](./RELATIONS.md) | Relaciones (Foreign Keys) y diagrama |
| [`STORED_PROCEDURES.md`](./STORED_PROCEDURES.md) | Inventario de Stored Procedures y reglas |
| [`LISTADO_GETALL.md`](./LISTADO_GETALL.md) | Reescritura de `sp_Listado_GetAll` |
| [`DATA_SNAPSHOT.md`](./DATA_SNAPSHOT.md) | Datos actuales (snapshot 2026-10-02) |
| [`RECREATION_SCRIPT.md`](./RECREATION_SCRIPT.md) | Script para recrear la base desde cero |
| [`MAINTENANCE_COMMANDS.md`](./MAINTENANCE_COMMANDS.md) | Comandos útiles de mantenimiento |
| [`LESSONS_LEARNED.md`](./LESSONS_LEARNED.md) | Lecciones aprendidas (bugs y fixes EF Core + SPs) |
| [`PASSWORDS.md`](./PASSWORDS.md) | Contraseñas de prueba (desarrollo) |
| [`STATISTICS.md`](./STATISTICS.md) | Estadísticas de la BD |
| [`CHANGELOG.md`](./CHANGELOG.md) | Historial de cambios de la BD |

---

## Resumen Rápido

| Métrica | Valor |
|---------|-------|
| Total tablas | 7 |
| Total columnas | 56 |
| Total registros (base) | 17 |
| Total registros (académica) | 4 (solo catálogo) |
| FKs definidas | 3 |
| Índices UNIQUE | 9 |
| Índices UNIQUE parciales | 1 (`UQ_Administradores_Email_Activo`) |
| Stored Procedures | ~40 (base + académicas) |

---

## Tablas

1. **Administradores** — Usuarios del sistema con soft delete
2. **Alumnos** — Estudiantes inscriptos
3. **Carreras** — Oferta académica
4. **Formularios** — Formularios de inscripción
5. **Profesores** — Docentes
6. **Inf_Academica** — Catálogo de tipos de información académica
7. **Inf_Academica_Est** — Registros académicos por alumno

---

*Documento mantenido por el equipo de desarrollo — Instituto Superior Docente Túpac Amaru (2026)*