---
theme: default
title: Sistema de Gestión Institucional
info: Instituto Superior Docente Túpac Amaru
class: text-center
highlighter: shiki
lineNumbers: true
transition: slide-left
mdc: true
colorSchema: dark
---

# Sistema de Gestión Institucional

**Instituto Superior Docente Túpac Amaru**

Plataforma de gestión académica

<div class="pt-12">
  <span class="px-2 py-1 rounded cursor-pointer" hover="bg-white bg-opacity-10">
    Presioná <kbd>espacio</kbd> para navegar →
  </span>
</div>

<!--
Portada. Stack: Vue 3 + ASP.NET Core 9 + SQL Server.
Duración estimada: 20-25 minutos.
-->

---
layout: default
---

# Agenda

1. Concepto del sistema
2. Arquitectura N-Tier
3. El viaje de un request
4. Autenticación JWT
5. Arquitectura Backend y Frontend
6. Base de datos y jerarquía POO
7. Casos de uso y estados
8. Estructura del proyecto y métricas
9. Errores, stack y deuda técnica
10. Roadmap

---
layout: default
---

# Concepto del Sistema

Plataforma de gestión académica que centraliza:

- **Carreras** — CRUD + validación de integridad
- **Alumnos** — Inscripción pública + listado admin
- **Administradores** — CRUD autenticado con JWT
- **Profesores** — CRUD autenticado
- **Formularios** — CRUD con estados (Borrador/Abierto/Cerrado)
- **Listados** — Vista consolidada alumnos + carrera
- **Autenticación** — Login JWT (8h) + BCrypt workFactor 12

<!--
Dos audiencias: panel interno (JWT) + formulario público de inscripción.
-->

---
layout: default
class: text-center
---

# Arquitectura N-Tier

<img src="/arquitectura-horizontal.png" class="mx-auto mt-8" style="max-height: 280px;" />

<!--
Vue 3 + TS + Vite → REST + JWT → ASP.NET Core 9 API → BR Layer → AD Layer → SQL Server
-->

---
layout: default
class: text-center
---

# Arquitectura Simplificada

<img src="/simple-3-bloques.png" class="mx-auto mt-8" style="max-height: 260px;" />

<div class="text-sm mt-6 opacity-70">
  Vista de alto nivel: 3 grandes bloques + flechas de ida y vuelta
</div>

<!--
Versión simplificada para audiencia no técnica.
-->

---
layout: default
class: text-center
---

# El Viaje de un Request

**`GET /api/carreras`** desde `CarreraView.vue`

<img src="/sequence-request.png" class="mx-auto mt-6" style="max-height: 420px;" />

<!--
Sequence diagram con 7 actores.
Camino de ida (banda superior) y camino de vuelta (banda inferior).
-->

---
layout: default
class: text-center
---

# Flujo Detallado del Request

<img src="/flujo-vertical.png" class="mx-auto mt-4" style="max-height: 480px;" />

<!--
Diagrama vertical con las 8 capas apiladas.
Muestra transformaciones: SQL → Entidad → DTO → JSON.
-->

---
layout: default
class: text-center
---

# Flujo Horizontal — Ida y Vuelta

<img src="/flujo-horizontal.png" class="mx-auto mt-8" style="max-height: 220px;" />

<div class="text-sm mt-6 opacity-70">
  <b>━━</b> Camino de ida (request) &nbsp;&nbsp;•&nbsp;&nbsp; <b>⇢⇢</b> Camino de vuelta (response)
</div>

<!--
Vista compacta con las 8 capas en línea.
-->

---
layout: default
class: text-center
---

# Arquitectura por Capas

<img src="/subgraphs-capas.png" class="mx-auto mt-6" style="max-height: 400px;" />

<!--
4 subgraphs: HTTP / BACKEND .NET / FRONTEND / DATABASE
-->

---
layout: default
class: text-center
---

# Flujo de Autenticación JWT

<img src="/auth-jwt.png" class="mx-auto mt-6" style="max-height: 430px;" />

<!--
Login completo con BCrypt.Verify + GenerarToken + sessionStorage.
-->

---
layout: default
class: text-center
---

# Arquitectura del Backend

<img src="/arquitectura-backend.png" class="mx-auto mt-6" style="max-height: 430px;" />

<!--
3 capas: Instituto.AD, Instituto.BR, Instituto.API.
-->

---
layout: default
class: text-center
---

# Arquitectura del Frontend

