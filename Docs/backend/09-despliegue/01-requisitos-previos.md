# Requisitos Previos

## Software Requerido

| Componente | Versión Mínima | Notas |
|------------|----------------|-------|
| **.NET SDK** | 9.0 | `dotnet --version` |
| **SQL Server** | 2019+ (LocalDB/Express/Standard) | LocalDB incluido en VS |
| **Git** | 2.x | Control de versiones |
| **IDE** | VS 2022 / VS Code / Rider | Con extensiones C# |

## Verificación de Entorno

```bash
# .NET SDK
dotnet --list-sdks
# Debe mostrar 9.0.x

# SQL Server LocalDB
sqllocaldb info
# Debe mostrar "MSSQLLocalDB" v15+

# Git
git --version
```

## Permisos Necesarios

- **Desarrollo**: Usuario local con acceso a LocalDB (Windows Auth)
- **Producción**: Usuario DB con `db_owner` en `InstitutoDB` o permisos `CREATE TABLE`, `INSERT`, `UPDATE`, `DELETE`, `SELECT`
- **FS**: Escritura en carpeta logs (si file logging)
- **Puerto**: 5089 (dev) / 8080 (prod) disponible

---

## Estructura de Carpetas Esperada

```
SitioWebInstituto/
├── instituto/
│   ├── backend/           # Proyecto ASP.NET Core
│   │   ├── bin/           # Compilado (gitignored)
│   │   ├── obj/           # Obj (gitignored)
│   │   ├── Database/      # Scripts SQL
│   │   └── ...
│   ├── Frondend/          # Proyecto Vue 3 (separado)
│   ├── Docs/              # Esta documentación
│   └── brana.sln          # Solution file
```

---

## Base de Datos

### Opción A: LocalDB (Desarrollo - Default)
- Ya instalado con Visual Studio / Build Tools
- Connection string: `Server=(localdb)\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;`
- Sin configuración extra

### Opción B: SQL Server Express / Developer / Standard
```bash
# Instalar (Windows)
# Descargar de https://www.microsoft.com/sql-server/sql-server-downloads

# Verificar instancia
sqlcmd -S localhost -Q "SELECT @@VERSION"

# Crear login para app (producción)
CREATE LOGIN [app_user] WITH PASSWORD = 'StrongPass123!';
CREATE USER [app_user] FOR LOGIN [app_user];
ALTER ROLE [db_owner] ADD MEMBER [app_user];
```

### Opción C: Docker (SQL Server Linux)
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```
Connection string: `Server=localhost,1433;Database=InstitutoDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;`

---

## Frontend (Vue 3) - Referencia

> El frontend está en `instituto/Frondend/` - documentación separada.

**Requisitos**:
- Node.js 18+ / 20+
- npm 9+ / pnpm / yarn
- Vite 5+

**Comandos**:
```bash
cd instituto/Frondend
npm install
npm run dev      # http://localhost:5173
npm run build    # Dist para producción
```

**Proxy Dev** (`vite.config.ts`):
```typescript
server: {
  proxy: {
    '/api': {
      target: 'http://localhost:5089',
      changeOrigin: true
    }
  }
}
```

---

## Puertos por Defecto

| Servicio | Puerto Dev | Puerto Prod | Configurable En |
|----------|------------|-------------|-----------------|
| Backend API | 5089 | 8080 | `ASPNETCORE_URLS` / `launchSettings.json` |
| Frontend Vite | 5173 | 80/443 (nginx) | `vite.config.ts` |
| SQL Server | 1433 (default instance) | 1433 | SQL Config Manager |
| LocalDB | Dinámico (named pipe) | N/A | `(localdb)\MSSQLLocalDB` |

---

## Verificación Rápida Pre-Despliegue

```bash
# 1. Compilar backend
cd instituto/backend
dotnet build --configuration Release

# 2. Ejecutar tests (si existen)
dotnet test

# 3. Verificar SQL script
sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/CreateDatabase.sql

# 4. Run backend
dotnet run --environment Development
# Debe mostrar: "Now listening on: http://localhost:5089"

# 5. Test endpoint salud
curl http://localhost:5089/
# "Backend corriendo correctamente!"

# 6. Test auth (crear admin si primera vez)
curl -X POST http://localhost:5089/api/setup/admin ...
curl -X POST http://localhost:5089/api/auth/login ...

# 7. Test CRUD
curl -H "Authorization: Bearer <token>" http://localhost:5089/api/carreras
```