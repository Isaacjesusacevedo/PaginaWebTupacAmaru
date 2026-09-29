---
theme: default
title: Sistema de Gestión Institucional — Instituto Superior Docente Túpac Amaru
info: |
  Defensa de Proyecto Final — 2026
  Alumno: [Tu Nombre] — Curso: 3.º Año TSAS
  Tutor: [Nombre Tutor]
class: text-center
highlighter: shiki
lineNumbers: true
transition: slide-left
mdc: true
colorSchema: dark
slideNumber: true
drawings:
  persist: false
---

# Sistema de Gestión Institucional

**Instituto Superior Docente Túpac Amaru**

Plataforma de gestión académica y administrativa

<div class="pt-12">
  <span class="px-3 py-1 rounded bg-white/10 border border-white/20">
    Presioná <kbd>Espacio</kbd> o <kbd>→</kbd> para avanzar
  </span>
</div>

<!--
PORTADA — 30 segundos
• Saludar al jurado, presentarse (nombre, curso, tutor)
• "Hoy voy a presentar mi proyecto final: un sistema de gestión institucional
  desarrollado para el Instituto Superior Docente Túpac Amaru."
• Stack: Vue 3 + JavaScript + Vite (frontend) + ASP.NET Core 9 + SQL Server (backend)
-->

---
layout: default
transition: fade
---

# Contexto & Problema

<div class="grid grid-cols-2 gap-8 mt-8">

<div>

### ❌ Situación Anterior — Google Forms

- Inscripciones vía **Google Forms** → proceso **manual y no eficiente**
- Datos en **planillas sueltas**, sin integridad referencial
- **Sin panel centralizado** para gestionar carreras, alumnos o admins
- **Imposible generar listados** consolidados (alumno + carrera)
- Sin **autenticación ni roles** (cualquiera con el link podía inscribirse)

</div>

<div>

### ✅ Objetivo del Proyecto

- **Centralizar** toda la gestión académica en un solo sistema
- **Formulario público** integrado + **panel admin** con JWT
- **Garantizar integridad** de datos (FK, validaciones, soft delete)
- **Gestionar roles** (Admin / SuperAdmin) con permisos diferenciados
- **Generar listados** consolidados en tiempo real

</div>

</div>

<div class="mt-12 p-4 bg-green-500/10 border border-green-500/30 rounded-lg">

**Resultado:** Sistema web full-stack N-Tier — Vue 3 + JavaScript + ASP.NET Core 9 + SQL Server

</div>

<!--
CONTEXTO & PROBLEMA — 60 segundos
• "Antes usaban Google Forms para las inscripciones."
• "Funcionaba, pero NO era eficiente: datos en planillas sueltas, sin integridad
  referencial, imposible cruzar alumno + carrera + turno."
• "Sin panel admin: cada consulta era abrir la planilla y buscar a mano."
• "Sin auth ni roles: cualquiera con el link podía inscribirse, y editar era
  manual, sin auditoría."
• "Nuestro objetivo: CENTRALIZAR, AUTOMATIZAR, GARANTIZAR integridad y roles."
• Transición: "Veamos la solución en números..."
-->

---
layout: center
transition: fade
---

# La Solución en Números

<div class="grid grid-cols-3 gap-4 mt-8 text-left">

<div class="p-4 bg-blue-500/10 rounded-lg border border-blue-500/30">
  <div class="text-4xl font-bold text-blue-400">8</div>
  <div class="text-sm opacity-80">Controllers REST</div>
</div>

<div class="p-4 bg-orange-500/10 rounded-lg border border-orange-500/30">
  <div class="text-4xl font-bold text-orange-400">6</div>
  <div class="text-sm opacity-80">Servicios de Negocio</div>
</div>

<div class="p-4 bg-cyan-500/10 rounded-lg border border-cyan-500/30">
  <div class="text-4xl font-bold text-cyan-400">6</div>
  <div class="text-sm opacity-80">Repositorios ADO.NET</div>
</div>

<div class="p-4 bg-green-500/10 rounded-lg border border-green-500/30">
  <div class="text-4xl font-bold text-green-400">44</div>
  <div class="text-sm opacity-80">Tests Pasando</div>
</div>

<div class="p-4 bg-purple-500/10 rounded-lg border border-purple-500/30">
  <div class="text-4xl font-bold text-purple-400">19</div>
  <div class="text-sm opacity-80">Archivos .vue</div>
</div>

<div class="p-4 bg-red-500/10 rounded-lg border border-red-500/30">
  <div class="text-4xl font-bold text-red-400">5</div>
  <div class="text-sm opacity-80">Tablas BD + 1 FK</div>
</div>

</div>

<!--
SOLUCIÓN EN NÚMEROS — 45 segundos
• "Esta es la solución en una slide: números concretos, no promesas."
• "8 controllers REST, 6 servicios con lógica de negocio, 6 repositorios ADO.NET puro."
• "44 tests automatizados (20 AD + 17 BR + 7 API integración)."
• "19 archivos .vue = 18 vistas + App.vue raíz cubriendo todos los módulos funcionales."
• "5 tablas normalizadas con 1 FK + soft delete en Admins."
-->

