import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/public/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // 🏠 Públicas
    { path: '/', name: 'home', component: HomeView },
    { path: '/contacto', name: 'contacto', component: () => import('../views/public/ContactoView.vue') },

    // 🔐 Auth
    { path: '/login', name: 'login', component: () => import('../views/auth/LoginView.vue') },

    // 🛠 Administración
    { path: '/administracion', name: 'administracion', component: () => import('../views/administradores/AdministradorView.vue') },
    { path: '/agregaradministracion', name: 'agregaradministracion', component: () => import('../views/administradores/AgregarAdministradorView.vue') },
    // ✏️ Editar administrador (CON ID)
    { path: '/editaradministrador/:id', name: 'editaradministrador', component: () => import('../views/administradores/EditarAdministradorView.vue'), props: true },
    {
      path: '/eliminaradministrador/:id',
      name: 'eliminaradministrador',
      component: () => import('../views/administradores/EliminarAdministradorView.vue'),
      props: true
    },
    // 📚 Carreras
    { path: '/carreras', name: 'carreras', component: () => import('../views/carrera/CarreraView.vue') },
    { path: '/agregarcarreras', name: 'agregarcarreras', component: () => import('../views/carrera/AgregarCarreraView.vue') },

    // ✏️ Editar carrera (CON ID)
    {
      path: '/editarcarrera/:id',
      name: 'editarcarrera',
      component: () => import('../views/carrera/EditarCarreraView.vue'),
      props: true
    },

    // 🗑 Eliminar carrera (CON ID)
    {
      path: '/eliminarcarreras/:id',
      name: 'eliminarcarreras',
      component: () => import('../views/carrera/EliminarCarreraView.vue'),
      props: true
    },

    // 📄 Listados
    { path: '/listados', name: 'listados', component: () => import('../views/Listados/ListadoView.vue') },
    { path: '/inscripcion', name: 'inscripcion', component: () => import('../views/Listados/InscripciónView.vue') },

    // ❌ 404
    { path: '/:pathMatch(.*)*', name: 'not-found', component: () => import('../views/NotFound.vue') }
  ]
})

export default router
