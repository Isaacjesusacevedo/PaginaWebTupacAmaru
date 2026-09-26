# Frontend - Visión General

## Descripción General

El frontend del Instituto Tupac Amaru es una **Single Page Application (SPA)** construida con **Vue 3** (Composition API + TypeScript) que sirve como panel de administración para la gestión de administradores, carreras, alumnos y listados de inscripción.

## Stack Tecnológico

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **Vue** | 3.5.x | Framework reactivo (Composition API) |
| **TypeScript** | 5.8.x | Tipado estático |
| **Vite** | 7.x | Bundler y dev server |
| **Vue Router** | 4.5.x | Enrutamiento SPA |
| **Pinia** | 3.x | Gestión de estado global |
| **Element Plus** | 2.11.x | Biblioteca de componentes UI |
| **ESLint + Prettier** | - | Linting y formateo |

## Arquitectura General

```
Frontend/
├── src/
│   ├── components/       # Componentes reutilizables
│   ├── composables/      # Lógica reactiva reutilizable (hooks)
│   ├── router/           # Configuración de rutas y guards
│   ├── views/            # Páginas/vistas por funcionalidad
│   │   ├── administradores/
│   │   ├── carrera/
│   │   ├── auth/
│   │   ├── public/
│   │   └── Listados/
│   ├── plugins/          # Plugins de Vue (Pinia, etc.)
│   ├── assets/           # Estilos, imágenes, fuentes
│   ├── App.vue           # Componente raíz
│   └── main.ts           # Punto de entrada
├── index.html
├── package.json
├── tsconfig.json
└── vite.config.ts
```

## Principios de Diseño

1. **Composition API** + `<script setup>` en todos los componentes
2. **TypeScript estricto** con interfaces para tipado de datos
3. **Lazy loading** de rutas para optimizar bundle inicial
4. **Guards de navegación** para autenticación/autorización
5. **Separación de responsabilidades**: vistas (UI) ↔ composables (lógica) ↔ API (fetch directo)
6. **Componentes UI consistentes** usando Element Plus + estilos propios