---
layout: image
image: /arquitectura-horizontal.png
backgroundSize: contain
transition: fade
---

# Arquitectura N-Tier

<!--
ARQUITECTURA N-TIER — 75 segundos
• "Arquitectura N-Tier clásica con separación estricta en 3 capas backend."
• "Vue 3 + JavaScript en el navegador llama a REST API con JWT."
• "API: Controllers + JWT + CORS + Global Exception Handler."
• "BR: Business Rules. Lógica de negocio, validaciones, Result Pattern."
• "AD: Data Access. Repositorios tipados + AccesoDB (wrapper ADO.NET)."
• "SQL Server: 5 tablas, SQL parametrizado, sin ORM."
-->

---
layout: image
image: /simple-3-bloques.png
backgroundSize: contain
transition: fade
---

# Vista Simplificada: 3 Bloques

<!--
VISTA SIMPLIFICADA — 45 segundos
• "Para quienes no son técnicos: 3 cajas grandes."
• "FRONTEND (Vue 3 + JS): 18 vistas + App.vue, guards, composables."
• "BACKEND (ASP.NET Core 9): 8 controllers, 6 servicios, 6 repositorios."
• "BASE DE DATOS (SQL Server): 5 tablas, 1 FK, soft delete."
• "Ida: Fetch+JWT → Controller → Service → Repository → SQL."
• "Vuelta: SQL → Entidad → DTO → ApiResponse → JSON → Vue."
-->

---
layout: two-cols
transition: slide-left
---

# El Viaje de un Request Real

`GET /api/carreras` desde `CarreraView.vue`

::left::

## Ida (Request)

<v-clicks>

1. **Vista** → `onMounted` → `apiGet('/api/carreras')`
2. **Front API** → `useApiFetch.js` agrega `Bearer <JWT>`
3. **HTTP** → `GET /api/carreras` en JSON camelCase
4. **Controller** → `CarreraController.GetAll()` `[AllowAnonymous]`
5. **Service** → `CarreraService.GetAll()` valida reglas
6. **Repository** → `CarreraRepository.GetAll()` mapea reader
7. **AccesoDB** → `db.GetData()` SQL parametrizado
8. **SQL Server** → `SELECT * FROM Carreras`

</v-clicks>

::right::

## Vuelta (Response)

<v-clicks>

1. **SQL Server** → `SqlDataReader` crudo
2. **Repository** → `MapReaderToCarrera()` → `List<Carrera>`
3. **Service** → `ServiceResult` con la lista
4. **Controller** → `ApiResponse.Success` → JSON camelCase
5. **Front API** → `res.json()` → desempaqueta
6. **Vista** → `carreras.value = data` → reactive UI

</v-clicks>

<!--
FLUJO DE REQUEST — 120 segundos (slide clave)
• "Esta es LA slide técnica más importante. Un request real paso a paso."
• USAR <v-clicks> para revelar los pasos progresivamente.
• "Ida: 8 pasos desde Vista hasta SQL. Vuelta: 6 pasos desde SQL hasta UI."
• Enfatizar: "Cada capa tiene UNA responsabilidad."
• "AccesoDB encapsula ADO.NET: conexión, comando, reader, parámetros."
• "ServiceResult<T> = Result Pattern. ApiResponse<T> = wrapper JSON."
-->

---
layout: image
image: /sequence-request.png
backgroundSize: contain
transition: fade
---

# Sequence Diagram — Ida y Vuelta

<!--
SEQUENCE DIAGRAM — 60 segundos
• "7 actores en columnas: Vista, Front API, Controller, Service,
  Repository, AccesoDB, SQL Server."
• "12 mensajes numerados, banda amarilla separa ida de vuelta."
• "Fíjense en las flechas punteadas: es la vuelta del response."
-->

---
layout: image
image: /flujo-vertical.png
backgroundSize: contain
transition: fade
---

# Transformaciones de Datos

<!--
FLUJO VERTICAL — 60 segundos
• "Transformaciones de datos en cada capa."
• "SQL nativo → Entidad (Carrera) → ServiceResult → ApiResponse → JSON → Vue."
• "Cada transformación tiene un propósito: tipado fuerte, manejo de errores,
  serialización consistente, reactividad."
• "No hay 'magic mapping' — todo explícito, trazable, testeable."
-->

---
layout: image
image: /arquitectura-backend.png
backgroundSize: contain
transition: fade
---

# Arquitectura Backend — 3 Capas + SQL

<!--
ARQUITECTURA BACKEND — 75 segundos
• "Estructura real del solution: 3 proyectos .NET + SQL Server."
• "AD (azul): 6 repositorios + AccesoDB (wrapper ADO.NET)."
• "BR (naranja): 6 servicios con lógica, validaciones, Result Pattern."
• "API (violeta): 8 controllers + JWT + CORS + Global Exception Handler."
• "Dependencias unidireccionales: API → BR → AD → SQL."
-->

