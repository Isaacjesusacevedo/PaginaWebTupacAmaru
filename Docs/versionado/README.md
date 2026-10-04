# Carpeta de Versionado — Historial de Evolución del Proyecto

Esta carpeta documenta la evolución del sistema de gestión institucional a través de tres versiones principales:

---

## Versiones

| Versión | Fecha | Proyecto Fuente | Estado | Descripción |
|---------|-------|-----------------|--------|-------------|
| **0.0.1** | ~2024 | `Rodrigo_David_Raffo_Fabio_Final_Algoritmos_3` | ✅ Base | Proyecto académico inicial: Minimal API + Dapper, solo administradores, login + exportar Excel |
| **0.1.0** | 2026-09 | `instituto` | ✅ Intermedia | Migración a N-Tier (AD/BR/API), EF Core + SPs, 6 entidades, JWT, Vue 3 Frontend |
| **1.0.0** | 2026-10 | `PracticaProfesionalizante_2026-main` | 🚀 **Actual** | Separación en 2 Minimal APIs, módulo académico completo, inscripción pública, ~52 SPs |

---

## Archivos en esta Carpeta

| Archivo | Contenido |
|---------|-----------|
| `v0.0.1_rodrigo.md` | Análisis completo de la versión base (Rodrigo) |
| `v0.1.0_instituto.md` | Análisis completo de la versión intermedia (Instituto) |
| `v1.0.0_actual.md` | Análisis completo de la versión actual |
| `comparativa_cambios.md` | Tabla comparativa detallada entre versiones |
| `migraciones_resumen.md` | Resumen de migraciones y decisiones técnicas |

---

## Evolución Arquitectural

```
v0.0.1 (Rodrigo)          v0.1.0 (Instituto)           v1.0.0 (Actual)
─────────────────         ─────────────────            ─────────────────
Minimal API único         Minimal API único (API)      2 Minimal APIs
Dapper                    EF Core + SPs                EF Core + SPs (SpInvoker)
1 entidad (Admin)         6 entidades + ListadoItem    7 entidades + InfAcademica*
Sin autenticación real    JWT + BCrypt                 JWT + BCrypt + AdminPolicy
Sin Frontend              Vue 3 + Vite + Element Plus  Vue 3 (igual)
1 SP (Logueo_Admin)       ~15 SPs base                 ~52 SPs (base + académicas)
Sin BD relacional compleja  5 tablas + 1 FK           7 tablas + 3 FKs (CASCADE)
```

---

## Módulo Académico (Nuevo en v1.0.0)

El **gran salto** de v0.1.0 a v1.0.0 es la implementación completa del módulo de información académica:

- **Catálogo** (`Inf_Academica`): 4 tipos configurables (Título, Título en trámite, Constancias)
- **Registros por alumno** (`Inf_Academica_Est`): FK a alumno (CASCADE) + FK a catálogo
- **Inscripción pública**: Crea alumno + múltiples registros académicos en una transacción
- **Listado inteligente**: SP devuelve N filas/alumno → BR agrupa por prioridad (Título > Trámite > Constancias)
- **Validaciones centralizadas**: `InfAcademicaValidator` + `InfAcademicaConstants`

---

*Documentación generada: 2026-10-03*