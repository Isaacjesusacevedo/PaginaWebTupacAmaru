<template>
  <main class="section">
    <div class="card card-center card-lg">

      <img
        class="card-media"
        src="/src/components/Banner/bannerEstudiante.jpg"
        alt="Banner inscripción"
      />

      <section class="text-center">
        <h1>
          Formulario de Inscripción Alumnos Tupac Amaru <br />
          2026
        </h1>
        <p class="subtitle">
          Completá todos los campos solicitados
        </p>
      </section>

      <form class="form" @submit.prevent="guardarAlumno">

        <section>
          <h2>Datos personales</h2>
          <hr />

          <div class="form-row">
            <div class="field">
              <label>Nombre</label>
              <input type="text" v-model="alumno.Nombre" />
            </div>

            <div class="field">
              <label>Apellido</label>
              <input type="text" v-model="alumno.Apellido" />
            </div>
          </div>

          <div class="form-row">
            <div class="field">
              <label>DNI</label>
              <input type="text" v-model="alumno.DNI" />
            </div>

            <div class="field">
              <label>Nacionalidad</label>
              <input type="text" v-model="alumno.Nacionalidad" />
            </div>
          </div>

          <div class="form-row">
            <div class="field">
              <label>Fecha de nacimiento</label>
              <input type="date" v-model="alumno.FechaNacimiento" />
            </div>

            <div class="field">
              <label>Edad al 30/06/2026</label>
              <input type="text" :value="edad ?? ''" disabled />
            </div>
          </div>
        </section>

        <section>
          <h2>Contacto</h2>
          <hr />

          <div class="field">
            <label>Dirección</label>
            <input type="text" v-model="alumno.Direccion" />
          </div>

          <div class="form-row">
            <div class="field">
              <label>Teléfono</label>
              <input type="tel" v-model="alumno.Telefono" />
            </div>

            <div class="field">
              <label>Correo electrónico</label>
              <input type="email" v-model="alumno.Email" />
            </div>
          </div>
        </section>

        <section>
          <h2>Información académica</h2>
          <hr />

          <div class="form-checklist mt-2">
            <p class="checklist-title">
              <strong>Indicar si posee:</strong>
            </p>

            <label class="check-item">
              <input type="checkbox" v-model="documentacion.titulo" />
              <span class="check-custom"></span>
              <span class="check-text">Título</span>
            </label>

            <label class="check-item">
              <input type="checkbox" v-model="documentacion.tituloEnTramite" />
              <span class="check-custom"></span>
              <span class="check-text">Título en trámite</span>
            </label>

            <label class="check-item">
              <input type="checkbox" v-model="documentacion.materiasAdeudadas" />
              <span class="check-custom"></span>
              <span class="check-text">Constancia de materias adeudadas</span>
            </label>

            <label class="check-item">
              <input type="checkbox" v-model="documentacion.alumnoRegular" />
              <span class="check-custom"></span>
              <span class="check-text">Constancia de alumno regular</span>
            </label>
          </div>

          <!-- Título: pide fecha de egreso + título secundario -->
          <div v-if="documentacion.titulo" class="form-row mt-3">
            <div class="field">
              <label>Fecha de egreso del título</label>
              <input type="date" v-model="academico.tituloFechaEmision" required />
            </div>

            <div class="field">
              <label>Título secundario</label>
              <input type="text" v-model="academico.tituloSecundario" required />
            </div>
          </div>

          <!-- Título en trámite: pide solo el título secundario -->
          <div v-if="documentacion.tituloEnTramite" class="form-row mt-3">
            <div class="field">
              <label>Título secundario</label>
              <input type="text" v-model="academico.tituloSecundario" required />
            </div>
          </div>

        </section>

        <section>
          <h2>Inscripción</h2>
          <hr />

          <div class="form-row">
            <div class="field">
              <label>Turno</label>
              <select v-model="alumno.Turno">
                <option disabled value="">Seleccionar turno</option>
                <option>Mañana</option>
                <option>Tarde</option>
                <option>Noche</option>
              </select>
            </div>

            <div class="field">
              <label>Carrera</label>
              <select v-model="alumno.CarreraId">
                <option disabled value="">Seleccionar carrera</option>
                <option
                  v-for="carrera in carreras"
                  :key="carrera.id"
                  :value="carrera.id"
                >
                  {{ carrera.nombre }}
                </option>
              </select>
            </div>
          </div>
        </section>

        <p v-if="error" class="text-center mt-2" style="color: var(--color-danger)">
          {{ error }}
        </p>

        <button
          type="submit"
          class="btn btn-success btn-block"
          :disabled="loading"
        >
          {{ loading ? 'Guardando...' : 'Enviar inscripción' }}
        </button>

        <footer class="form-footer">
          <p>
            Revisá que los datos ingresados sean correctos antes de enviar el formulario.
          </p>
          <p class="footer-copy">
            Formulario desarrollado por estudiantes de 3.º año de la TSAS — 2026
          </p>
        </footer>

      </form>
    </div>
  </main>
