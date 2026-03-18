# API de Reservas de Gastronomía - Arroyo Seco

## Descripción
Sistema completo de reservas para establecimientos de gastronomía con notificaciones por correo automáticas.

---

## 📋 Endpoints

### 1. Crear Reserva de Gastronomía

**Endpoint:**
```
POST /api/ReservasGastronomia
```

**Autenticación:** ✅ Requerida (JWT Bearer Token)

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "establecimientoId": 1,
  "fecha": "2025-12-15T19:30:00Z",
  "numeroPersonas": 4,
  "mesaId": null
}
```

**Parámetros:**
| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| `establecimientoId` | int | ✅ Sí | ID del establecimiento/restaurante |
| `fecha` | datetime (ISO 8601) | ✅ Sí | Fecha y hora de la reserva |
| `numeroPersonas` | int | ✅ Sí | Cantidad de personas (> 0) |
| `mesaId` | int | ❌ No | ID de mesa específica (opcional) |

**Respuesta Exitosa (201 Created):**
```json
{
  "id": 5,
  "establecimientoId": 1,
  "fecha": "2025-12-15T19:30:00Z",
  "numeroPersonas": 4,
  "estado": "Pendiente"
}
```

**Códigos de Error:**
| Código | Descripción |
|--------|-------------|
| 400 | Datos inválidos (número de personas ≤ 0, fecha inválida, etc.) |
| 401 | No autenticado |
| 404 | Establecimiento o mesa no encontrados |
| 500 | Error interno del servidor |

**Acciones Automáticas al Crear:**
- ✅ Reserva guardada en BD con estado "Pendiente"
- ✅ Correo enviado al cliente
- ✅ Correo enviado al oferente del establecimiento
- ✅ Notificación guardada en BD

---

### 2. Obtener Reserva por ID

**Endpoint:**
```
GET /api/ReservasGastronomia/{id}
```

**Autenticación:** ✅ Requerida

**Path Parameters:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | int | ID de la reserva |

**Respuesta Exitosa (200 OK):**
```json
{
  "id": 5,
  "establecimientoId": 1,
  "establecimientoNombre": "Cafetería Uteq",
  "mesaId": 3,
  "mesaNumero": 5,
  "usuarioId": "user-uuid-123",
  "fecha": "2025-12-15T19:30:00Z",
  "numeroPersonas": 4,
  "estado": "Pendiente",
  "total": 0.00
}
```

---

### 3. Obtener Reservas Activas

**Endpoint:**
```
GET /api/ReservasGastronomia/activas
```

**Autenticación:** ✅ Requerida

**Notas:**
- Clientes ven solo sus reservas
- Oferentes ven reservas de sus establecimientos
- Solo muestra estado "Pendiente" o "Confirmada" y fecha >= ahora

**Respuesta Exitosa (200 OK):**
```json
[
  {
    "id": 5,
    "establecimientoId": 1,
    "establecimientoNombre": "Cafetería Uteq",
    "mesaId": 3,
    "mesaNumero": 5,
    "usuarioId": "user-uuid-123",
    "fecha": "2025-12-15T19:30:00Z",
    "numeroPersonas": 4,
    "estado": "Pendiente",
    "total": 0.00
  }
]
```

---

### 4. Obtener Historial de Reservas

**Endpoint:**
```
GET /api/ReservasGastronomia/historial
```

**Autenticación:** ✅ Requerida

**Notas:**
- Clientes ven solo sus reservas pasadas
- Oferentes ven historial de sus establecimientos
- Muestra reservas con estado "Cancelada", "Completada" o fecha pasada

**Respuesta Exitosa (200 OK):**
```json
[
  {
    "id": 2,
    "establecimientoId": 1,
    "establecimientoNombre": "Cafetería Uteq",
    "mesaId": null,
    "mesaNumero": null,
    "usuarioId": "user-uuid-456",
    "fecha": "2025-11-20T19:00:00Z",
    "numeroPersonas": 2,
    "estado": "Completada",
    "total": 0.00
  }
]
```

---

### 5. Cambiar Estado de Reserva

**Endpoint:**
```
PATCH /api/ReservasGastronomia/{id}/estado
```

**Autenticación:** ✅ Requerida (Solo Admin o Oferente propietario)

**Path Parameters:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| `id` | int | ID de la reserva |

**Body (JSON):**
```json
{
  "estado": "Confirmada"
}
```

**Estados Válidos:**
- `Pendiente` - Inicial
- `Confirmada` - Aceptada por oferente
- `Cancelada` - Cancelada
- `Completada` - Completada

**Respuesta Exitosa (200 OK):**
```json
{
  "id": 5,
  "estado": "Confirmada"
}
```

**Acciones Automáticas al Cambiar Estado:**
- ✅ Correo enviado al cliente con notificación
- ✅ Estado actualizado en BD
- ✅ Notificación guardada en BD

**Correos Automáticos por Estado:**

| Estado | Asunto | Color |
|--------|--------|-------|
| **Confirmada** | "Tu reserva en gastronomía ha sido confirmada" | Verde (#27ae60) |
| **Cancelada** | "Tu reserva en gastronomía ha sido cancelada" | Rojo (#e74c3c) |
| **Completada** | "Tu reserva en gastronomía ha sido completada" | Azul (#3498db) |

---

## 🔄 Flujo de Integración en el Frontend

### Paso 1: Obtener Datos del Usuario
```javascript
// Headers con token
const headers = {
  'Authorization': `Bearer ${token}`,
  'Content-Type': 'application/json'
};
```

### Paso 2: Capturar Datos del Formulario
```javascript
const reservaData = {
  establecimientoId: parseInt(formData.establecimientoId),
  fecha: new Date(formData.fecha).toISOString(),
  numeroPersonas: parseInt(formData.numeroPersonas),
  mesaId: formData.mesaId ? parseInt(formData.mesaId) : null
};
```

### Paso 3: Enviar POST
```javascript
const response = await fetch('/api/ReservasGastronomia', {
  method: 'POST',
  headers: headers,
  body: JSON.stringify(reservaData)
});

