v# Sistema de Solicitudes de Oferente

## Descripción

El sistema de solicitudes permite que usuarios interesados en convertirse en oferentes envíen solicitudes que luego son revisadas y aprobadas/rechazadas por los administradores.

## Flujo Completo

### 1. Usuario Envía Solicitud

Un usuario (no autenticado) completa un formulario de solicitud para convertirse en oferente.

**Endpoint:**
```http
POST /api/solicitudesoferente
Content-Type: application/json

{
  "nombreSolicitante": "Juan Pérez",
  "telefono": "+54 9 11 1234-5678",
  "nombreNegocio": "Restaurante El Buen Sabor",
  "correo": "juan@restaurante.com",
  "mensaje": "Quiero ofrecer mis servicios gastronómicos",
  "tipoSolicitado": 2
}
```

**Campos:**
- `nombreSolicitante`: Nombre completo del solicitante
- `telefono`: Número de contacto
- `nombreNegocio`: Nombre del negocio que desea registrar
- `correo`: Email (será el username si se aprueba)
- `mensaje`: Mensaje opcional explicando su solicitud
- `tipoSolicitado`: 
  - `1` = Alojamiento
  - `2` = Gastronomía
  - `3` = Ambos (default)

**Respuesta:**
```json
{
  "id": 1
}
```

**Qué sucede:**
1. Se crea la solicitud con estado "Pendiente"
2. Se asigna `FechaSolicitud` automáticamente
3. **Se notifica a TODOS los administradores** sobre la nueva solicitud
4. Los admins reciben notificación: "Nueva solicitud de oferente - Juan Pérez (Restaurante El Buen Sabor) - Gastronomia"

---

### 2. Admin Revisa Solicitudes Pendientes

Los administradores pueden ver todas las solicitudes o filtrar por estado.

**Listar todas las solicitudes:**
```http
GET /api/admin/oferentes/solicitudes
Authorization: Bearer {admin-token}
```

**Filtrar por estado:**
```http
GET /api/admin/oferentes/solicitudes?estatus=Pendiente
Authorization: Bearer {admin-token}
```

**Respuesta:**
```json
[
  {
    "id": 1,
    "nombreSolicitante": "Juan Pérez",
    "telefono": "+54 9 11 1234-5678",
    "nombreNegocio": "Restaurante El Buen Sabor",
    "correo": "juan@restaurante.com",
    "mensaje": "Quiero ofrecer mis servicios gastronómicos",
    "tipoSolicitado": 2,
    "estatus": "Pendiente",
    "fechaSolicitud": "2025-11-21T18:00:00Z",
    "fechaRespuesta": null,
    "adminId": null
  }
]
```

**Contar solicitudes pendientes:**
```http
GET /api/solicitudesoferente/pendientes/count
```

Respuesta:
```json
{
  "count": 5
}
```

---

### 3. Admin Aprueba la Solicitud

El administrador decide aprobar la solicitud.

**Endpoint:**
```http
POST /api/admin/oferentes/solicitudes/1/aprobar
Authorization: Bearer {admin-token}
```

**Qué sucede:**
1. **Busca si el usuario ya existe** por el email de la solicitud
2. **Si no existe:**
   - Crea un usuario en `AspNetUsers` con email y contraseña temporal
   - La contraseña temporal tiene formato: `TempXXXXXXXX!` (donde X son caracteres aleatorios)
   - Asigna el rol "Oferente"
3. **Si ya existe:**
   - Reutiliza el usuario existente
4. **Crea el registro en tabla Oferentes:**
   - `Id`: ID del usuario Identity
   - `Nombre`: Toma el valor de `NombreNegocio` de la solicitud
   - `Tipo`: Usa el valor de `TipoSolicitado` (Alojamiento, Gastronomía o Ambos)
   - `NumeroAlojamientos`: 0
5. **Actualiza la solicitud:**
   - `Estatus`: "Aprobada"
   - `FechaRespuesta`: Fecha actual
