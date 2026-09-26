# Pasos de Instalación

## 1. Clonar Repositorio

```bash
git clone <url-repositorio>
cd SitioWebInstituto/instituto
```

## 2. Base de Datos

### Ejecutar Script SQL

**Opción A: SSMS (GUI)**
1. Abrir SQL Server Management Studio
2. Conectar a: `(localdb)\MSSQLLocalDB` (Windows Auth)
3. Archivo → Abrir → `backend/Database/CreateDatabase.sql`
4. Ejecutar (F5)

**Opción B: Línea de comandos**
```bash
cd backend
sqlcmd -S "(localdb)\MSSQLLocalDB" -i Database/CreateDatabase.sql
```

**Opción C: PowerShell**
```powershell
Invoke-Sqlcmd -ServerInstance "(localdb)\MSSQLLocalDB" -InputFile "backend/Database/CreateDatabase.sql"
```

**Verificación**:
```sql
USE InstitutoDB;
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
-- Debe retornar: Administradores, Alumnos, Carreras, Profesores
```

## 3. Configuración Backend

### Development (Local)
```bash
cd backend

# Opción A: User Secrets (recomendado)
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:SqlServer" "Server=(localdb)\MSSQLLocalDB;Database=InstitutoDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "TuClaveDesarrollo32CharsMinimo!!!"

# Opción B: appsettings.Development.json (ya existe con valores por defecto)
# Editar si necesario
```

### Production
```bash
# Variables de entorno (ver 02-variables-entorno.md)
export ConnectionStrings__SqlServer="Server=prod-sql;Database=InstitutoDB;User Id=app;Password=...;"
export Jwt__Key="ProduccionJwtKey32CharsMinimum!!!"
export Jwt__Issuer="InstitutoTupacAmaru"
export Jwt__Audience="InstitutoTupacAmaruAdmin"
export ASPNETCORE_ENVIRONMENT="Production"
export ASPNETCORE_URLS="http://0.0.0.0:8080"
```

## 4. Restaurar Dependencias y Compilar

```bash
cd backend
dotnet restore
dotnet build --configuration Release
```

## 5. Crear Primer Administrador (Solo Primera Vez)

```bash
# Asegurar que backend esté corriendo en Development
dotnet run --environment Development &

# Esperar a "Now listening on: http://localhost:5089"

# Crear admin
curl -X POST http://localhost:5089/api/setup/admin \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Admin",
    "apellido": "Sistema",
    "email": "admin@tupac.edu",
    "password": "AdminSeguro2026!",
    "role": "Admin"
  }'

# Respuesta esperada:
# {"mensaje":"Administrador 'admin@tupac.edu' creado correctamente. Este endpoint ya no puede volver a usarse."}
```

## 6. Verificar Login

```bash
curl -X POST http://localhost:5089/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@tupac.edu","password":"AdminSeguro2026!"}'

# Respuesta: { "token": "...", "expiraEn": "...", "admin": {...} }
```

## 7. Probar Endpoints CRUD

```bash
# Guardar token
TOKEN="<token-del-login>"

# Carreras (público GET)
curl http://localhost:5089/api/carreras

# Alumnos (auth)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5089/api/alumnos

# Crear carrera (auth)
curl -X POST http://localhost:5089/api/carreras \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"nombre":"Tecnicatura en IA","duracionAnios":3,"estado":"Activa"}'

# Listado (auth)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5089/api/listado
```

---

## 8. Frontend (Vue 3) - Instalación Paralela

```bash
cd ../Frondend
npm install
npm run dev
# Abre http://localhost:5173
```

**Configurar proxy** (ya incluido en `vite.config.ts`):
```typescript
export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5089',
        changeOrigin: true
      }
    }
  }
})
```

---

## 9. Publicar para Producción

### Backend - Self-Contained
```bash
cd backend
dotnet publish -c Release -o ./publish --self-contained false
# Copiar carpeta publish/ al servidor
```

### Backend - Framework-Dependent (Recomendado si .NET instalado en servidor)
```bash
dotnet publish -c Release -o ./publish
```

### Frontend - Build Estático
```bash
cd ../Frondend
npm run build
# Copiar carpeta dist/ a servidor web (nginx/IIS/Apache)
```

---

## 10. Configuración Servidor Producción (Linux/Windows)

### systemd Service (Linux)
```ini
# /etc/systemd/system/instituto-backend.service
[Unit]
Description=Instituto Backend API
After=network.target

[Service]
Type=notify
WorkingDirectory=/var/www/instituto/backend
ExecStart=/usr/bin/dotnet /var/www/instituto/backend/Backend.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=instituto-backend
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
# Variables de entorno desde /etc/environment o EnvironmentFile

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable instituto-backend
sudo systemctl start instituto-backend
sudo systemctl status instituto-backend
```

### IIS (Windows)
1. Instalar **ASP.NET Core Hosting Bundle**
2. Crear Application Pool: "No Managed Code", Integrated
3. Sitio Web → Physical Path: `C:\inetpub\instituto\backend\publish`
4. Configurar `web.config` (generado por `dotnet publish`)
5. Variables de entorno en **Advanced Settings** → **Environment Variables**

### Nginx Reverse Proxy (Linux)
```nginx
# /etc/nginx/sites-available/instituto-api
server {
    listen 80;
    server_name api.tupac.edu;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/instituto-api /etc/nginx/sites-enabled/
sudo nginx -t && sudo systemctl reload nginx
```

---

## Checklist Post-Instalación

- [ ] BD creada y accesible
- [ ] Backend compila sin errores
- [ ] Primer admin creado (`/api/setup/admin`)
- [ ] Login funciona (`/api/auth/login`)
- [ ] JWT token válido (8hs expiración)
- [ ] Endpoints CRUD responden 200/201
- [ ] CORS permite frontend
- [ ] Logs visibles (Console / File)
- [ ] Health check `/health` responde 200 (si configurado)
- [ ] Frontend conecta a backend correctamente