---
layout: image
image: /arquitectura-frontend.png
backgroundSize: contain
transition: fade
---

# Arquitectura Frontend — 6 Áreas

<!--
ARQUITECTURA FRONTEND — 75 segundos
• "Frontend modular: 19 archivos .vue (18 vistas + App.vue raíz) organizados por dominio funcional."
• "Router + Guards: requiereAuth protege panel, soloInvitado login."
• "useAuth composable (JS): token, admin, headers, logout, re-auth."
• "UI + Estilos: Element Plus + CSS modular (sin <style> en .vue)."
• "Integración API: fetch nativo + VITE_API_URL, sin proxy Vite."
• "Pinia solo para auth store; resto estado local reactivo."
-->

---
layout: image
image: /css-arquitectura.png
backgroundSize: contain
transition: fade
---

# Organización del CSS

<!--
CSS — 60 segundos
• "CSS modular sin `<style>` en los .vue. Todo centralizado en /assets/css/."
• "10 archivos: 2 base + 7 componentes + 1 layout."
• "main.css hace el @import chain — single entry point."
• "global.css: variables CSS (colores, radios, sombras) — single source of truth."
• "Cada archivo tiene UNA responsabilidad: botones, cards, forms, tablas..."
• "Las vistas solo usan clases globales: .card, .form, .btn, .table."
• "Ventaja: si cambio un color en global.css, se actualiza todo el sitio."
-->

---
layout: image
image: /vistas-css-mapping.png
backgroundSize: contain
transition: fade
---

# Vistas y Estilos

<!--
VISTAS ↔ CSS — 60 segundos
• "Mapeo de qué clases usa cada vista."
• "Vistas de admin (CRUD): usan .table + .form + .btn (listado + form + acciones)."
• "Vista de login: usa .card + .form + .password-field (formulario centrado)."
• "Inscripción pública: .form + inputs + btn (formulario extenso sin navbar)."
• "Dashboard: .card + .admin-menu (cards de menú con botones)."
• "Todas las vistas comparten .card y .btn como base visual."
-->

---
layout: two-cols
transition: slide-left
---

# Autenticación JWT — Login + Re-auth

::left::

## Login Flow

<v-clicks>

1. **Usuario** → `/login` → Email + Password
2. **POST** `/api/auth/login`
3. **Service** → `Login()` → `GetByEmail()` → `WHERE Activo = 1`
4. **BCrypt** → `Verify(password, hash)` con workFactor 12
5. **OK** → `AdminResult` → **JWT 8h**
6. **Claims** → `sub`, `email`, `name`, `role`, `jti`
7. **Frontend** → `sessionStorage` → redirect a `/`

</v-clicks>

::right::

## Re-autenticación (Editar Admin)

<v-clicks>

1. **Usuario** → `/editaradministrador/:id`
2. **Modal** → exige password actual
3. **POST** `/api/auth/verify-password` + JWT
4. **Service** → verifica hash con `BCrypt.Verify`
5. **OK** → guarda `passwordVerificada` en memoria (ref)
6. **Carga datos** → `GET /api/administradores/:id`
7. **Edita** → `PUT` datos + password opcional
8. **onUnmounted** → limpia password en memoria

</v-clicks>

<!--
AUTH JWT — 90 segundos
• "Autenticación completa: login + re-auth para operaciones sensibles."
• IZQUIERDA: "Login síncrono. BCrypt workFactor 12 = ~250ms."
• DERECHA: "Al editar admin, exige password actual ANTES de cargar datos."
• "Password en memoria (ref), se limpia en onUnmounted."
• "Soft delete en admins: Activo=0, índice UNIQUE parcial."
-->

---
layout: default
transition: fade
---

# Código Real: `CarreraController.cs`

```csharp
[ApiController]
[Authorize]
[Route("api/carreras")]
public class CarreraController : ControllerBase
{
    private readonly ICarreraService _service;

    public CarreraController(ICarreraService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetAll()
        => Ok(ApiResponse<List<Carrera>>.Success(_service.GetAll()));

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public IActionResult GetById(int id)
    {
        var carrera = _service.GetById(id);
        return carrera is null
            ? NotFound(ApiResponse<Carrera>.Error($"Carrera {id} no encontrada."))
            : Ok(ApiResponse<Carrera>.Success(carrera));
    }

    [HttpPost]
    public IActionResult Create([FromBody] Carrera carrera)
    {
        var result = _service.Create(carrera);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id },
                ApiResponse<Carrera>.Success(result.Data))
            : BadRequest(ApiResponse<Carrera>.Error(result.Message!));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var result = _service.Delete(id);
        return result.Success
            ? NoContent()
            : NotFound(ApiResponse<string>.Error(result.Message!));
    }
}