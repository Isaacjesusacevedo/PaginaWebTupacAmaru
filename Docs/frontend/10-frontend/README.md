# Documentación del Frontend - Instituto Tupac Amaru

## Índice de Documentación

### [01 - Visión General](./01-vision-general.md)
- Stack tecnológico
- Arquitectura general
- Principios de diseño

### [02 - Estructura del Proyecto](./02-estructura-proyecto.md)
- Directorios y archivos principales
- Convenciones de nomenclatura
- Organización por módulos

### [03 - Enrutamiento](./03-enrutamiento.md)
- Configuración de Vue Router
- Tabla completa de rutas
- Navigation guards y metadatos
- Props en rutas dinámicas

### [04 - Autenticación y Autorización](./04-autenticacion.md)
- Composable `useAuth`
- Flujo de login/logout
- Headers de autenticación
- Integración con guards

### [05 - Componentes y Vistas](./05-componentes-vistas.md)
- Componentes reutilizables
- Vistas por módulo (Admin, Carreras, Listados, Auth, Público)
- Patrones comunes de código
- Estilos globales

### [06 - Integración con API](./06-integracion-api.md)
- Cliente HTTP (fetch nativo)
- Endpoints consumidos por módulo
- Patrones de request/response
- Manejo de errores
- Tipado de respuestas

### [07 - Configuración y Build](./07-configuracion.md)
- Archivos de configuración (Vite, TS, ESLint, Prettier)
- Variables de entorno
- Scripts disponibles
- Build producción y Docker

---

## Acceso Rápido

| Tarea | Documento |
|-------|-----------|
| Entender la arquitectura | [01 - Visión General](./01-vision-general.md) |
| Encontrar un archivo | [02 - Estructura del Proyecto](./02-estructura-proyecto.md) |
| Agregar/modificar ruta | [03 - Enrutamiento](./03-enrutamiento.md) |
| Cambiar lógica de auth | [04 - Autenticación](./04-autenticacion.md) |
| Crear nueva vista/CRUD | [05 - Componentes y Vistas](./05-componentes-vistas.md) |
| Conectar nuevo endpoint | [06 - Integración API](./06-integracion-api.md) |
| Configurar build/CI | [07 - Configuración](./07-configuracion.md) |

---

## Estado del Proyecto

- **Framework:** Vue 3 + TypeScript + Vite
- **UI:** Element Plus + estilos propios
- **Estado:** Pinia (configurado, uso mínimo actual)
- **Routing:** Vue Router 4 con lazy loading + guards
- **Auth:** JWT en sessionStorage + composable `useAuth`
- **API:** Fetch nativo + `VITE_API_URL`

## Próximas Mejoras Sugeridas

1. **Centralizar tipos TypeScript** en `src/types/` para sincronía con backend
2. **Corregir URLs hardcodeadas** → usar `import.meta.env.VITE_API_URL` en todas las vistas
3. **Implementar Pinia store** para estado global (usuario, notificaciones, cache)
4. **Agregar interceptor/timeout** para fetch (wrapper con AbortController)
5. **Tests unitarios** (Vitest) y E2E (Cypress/Playwright)
6. **Storybook** para documentación de componentes
7. **PWA** (service worker, manifest) para instalación
8. **Internacionalización (i18n)** si se requiere multiidioma