6. **Notifica al usuario:**
   - Título: "Solicitud aprobada"
   - Mensaje: "Tu solicitud para ser oferente de {Tipo} fue aprobada. Contraseña temporal enviada a tu correo."

**Respuesta:**
```json
{
  "id": "uuid-del-usuario",
  "email": "juan@restaurante.com",
  "tipo": 2
}
```

**Importante:** En un sistema real, deberías enviar un email con la contraseña temporal. Actualmente solo se genera pero no se envía.

---

### 4. Admin Rechaza la Solicitud

Si el administrador decide rechazar la solicitud.

**Endpoint:**
```http
POST /api/admin/oferentes/solicitudes/1/rechazar
Authorization: Bearer {admin-token}
```

**Qué sucede:**
1. Actualiza `Estatus` a "Rechazada"
2. Asigna `FechaRespuesta` con la fecha actual
3. **NO se crea usuario ni oferente**
4. **NO se envía notificación** (opcional: podrías agregar una)

**Respuesta:**
```http
204 No Content
```

---

## Endpoints Completos

### Endpoints Públicos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/solicitudesoferente` | Crear nueva solicitud |
| GET | `/api/solicitudesoferente` | Listar solicitudes (opcionalmente filtrar por `?estatus=Pendiente`) |
| GET | `/api/solicitudesoferente/{id}` | Obtener solicitud por ID |
| GET | `/api/solicitudesoferente/pendientes/count` | Contar solicitudes pendientes |

### Endpoints Admin

| Método | Endpoint | Descripción | Requiere |
|--------|----------|-------------|----------|
| GET | `/api/admin/oferentes/solicitudes` | Listar todas las solicitudes | Admin |
| GET | `/api/admin/oferentes/solicitudes?estatus={estatus}` | Filtrar por estado | Admin |
| POST | `/api/admin/oferentes/solicitudes/{id}/aprobar` | Aprobar solicitud | Admin |
| POST | `/api/admin/oferentes/solicitudes/{id}/rechazar` | Rechazar solicitud | Admin |

---

## Estados de Solicitud

- **Pendiente**: Solicitud recién creada, esperando revisión
- **Aprobada**: Admin aprobó la solicitud, usuario creado
- **Rechazada**: Admin rechazó la solicitud

---

## Notificaciones

### Cuando se Crea una Solicitud

**Destinatarios:** Todos los usuarios con rol "Admin"

**Contenido:**
```json
{
  "titulo": "Nueva solicitud de oferente",
  "mensaje": "Solicitud de {NombreSolicitante} ({NombreNegocio}) - {TipoSolicitado}",
  "tipo": "SolicitudOferente",
  "urlAccion": "/admin/solicitudes/{id}"
}
```

### Cuando se Aprueba una Solicitud

**Destinatario:** Usuario cuya solicitud fue aprobada

**Contenido:**
```json
{
  "titulo": "Solicitud aprobada",
  "mensaje": "Tu solicitud para ser oferente de {Tipo} fue aprobada. Contraseña temporal enviada a tu correo.",
  "tipo": "SolicitudOferente",
  "urlAccion": null
}
```

---

## Ejemplo Completo de Uso

### Paso 1: Usuario solicita ser oferente de gastronomía

```bash
curl -X POST https://localhost:7190/api/solicitudesoferente \
  -H "Content-Type: application/json" \
  -d '{
    "nombreSolicitante": "María González",
    "telefono": "+54 9 11 9876-5432",
    "nombreNegocio": "Café del Centro",
    "correo": "maria@cafedelcentro.com",
    "mensaje": "Tengo una cafetería en el centro y quiero ofrecer mis servicios",
    "tipoSolicitado": 2
  }'
```

**Respuesta:**
```json
1
```

**Efecto:** Todos los admins reciben notificación.

---

### Paso 2: Admin consulta sus notificaciones

```bash
curl -X GET https://localhost:7190/api/notificaciones \
  -H "Authorization: Bearer {admin-token}"
```