if (response.status === 201) {
  const result = await response.json();
  // Redirigir a confirmación
  window.location.href = `/reservas/gastronomia/${result.id}`;
} else {
  const error = await response.json();
  // Mostrar error
  console.error(error);
}
```

### Paso 4: Rutas de Redirección
```
✅ Exitoso → /reservas/gastronomia/{id}
❌ Error → Mostrar mensaje en modal/alert
```

---

## 📧 Sistema de Notificaciones

### Correos Automáticos Enviados

#### 1. Al Crear Reserva
**Para:** Cliente  
**Asunto:** Notificación personalizada según el establecimiento  
**Contenido:** Detalles de la reserva (fecha, personas, establecimiento)

#### 2. Al Confirmar Reserva
**Para:** Cliente  
**Asunto:** "Tu reserva en gastronomía ha sido confirmada"  
**Contenido:** 
- Establecimiento
- Fecha y hora
- Número de personas
- Total (si aplica)

#### 3. Al Cancelar Reserva
**Para:** Cliente  
**Asunto:** "Tu reserva en gastronomía ha sido cancelada"  
**Contenido:** Establecimiento y motivo

#### 4. Al Completar Reserva
**Para:** Cliente  
**Asunto:** "Tu reserva en gastronomía ha sido completada"  
**Contenido:** Agradecimiento y datos de la reserva

### Configuración SMTP (Mailtrap)
```
Host: live.smtp.mailtrap.io
Port: 587 (recomendado) o 465
Username: api
Password: 82a0f1f4819b1d9981514b479989056a
From Email: noreply@arroyoseco.com
From Name: Arroyo Seco
```

---

## ⚠️ Validaciones

| Campo | Validación | Ejemplo |
|-------|-----------|---------|
| `numeroPersonas` | > 0 | ❌ 0, ❌ -5, ✅ 1-999 |
| `fecha` | ISO 8601, futura | ✅ "2025-12-15T19:30:00Z" |
| `establecimientoId` | Debe existir | ✅ ID válido |
| `mesaId` | Si se envía, debe existir y estar disponible | ✅ ID válido o null |

---

## 🔐 Roles y Permisos

### Cliente
- ✅ Crear reserva
- ✅ Ver sus propias reservas
- ✅ Ver historial personal

### Oferente
- ✅ Ver reservas de sus establecimientos
- ✅ Cambiar estado de reservas
- ✅ Ver historial de sus establecimientos

### Admin
- ✅ Todo acceso

---

## 📝 Ejemplos de Uso

### Ejemplo 1: Crear Reserva
```bash
curl -X POST "https://arroyosecomascercade.nominias/api/ReservasGastronomia" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "establecimientoId": 1,
    "fecha": "2025-12-15T19:30:00Z",
    "numeroPersonas": 4,
    "mesaId": null
  }'
```

### Ejemplo 2: Cambiar Estado a Confirmada
```bash
curl -X PATCH "https://arroyosecomascercade.nominias/api/ReservasGastronomia/5/estado" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "estado": "Confirmada"
  }'
```

### Ejemplo 3: Obtener Reservas Activas
```bash
curl -X GET "https://arroyosecomascercade.nominias/api/ReservasGastronomia/activas" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## 🐛 Troubleshooting

| Problema | Causa | Solución |
|----------|-------|----------|
| Error 401 | Token inválido o expirado | Obtener nuevo token |
| Error 404 | Establecimiento no existe | Verificar ID del establecimiento |
| Error 400 | Datos inválidos | Revisar formato JSON y tipos |
| Correo no llega | SMTP no configurado | Verificar credenciales Mailtrap |

---

## 📞 Contacto y Soporte

- **Backend API:** https://arroyosecomascercade.nominias
- **Frontend:** https://arroyosecoservices.vercel.app
- **Email:** noreply@arroyoseco.com

---

**Última actualización:** 26 de noviembre, 2025  
**Versión:** 1.0
