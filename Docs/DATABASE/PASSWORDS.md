# Contraseñas de Prueba (Desarrollo)

| Email | Contraseña | Rol | Activo |
|-------|------------|-----|--------|
| `admin@tupac.edu.ar` | `Tupac123` | SuperAdmin | Sí |
| `isa@test.com` | (definida al crear) | Admin | Sí |
| `admin@instituto.edu.ar` | (histórica) | Admin | No (soft delete) |

> **Antes de producción:** eliminar todos los usuarios de prueba y crear nuevos con contraseñas seguras.

---

## Crear el Primer Admin (Solo Dev, Una Sola Vez)

```bash
curl -X POST http://localhost:5127/api/setup/admin \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Super","apellido":"Admin","email":"admin@tupac.edu.ar","password":"Tupac123","role":"SuperAdmin"}'
```