**Respuesta:**
```json
[
  {
    "id": 1,
    "titulo": "Nueva solicitud de oferente",
    "mensaje": "Solicitud de María González (Café del Centro) - Gastronomia",
    "tipo": "SolicitudOferente",
    "urlAccion": "/admin/solicitudes/1",
    "fecha": "2025-11-21T18:30:00Z",
    "leida": false
  }
]
```

---

### Paso 3: Admin ve las solicitudes pendientes

```bash
curl -X GET https://localhost:7190/api/admin/oferentes/solicitudes?estatus=Pendiente \
  -H "Authorization: Bearer {admin-token}"
```

**Respuesta:**
```json
[
  {
    "id": 1,
    "nombreSolicitante": "María González",
    "telefono": "+54 9 11 9876-5432",
    "nombreNegocio": "Café del Centro",
    "correo": "maria@cafedelcentro.com",
    "mensaje": "Tengo una cafetería en el centro y quiero ofrecer mis servicios",
    "tipoSolicitado": 2,
    "estatus": "Pendiente",
    "fechaSolicitud": "2025-11-21T18:30:00Z",
    "fechaRespuesta": null,
    "adminId": null
  }
]
```

---

### Paso 4: Admin aprueba la solicitud

```bash
curl -X POST https://localhost:7190/api/admin/oferentes/solicitudes/1/aprobar \
  -H "Authorization: Bearer {admin-token}"
```

**Respuesta:**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "email": "maria@cafedelcentro.com",
  "tipo": 2
}
```

**Efecto:**
1. Se crea usuario con email `maria@cafedelcentro.com`
2. Se genera contraseña temporal (ej: `Tempab12cd34!`)
3. Se crea oferente de tipo Gastronomía
4. María recibe notificación de aprobación

---

### Paso 5: María inicia sesión con su cuenta nueva

```bash
curl -X POST https://localhost:7190/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "maria@cafedelcentro.com",
    "password": "Tempab12cd34!"
  }'
```

**Respuesta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "maria@cafedelcentro.com",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "role": "Oferente"
}
```

---

## Mejoras Recomendadas

### 1. Envío de Email con Contraseña Temporal

Actualmente la contraseña temporal se genera pero no se envía. Deberías:

```csharp
// En OferentesAdminController.Aprobar()
await _emailService.SendAsync(
    email,
    "Cuenta de Oferente Aprobada",
    $"Tu solicitud fue aprobada. Tu contraseña temporal es: {tempPass}"
);
```

### 2. Forzar Cambio de Contraseña

Agregar un flag `MustChangePassword` al usuario:

```csharp
user.MustChangePassword = true;
```

Y validar en el login que cambie la contraseña antes de usar el sistema.

### 3. Comentarios del Admin

Permitir que el admin agregue comentarios al aprobar/rechazar:

```csharp
public record AprobarSolicitudDto(string? Comentario);

[HttpPost("solicitudes/{id}/aprobar")]
public async Task<IActionResult> Aprobar(int id, [FromBody] AprobarSolicitudDto dto)
{
    // ... código existente
    s.ComentarioAdmin = dto.Comentario;
}
```

### 4. Límite de Reintentos

Evitar spam de solicitudes del mismo correo:

```csharp
var existentes = await _db.SolicitudesOferente
    .Where(s => s.Correo == solicitud.Correo && s.Estatus == "Pendiente")
    .CountAsync();

if (existentes > 0)
    return Conflict("Ya tienes una solicitud pendiente");
```

---

## Resumen

✅ Usuario envía solicitud → Admins reciben notificación  
✅ Admin revisa solicitudes pendientes  
✅ Admin aprueba → Se crea usuario + oferente con el tipo solicitado  
✅ Admin rechaza → Solicitud marcada como rechazada  
✅ Sistema incluye tipo de oferente en solicitudes  
✅ Contador de solicitudes pendientes disponible  

**Importante:** Recuerda implementar el envío de emails para notificar la contraseña temporal al usuario aprobado.