</template>

<script setup>
import { reactive, ref, watch, onMounted, computed } from 'vue'
import { alumnoApi } from '@/services/api'

// Años cumplidos a una fecha fija del ciclo de inscripción, no a la fecha de hoy.
const FECHA_REFERENCIA_EDAD = new Date(2026, 5, 30)

const edad = computed(() => {
  if (!alumno.FechaNacimiento) return null
  const nacimiento = new Date(alumno.FechaNacimiento)
  if (Number.isNaN(nacimiento.getTime())) return null

  let anios = FECHA_REFERENCIA_EDAD.getFullYear() - nacimiento.getFullYear()
  const aunNoCumplio =
    FECHA_REFERENCIA_EDAD.getMonth() < nacimiento.getMonth() ||
    (FECHA_REFERENCIA_EDAD.getMonth() === nacimiento.getMonth() &&
      FECHA_REFERENCIA_EDAD.getDate() < nacimiento.getDate())

  if (aunNoCumplio) anios--
  return anios
})

const alumno = reactive({
  Nombre: '',
  Apellido: '',
  DNI: '',
  Email: '',
  FechaNacimiento: '',
  Direccion: '',
  Nacionalidad: '',
  Telefono: '',
  Turno: '',
  CarreraId: '',
})

const documentacion = reactive({
  titulo: false,
  tituloEnTramite: false,
  materiasAdeudadas: false,
  alumnoRegular: false,
})

// Campos de info académica que se envían al backend (modelo simplificado)
const academico = reactive({
  tituloFechaEmision: '',
  tituloSecundario: '',
})

// Título y Título en trámite son excluyentes.
watch(() => documentacion.titulo, (v) => {
  if (v) {
    documentacion.tituloEnTramite = false
  } else {
    academico.tituloFechaEmision = ''
  }
})
watch(() => documentacion.tituloEnTramite, (v) => {
  if (v) {
    documentacion.titulo = false
    academico.tituloFechaEmision = ''
  }
})

const loading = ref(false)
const error = ref(null)

const carreras = ref([])

const cargarCarreras = async () => {
  try {
    carreras.value = await alumnoApi.getCarreras()
  } catch (err) {
    console.error(err)
    error.value = "No se pudieron cargar las carreras"
  }
}

onMounted(cargarCarreras)

const resetFormulario = () => {
  alumno.Nombre = ''
  alumno.Apellido = ''
  alumno.DNI = ''
  alumno.Email = ''
  alumno.FechaNacimiento = ''
  alumno.Direccion = ''
  alumno.Nacionalidad = ''
  alumno.Telefono = ''
  alumno.Turno = ''
  alumno.CarreraId = ''

  documentacion.titulo = false
  documentacion.tituloEnTramite = false
  documentacion.materiasAdeudadas = false
  documentacion.alumnoRegular = false

  academico.tituloFechaEmision = ''
  academico.tituloSecundario = ''
}

const validar = () => {
  if (!alumno.Nombre || !alumno.Apellido || !alumno.Email || !alumno.DNI || !alumno.CarreraId) {
    return "Nombre, Apellido, Email, DNI y Carrera son obligatorios"
  }

  if (documentacion.titulo && (!academico.tituloFechaEmision || !academico.tituloSecundario)) {
    return "Si posee título, debe completar la fecha de egreso y el título secundario"
  }

  if (documentacion.tituloEnTramite && !academico.tituloSecundario) {
    return "Si tiene título en trámite, debe completar el título secundario"
  }

  if (!documentacion.titulo && !documentacion.tituloEnTramite && !documentacion.materiasAdeudadas && !documentacion.alumnoRegular) {
    return "Debe informar al menos un dato académico"
  }

  return null
}

const guardarAlumno = async () => {
  error.value = null

  const mensajeValidacion = validar()
  if (mensajeValidacion) {
    error.value = mensajeValidacion
    return
  }

  loading.value = true

  try {
    await alumnoApi.inscribir({
      nombre: alumno.Nombre,
      apellido: alumno.Apellido,
      email: alumno.Email,
      dni: Number(alumno.DNI),
      fechaNacimiento: alumno.FechaNacimiento,
      direccion: alumno.Direccion || null,
      nacionalidad: alumno.Nacionalidad || null,
      telefono: alumno.Telefono || null,
      turno: alumno.Turno || null,
      carreraId: Number(alumno.CarreraId),
      // Info académica (modelo simplificado):
      fechaEgreso: academico.tituloFechaEmision || null,
      tituloSecundario: academico.tituloSecundario || null,
      poseeTitulo: documentacion.titulo,
      tituloEnTramite: documentacion.tituloEnTramite,
      consMaterias: documentacion.materiasAdeudadas,
      consAlumnoRegular: documentacion.alumnoRegular,
    })

    alert("Inscripción enviada correctamente")
    resetFormulario()
  } catch (err) {
    console.error(err)
    error.value = err.message ?? "Hubo un error al enviar la inscripción"
  } finally {
    loading.value = false
  }
}
</script>