<img src="/arquitectura-frontend.png" class="mx-auto mt-6" style="max-height: 430px;" />

<!--
6 áreas: Vistas, Router, Auth, UI, Integración API, Pinia.
-->

---
layout: default
class: text-center
---

# Base de Datos — Modelo ER

<img src="/db-er.png" class="mx-auto mt-6" style="max-height: 430px;" />

<!--
5 tablas: CARRERAS, ALUMNOS, ADMINISTRADORES, PROFESORES, FORMULARIOS.
Relación 1:N entre CARRERAS y ALUMNOS.
-->

---
layout: default
class: text-center
---

# Jerarquía de Clases (POO)

<img src="/jerarquia-clases.png" class="mx-auto mt-6" style="max-height: 430px;" />

<!--
Persona abstracta → Alumno, Administrador, Profesor.
-->

---
layout: default
class: text-center
---

# Casos de Uso por Rol

<img src="/casos-uso.png" class="mx-auto mt-6" style="max-height: 430px;" />

<!--
3 roles: SuperAdmin, Admin, Público.
-->

---
layout: default
class: text-center
---

# Estados de Formularios

<img src="/formulario-estados.png" class="mx-auto mt-6" style="max-height: 430px;" />

<!--
State diagram: Borrador → Abierto → Cerrado → Archivado.
-->

---
layout: default
class: text-center
---

# Estructura del Proyecto

<img src="/estructura-solution.png" class="mx-auto mt-6" style="max-height: 300px;" />

<!--
Instituto.sln → 5 proyectos.
-->

---
layout: default
class: text-center
---

# Métricas del Proyecto

<img src="/metricas.png" class="mx-auto mt-6" style="max-height: 450px;" />

<!--
8 Controllers, 6 Servicios, 6 Repositorios, 44 Tests, 19 Vistas, 5 Tablas.
-->

---
layout: default
---

# Manejo de Errores

| Caso | Origen | Captura | HTTP |
|------|--------|---------|------|
| Validación negocio | Service | Controller | **400/404** |
| Excepción no controlada | Cualquiera | `UseExceptionHandler` | **500** |
| JWT inválido/expirado | Middleware | `[Authorize]` | **401** |
| Error de BD | AD | Controller | **500** |

<div class="mt-8 text-center text-xl">
  ✅ Ningún stack trace llega al cliente
</div>

<!--
Respuestas siempre JSON: { "error": "..." }
Seguridad por diseño.
-->

---
layout: default
---

# Stack Tecnológico

<div class="grid grid-cols-2 gap-8 mt-8">

<div>

### Backend
- ASP.NET Core 9
- ADO.NET (`Microsoft.Data.SqlClient`)
- JWT Bearer + BCrypt 12
- MSTest + Moq (44 tests)

</div>

<div>

### Frontend
- Vue 3.5 + TypeScript 5.8
- Vite 7 + Vue Router 4
- Pinia + Element Plus
- CSS modular

</div>

</div>

<!--
Sin ORM. ADO.NET puro con SQL parametrizado.
-->

---
layout: default
---

# Deuda Técnica Conocida

### 🔴 Crítica
- Bug `CarreraId: string` vs `number` en `InscripciónView.vue`
- Frontend de **Profesores** sin vistas (backend listo)

### 🟠 Alta
- Capa `src/services/` no centralizada
- GET admins expone `PasswordHash`/`Role`
- Sin rate limiting

### 🟡 Media
- Sin paginación en listados
- Sin Swagger/OpenAPI

<!--
No escondemos la deuda.
Sabemos qué falta y por qué.
-->

---
layout: default
class: text-center
---

# Roadmap del Proyecto

<img src="/roadmap-timeline.png" class="mx-auto mt-6" style="max-height: 300px;" />

<!--
Timeline v1.0 → v1.1 → v1.2 → v2.0.
-->

---
layout: center
class: text-center
---

# ¿Preguntas?

**Gracias por su atención**

Instituto Superior Docente Túpac Amaru — 2026

<!--
Tener a mano:
- Código de CarreraController
- Diagrama de BD
- Documentación en Docs/
-->

---
layout: center
class: text-center
---

<div class="text-6xl font-bold mb-6">🏛️</div>

# Sistema de Gestión Institucional

**Instituto Superior Docente Túpac Amaru**

<div class="mt-8 text-sm opacity-60">
  Vue 3 + TypeScript + Vite · ASP.NET Core 9 · SQL Server
</div>

<!--
Slide final para cerrar la presentación.
-->