# 🚀 Guía de Despliegue GRATIS - Render + Clever Cloud

## 📋 Requisitos Previos
- [ ] Cuenta GitHub (gratis)
- [ ] Cuenta Render (gratis) - https://render.com
- [ ] Cuenta Clever Cloud (gratis) - https://www.clever-cloud.com

---

## PASO 1️⃣: Crear Base de Datos en Clever Cloud

1. Ve a https://www.clever-cloud.com y crea una cuenta
2. Click en "Create an application"
3. Selecciona "MySQL"
4. Elige el plan **FREE** (256MB, 5 conexiones)
5. Dale un nombre: `arroyoseco-db`
6. Click en "Create"

### ⚠️ GUARDA ESTAS CREDENCIALES (las necesitarás):
```
Host: xxxxxx-mysql.services.clever-cloud.com
Port: 3306
Database: xxxxxx
User: xxxxxx
Password: xxxxxx
```

### 🔧 Crear la Cadena de Conexión
Combina los datos así:
```
Server=HOST;Port=3306;Database=DATABASE;User=USER;Password=PASSWORD;SslMode=Required;
```

Ejemplo:
```
Server=bq8abc-mysql.services.clever-cloud.com;Port=3306;Database=bq8abc;User=uabc123;Password=xyz789;SslMode=Required;
```

---

## PASO 2️⃣: Crear Nuevo Repositorio Git

### Opción A: Desde cero (recomendado)
```powershell
# En la carpeta del proyecto
cd c:\Users\david\Downloads\arroyoSeco-feature-alojamiento-oferente\arroyoSeco-feature-alojamiento-oferente

# Inicializar git
git init

# Agregar archivos
git add .

# Primer commit
git commit -m "Initial commit - Arroyo Seco API"

# Crear repo en GitHub (usa la interfaz web)
# Luego conecta:
git remote add origin https://github.com/TU_USUARIO/arroyoseco-api.git
git branch -M main
git push -u origin main
```

### Opción B: Clonar y limpiar repo existente
Si prefieres limpiar el actual, dime y te ayudo.

---

## PASO 3️⃣: Configurar Render

1. Ve a https://dashboard.render.com
2. Click en "New +" → "Web Service"
3. Conecta tu repositorio de GitHub
4. Selecciona el repositorio `arroyoseco-api`

### Configuración del servicio:
- **Name**: `arroyoseco-api`
- **Runtime**: `.NET`
- **Build Command**: `dotnet publish arroyoSeco/arroyoSeco.API.csproj -c Release -o out`
- **Start Command**: `dotnet out/arroyoSeco.API.dll`
- **Plan**: `Free`

### ⚙️ Variables de Entorno (Environment Variables):

Agrega estas variables en Render:

| Key | Value |
|-----|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://0.0.0.0:10000` |
| `ConnectionStrings__DefaultConnection` | Tu cadena de Clever Cloud (paso 1) |
| `Jwt__Key` | `CAmbia_Esta_Clave_Secreta_Larga_y_Segura_Para_JWT_Produccion_2024` |
| `Jwt__Issuer` | `arroyoSeco` |
| `Jwt__Audience` | `arroyoSeco-client` |
| `Jwt__ExpirationMinutes` | `120` |
| `SeedAdmin__Email` | `admin@arroyo.com` |
| `SeedAdmin__Password` | `Admin123!` |

4. Click en "Create Web Service"

---

## PASO 4️⃣: Migrar la Base de Datos

Clever Cloud te da acceso directo a MySQL:

### Opción A: Desde tu local con MySQL Workbench
1. Abre MySQL Workbench
2. Crea nueva conexión con los datos de Clever Cloud
3. Exporta tu base de datos local:
   ```sql
   mysqldump -u root -p arroyoseco > arroyoseco_backup.sql
   ```
4. Importa en Clever Cloud desde Workbench

### Opción B: Desde PowerShell
```powershell
# Exportar local
mysqldump -u root -p arroyoseco > arroyoseco_backup.sql

# Importar a Clever Cloud
Get-Content arroyoseco_backup.sql | mysql -h HOST_CLEVER -u USER_CLEVER -p DATABASE_CLEVER
```

### Opción C: Solo estructura (Entity Framework lo llena)
En tu próximo despliegue, Entity Framework creará las tablas automáticamente.

---

## ✅ VERIFICACIÓN FINAL

### 1. Render debe mostrar:
- ✅ Build exitoso
- ✅ Deployed (verde)
- ✅ URL pública: `https://arroyoseco-api.onrender.com`

### 2. Prueba tu API:
```
https://arroyoseco-api.onrender.com/api/Gastronomias
```

### 3. Prueba el login admin:
```
POST https://arroyoseco-api.onrender.com/api/auth/login
{
  "email": "admin@arroyo.com",
  "password": "Admin123!"
}
```

---

## 🎯 URLS FINALES

- **Backend API**: `https://arroyoseco-api.onrender.com`
- **Base de Datos**: Panel Clever Cloud
- **Logs**: Panel Render → Logs tab

---

## ⚠️ LIMITACIONES PLAN GRATUITO

### Render Free:
- ❌ Se duerme después de 15 min inactividad
- ❌ Primera petición tarda ~30-60 segundos (cold start)
- ✅ 750 horas/mes (suficiente para demo)

### Clever Cloud MySQL Free:
- ❌ 256MB de datos
- ❌ 5 conexiones simultáneas
- ✅ Suficiente para pruebas y demos

---

## 🔄 ACTUALIZACIONES

Cada vez que hagas `git push` a tu repositorio, Render automáticamente:
1. Descarga el código
2. Compila el proyecto
3. Despliega la nueva versión

¡Es automático! 🎉

---

## 🆘 TROUBLESHOOTING

### Error: "Database connection failed"
- Verifica la cadena de conexión en Variables de Entorno
- Asegúrate de incluir `SslMode=Required;`

### Error: "Application failed to start"
- Revisa los logs en Render
- Verifica que el `StartCommand` sea correcto

### Base de datos vacía
- Ejecuta las migraciones o importa el backup SQL

---

## 📞 SIGUIENTE PASO

¿Quieres que te ayude con alguno de estos pasos específicamente?
1. Crear el nuevo repositorio Git
2. Configurar Render
3. Migrar la base de datos
4. Conectar el frontend

