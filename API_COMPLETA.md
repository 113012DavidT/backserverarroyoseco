# API Completa - ArroyoSeco

Documentación completa de todos los endpoints disponibles en el sistema.

**Base URL:** `https://localhost:7190`

---

## 📋 Tabla de Contenidos

1. [Autenticación](#autenticación)
2. [Alojamientos](#alojamientos)
3. [Gastronomía](#gastronomía)
4. [Reservas](#reservas)
5. [Oferentes](#oferentes)
6. [Solicitudes de Oferente](#solicitudes-de-oferente)
7. [Notificaciones](#notificaciones)
8. [Storage (Archivos)](#storage-archivos)

---

## 🔐 Autenticación

### Registrar Usuario

**Endpoint:** `POST /api/auth/register`

**Request:**
```json
{
  "email": "usuario@example.com",
  "password": "Password123!",
  "role": "Cliente",  // "Cliente", "Oferente", o "Admin"
  "tipoOferente": 2   // Opcional, solo si role="Oferente": 1=Alojamiento, 2=Gastronomia, 3=Ambos
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "usuario@example.com",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "role": "Cliente"
}
```

**Errores:**
- `400 Bad Request`: Email ya existe o validación fallida
- `500 Internal Server Error`: Error al crear usuario

**Notas:**
- Si `role="Oferente"` se crea automáticamente el registro en tabla Oferentes
- `tipoOferente` default es `3` (Ambos) si no se especifica

---

### Iniciar Sesión

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "email": "usuario@example.com",
  "password": "Password123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "usuario@example.com",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "role": "Cliente"
}
```

**Errores:**
- `401 Unauthorized`: Credenciales incorrectas

---

## 🏠 Alojamientos

### Listar Todos los Alojamientos

**Endpoint:** `GET /api/alojamientos`

**Auth:** No requerida

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "oferenteId": "uuid-oferente",
    "nombre": "Cabaña del Bosque",
    "ubicacion": "San Martín de los Andes",
    "precioPorNoche": 15000.00,
    "maxHuespedes": 6,
    "habitaciones": 3,
    "banos": 2,
    "fotoPrincipal": "/uploads/alojamientos/foto1.jpg"
  }
]
```

---

### Obtener Alojamiento por ID

**Endpoint:** `GET /api/alojamientos/{id}`

**Auth:** No requerida

**Response (200 OK):**
```json
{
  "id": 1,
  "oferenteId": "uuid-oferente",
  "nombre": "Cabaña del Bosque",
  "ubicacion": "San Martín de los Andes",
  "precioPorNoche": 15000.00,
  "maxHuespedes": 6,
  "habitaciones": 3,
  "banos": 2,
  "fotoPrincipal": "/uploads/alojamientos/foto1.jpg",
  "fotos": [
    {
      "id": 1,
      "url": "/uploads/alojamientos/foto1.jpg"
    },
    {
      "id": 2,
      "url": "/uploads/alojamientos/foto2.jpg"
    }
  ]
}
```

**Errores:**
- `404 Not Found`: Alojamiento no existe

---

### Crear Alojamiento

**Endpoint:** `POST /api/alojamientos`

**Auth:** Requerida (Rol: Oferente)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "nombre": "Cabaña del Bosque",
  "ubicacion": "San Martín de los Andes",
  "precioPorNoche": 15000.00,
  "maxHuespedes": 6,
  "habitaciones": 3,
  "banos": 2,
  "fotoPrincipal": "/uploads/alojamientos/foto1.jpg"
}
```

**Response (201 Created):**
```json
{
  "id": 1
}
```

**Errores:**
- `401 Unauthorized`: Token inválido o no proporcionado
- `403 Forbidden`: Usuario no es Oferente

**Notas:**
- El alojamiento se asocia automáticamente al oferente autenticado
- Primero sube la foto con `/api/storage/upload`, luego usa la URL retornada

---

### Actualizar Alojamiento

**Endpoint:** `PUT /api/alojamientos/{id}`

**Auth:** Requerida (Rol: Oferente, propietario del alojamiento)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "nombre": "Cabaña del Bosque Premium",
  "ubicacion": "San Martín de los Andes",
  "precioPorNoche": 18000.00,
  "maxHuespedes": 6,
  "habitaciones": 3,
  "banos": 2,
  "fotoPrincipal": "/uploads/alojamientos/foto1.jpg"
}
```

**Response (204 No Content)**

**Errores:**
- `401 Unauthorized`: Token inválido
- `403 Forbidden`: No eres el propietario
- `404 Not Found`: Alojamiento no existe

---

### Eliminar Alojamiento

**Endpoint:** `DELETE /api/alojamientos/{id}`

**Auth:** Requerida (Rol: Oferente, propietario del alojamiento)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (204 No Content)**

**Errores:**
- `401 Unauthorized`: Token inválido
- `403 Forbidden`: No eres el propietario
- `404 Not Found`: Alojamiento no existe

---

### Agregar Foto a Alojamiento

**Endpoint:** `POST /api/alojamientos/{id}/fotos`

**Auth:** Requerida (Rol: Oferente, propietario del alojamiento)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "url": "/uploads/alojamientos/foto3.jpg"
}
```

**Response (200 OK):**
```json
{
  "id": 3
}
```

---

### Eliminar Foto de Alojamiento

**Endpoint:** `DELETE /api/alojamientos/{alojamientoId}/fotos/{fotoId}`

**Auth:** Requerida (Rol: Oferente, propietario del alojamiento)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (204 No Content)**

---

## 🍽️ Gastronomía

### Listar Todos los Establecimientos

**Endpoint:** `GET /api/gastronomias`

**Auth:** No requerida

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "oferenteId": "uuid-oferente",
    "nombre": "Restaurante El Buen Sabor",
    "ubicacion": "Av. Principal 123",
    "descripcion": "Cocina regional argentina",
    "fotoPrincipal": "/uploads/gastronomia/foto1.jpg",
    "menus": [
      {
        "id": 1,
        "nombre": "Menú Principal",
        "items": [
          {
            "id": 1,
            "nombre": "Bife de Chorizo",
            "descripcion": "500g con guarnición",
            "precio": 8500.00
          }
        ]
      }
    ],
    "mesas": [
      {
        "id": 1,
        "numero": 1,
        "capacidad": 4,
        "disponible": true
      }
    ]
  }
]
```

---

### Obtener Establecimiento por ID

**Endpoint:** `GET /api/gastronomias/{id}`

**Auth:** No requerida

**Response (200 OK):**
```json
{
  "id": 1,
  "oferenteId": "uuid-oferente",
  "nombre": "Restaurante El Buen Sabor",
  "ubicacion": "Av. Principal 123",
  "descripcion": "Cocina regional argentina",
  "fotoPrincipal": "/uploads/gastronomia/foto1.jpg",
  "menus": [...],
  "mesas": [...]
}
```

**Errores:**
- `404 Not Found`: Establecimiento no existe

---

### Crear Establecimiento

**Endpoint:** `POST /api/gastronomias`

**Auth:** Requerida (Rol: Oferente)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "nombre": "Restaurante El Buen Sabor",
  "ubicacion": "Av. Principal 123",
  "descripcion": "Cocina regional argentina",
  "fotoPrincipal": "/uploads/gastronomia/foto1.jpg"
}
```

**Response (201 Created):**
```json
{
  "id": 1
}
```

**Errores:**
- `401 Unauthorized`: Token inválido
- `403 Forbidden`: Usuario no es Oferente

---

### Listar Menús de un Establecimiento

**Endpoint:** `GET /api/gastronomias/{id}/menus`

**Auth:** No requerida

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "establecimientoId": 1,
    "nombre": "Menú Principal",
    "items": [
      {
        "id": 1,
        "menuId": 1,
        "nombre": "Bife de Chorizo",
        "descripcion": "500g con guarnición",
        "precio": 8500.00
      },
      {
        "id": 2,
        "menuId": 1,
        "nombre": "Ensalada Caesar",
        "descripcion": "Con pollo grillado",
        "precio": 4500.00
      }
    ]
  }
]
```

---

### Crear Menú

**Endpoint:** `POST /api/gastronomias/{id}/menus`

**Auth:** Requerida (Rol: Oferente, propietario del establecimiento)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "nombre": "Menú de Postres"
}
```

**Response (201 Created):**
```json
{
  "id": 2
}
```

---

### Agregar Item a Menú

**Endpoint:** `POST /api/gastronomias/{establecimientoId}/menus/{menuId}/items`

**Auth:** Requerida (Rol: Oferente, propietario del establecimiento)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "nombre": "Tiramisu",
  "descripcion": "Clásico italiano",
  "precio": 3500.00
}
```

**Response (201 Created):**
```json
{
  "id": 5
}
```

---

### Listar Mesas de un Establecimiento

**Endpoint:** `GET /api/gastronomias/{id}/mesas`

**Auth:** No requerida

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "establecimientoId": 1,
    "numero": 1,
    "capacidad": 4,
    "disponible": true
  },
  {
    "id": 2,
    "establecimientoId": 1,
    "numero": 2,
    "capacidad": 2,
    "disponible": false
  }
]
```

---

### Crear Mesa

**Endpoint:** `POST /api/gastronomias/{id}/mesas`

**Auth:** Requerida (Rol: Oferente, propietario del establecimiento)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "numero": 5,
  "capacidad": 6
}
```

**Response (201 Created):**
```json
{
  "id": 5
}
```

**Notas:**
- Las mesas se crean como `disponible: true` por defecto

---

### Actualizar Disponibilidad de Mesa

**Endpoint:** `PUT /api/gastronomias/{establecimientoId}/mesas/{mesaId}/disponibilidad`

**Auth:** Requerida (Rol: Oferente, propietario del establecimiento)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "disponible": false
}
```

**Response (204 No Content)**

---

### Verificar Disponibilidad de Mesa

**Endpoint:** `GET /api/gastronomias/{establecimientoId}/mesas/{mesaId}/disponibilidad`

**Auth:** No requerida

**Query Params:**
- `fecha` (DateTime): Fecha y hora de la reserva deseada
- `numeroPersonas` (int): Número de personas

**Ejemplo:**
```
GET /api/gastronomias/1/mesas/2/disponibilidad?fecha=2025-11-25T20:00:00&numeroPersonas=4
```

**Response (200 OK):**
```json
{
  "disponible": true,
  "mesa": {
    "id": 2,
    "numero": 2,
    "capacidad": 6
  }
}
```

---

### Crear Reserva en Restaurante

**Endpoint:** `POST /api/gastronomias/reservas`

**Auth:** Requerida (Rol: Cliente o Oferente)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "establecimientoId": 1,
  "mesaId": 2,
  "fecha": "2025-11-25T20:00:00",
  "numeroPersonas": 4,
  "total": 0  // Opcional, puede ser 0 para reservas sin pago anticipado
}
```

**Response (201 Created):**
```json
{
  "id": 1
}
```

**Errores:**
- `400 Bad Request`: Mesa no disponible en esa fecha
- `401 Unauthorized`: Token inválido

---

## 📅 Reservas (Alojamientos)

### Crear Reserva de Alojamiento

**Endpoint:** `POST /api/reservas`

**Auth:** Requerida (Rol: Cliente)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "alojamientoId": 1,
  "fechaInicio": "2025-12-01T14:00:00",
  "fechaFin": "2025-12-05T10:00:00",
  "numeroHuespedes": 4,
  "total": 60000.00
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "alojamientoId": 1,
  "usuarioId": "uuid-cliente",
  "fechaInicio": "2025-12-01T14:00:00",
  "fechaFin": "2025-12-05T10:00:00",
  "numeroHuespedes": 4,
  "estado": "Pendiente",
  "total": 60000.00
}
```

**Errores:**
- `401 Unauthorized`: Token inválido
- `403 Forbidden`: Usuario no es Cliente
- `404 Not Found`: Alojamiento no existe

---

### Listar Reservas del Usuario

**Endpoint:** `GET /api/reservas`

**Auth:** Requerida

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "alojamientoId": 1,
    "usuarioId": "uuid-cliente",
    "fechaInicio": "2025-12-01T14:00:00",
    "fechaFin": "2025-12-05T10:00:00",
    "numeroHuespedes": 4,
    "estado": "Confirmada",
    "total": 60000.00,
    "alojamiento": {
      "id": 1,
      "nombre": "Cabaña del Bosque",
      "ubicacion": "San Martín de los Andes"
    }
  }
]
```

---

### Obtener Reserva por ID

**Endpoint:** `GET /api/reservas/{id}`

**Auth:** Requerida (propietario de la reserva)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "alojamientoId": 1,
  "usuarioId": "uuid-cliente",
  "fechaInicio": "2025-12-01T14:00:00",
  "fechaFin": "2025-12-05T10:00:00",
  "numeroHuespedes": 4,
  "estado": "Confirmada",
  "total": 60000.00
}
```

**Errores:**
- `404 Not Found`: Reserva no existe o no tienes acceso

---

### Cancelar Reserva

**Endpoint:** `DELETE /api/reservas/{id}`

**Auth:** Requerida (propietario de la reserva)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (204 No Content)**

**Errores:**
- `404 Not Found`: Reserva no existe
- `403 Forbidden`: No eres el propietario

---

## 👥 Oferentes

### Listar Todos los Oferentes (Admin)

**Endpoint:** `GET /api/oferentes`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Query Params (opcional):**
- `tipo` (int): Filtrar por tipo (1=Alojamiento, 2=Gastronomia, 3=Ambos)

**Ejemplo:**
```
GET /api/oferentes?tipo=2
```

**Response (200 OK):**
```json
[
  {
    "id": "uuid-1",
    "nombre": "Hotel Paradise",
    "tipo": 1,
    "estado": "Activo",
    "numeroAlojamientos": 5,
    "email": "oferente1@example.com",
    "telefono": "+54 9 11 1234-5678",
    "alojamientos": [...],
    "establecimientos": []
  },
  {
    "id": "uuid-2",
    "nombre": "Restaurante El Buen Sabor",
    "tipo": 2,
    "estado": "Activo",
    "numeroAlojamientos": 0,
    "email": "oferente2@example.com",
    "telefono": "+54 9 11 8765-4321",
    "alojamientos": [],
    "establecimientos": [...]
  }
]
```

---

### Listar Oferentes de Alojamiento (Admin)

**Endpoint:** `GET /api/oferentes/alojamiento`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": "uuid-1",
    "nombre": "Hotel Paradise",
    "tipo": 1,
    "estado": "Activo",
    "numeroAlojamientos": 5,
    "email": "oferente1@example.com",
    "telefono": "+54 9 11 1234-5678",
    "alojamientos": [...]
  }
]
```

**Notas:**
- Retorna oferentes con `tipo = 1 (Alojamiento)` o `tipo = 3 (Ambos)`

---

### Listar Oferentes de Gastronomía (Admin)

**Endpoint:** `GET /api/oferentes/gastronomia`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": "uuid-2",
    "nombre": "Restaurante El Buen Sabor",
    "tipo": 2,
    "estado": "Activo",
    "numeroAlojamientos": 0,
    "email": "oferente2@example.com",
    "telefono": "+54 9 11 8765-4321",
    "establecimientos": [...]
  }
]
```

**Notas:**
- Retorna oferentes con `tipo = 2 (Gastronomía)` o `tipo = 3 (Ambos)`

---

### Cambiar Tipo de Oferente (Admin)

**Endpoint:** `PUT /api/oferentes/{id}/tipo`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "nuevoTipo": 3
}
```

**Response (200 OK):**
```json
{
  "id": "uuid-1",
  "nombre": "Hotel Paradise",
  "tipo": 3
}
```

**Errores:**
- `404 Not Found`: Oferente no existe
- `403 Forbidden`: Usuario no es Admin

---

### Obtener Perfil del Oferente (Oferente)

**Endpoint:** `GET /api/oferentes/perfil`

**Auth:** Requerida (Rol: Oferente)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": "uuid-oferente",
  "nombre": "Mi Restaurante",
  "tipo": 2,
  "estado": "Activo",
  "email": "oferente@example.com",
  "telefono": "+54 9 11 1234-5678"
}
```

**Errores:**
- `401 Unauthorized`: Token inválido o no es Oferente
- `404 Not Found`: Oferente no encontrado

**Notas:**
- Retorna los datos del oferente autenticado actualmente
- Útil para mostrar en la pantalla de perfil/edición

---

### Actualizar Perfil del Oferente (Oferente)

**Endpoint:** `PUT /api/oferentes/perfil`

**Auth:** Requerida (Rol: Oferente)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "nombre": "Nuevo Nombre del Negocio",
  "telefono": "+54 9 11 9999-8888"
}
```

**Response (200 OK):**
```json
{
  "id": "uuid-oferente",
  "nombre": "Nuevo Nombre del Negocio",
  "tipo": 2,
  "estado": "Activo",
  "email": "oferente@example.com",
  "telefono": "+54 9 11 9999-8888"
}
```

**Errores:**
- `401 Unauthorized`: Token inválido o no es Oferente
- `404 Not Found`: Oferente no encontrado
- `400 Bad Request`: Error al actualizar teléfono

**Notas:**
- Permite actualizar el nombre del negocio y el teléfono
- El email NO se puede cambiar desde este endpoint
- Ambos campos son opcionales (puedes enviar solo el que quieras actualizar)

---

### Crear Usuario Oferente (Admin)

**Endpoint:** `POST /api/admin/oferentes/usuarios`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "email": "oferente@example.com",
  "password": "Password123!",
  "nombre": "Mi Negocio"
}
```

**Response (201 Created):**
```json
{
  "id": "uuid-nuevo",
  "email": "oferente@example.com"
}
```

---

### Actualizar Oferente (Admin)

**Endpoint:** `PUT /api/admin/oferentes/{id}`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "nombre": "Nuevo Nombre del Negocio",
  "telefono": "+54 9 11 9999-8888",
  "tipo": 3
}
```

**Response (200 OK):**
```json
{
  "id": "uuid-oferente",
  "nombre": "Nuevo Nombre del Negocio",
  "tipo": 3,
  "estado": "Activo",
  "email": "oferente@example.com",
  "telefono": "+54 9 11 9999-8888"
}
```

**Errores:**
- `404 Not Found`: Oferente no encontrado
- `400 Bad Request`: Error al actualizar teléfono

**Notas:**
- Permite actualizar nombre, teléfono y tipo del oferente
- Todos los campos son opcionales (puedes enviar solo los que quieras actualizar)
- El tipo debe ser: 1=Alojamiento, 2=Gastronomía, 3=Ambos
- El email NO se puede cambiar desde este endpoint

---

### Eliminar Oferente (Admin)

**Endpoint:** `DELETE /api/admin/oferentes/{id}`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (204 No Content)**

**Errores:**
- `400 Bad Request`: Oferente tiene alojamientos/establecimientos asociados
- `404 Not Found`: Oferente no existe

---

### Cambiar Estado de Oferente (Admin)

**Endpoint:** `PUT /api/admin/oferentes/{id}/estado`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Request:**
```json
{
  "estado": "Activo"
}
```

**Response (200 OK):**
```json
{
  "id": "uuid-oferente",
  "estado": "Activo"
}
```

**Errores:**
- `404 Not Found`: Oferente no encontrado
- `403 Forbidden`: Usuario no es Admin

**Estados Válidos:**
- `Pendiente`: Oferente recién registrado, esperando verificación
- `Activo`: Oferente activo, puede operar normalmente
- `Inactivo`: Oferente temporalmente inactivo
- `Suspendido`: Oferente suspendido por el administrador

**Notas:**
- Los oferentes se crean con estado `Pendiente` por defecto
- Solo los oferentes con estado `Activo` deberían poder crear alojamientos/establecimientos
- Este endpoint permite al admin gestionar el ciclo de vida de los oferentes

---

## 📝 Solicitudes de Oferente

### Crear Solicitud (Público)

**Endpoint:** `POST /api/solicitudesoferente`

**Auth:** No requerida

**Request:**
```json
{
  "nombreSolicitante": "Juan Pérez",
  "telefono": "+54 9 11 1234-5678",
  "nombreNegocio": "Restaurante El Buen Sabor",
  "correo": "juan@restaurante.com",
  "mensaje": "Quiero ofrecer mis servicios gastronómicos",
  "tipoSolicitado": 2
}
```

**Response (201 Created):**
```json
1
```

**Notas:**
- Se notifica automáticamente a todos los administradores
- `tipoSolicitado`: 1=Alojamiento, 2=Gastronomía, 3=Ambos (default: 3)

---

### Listar Solicitudes (Público)

**Endpoint:** `GET /api/solicitudesoferente`

**Auth:** No requerida

**Query Params (opcional):**
- `estatus` (string): "Pendiente", "Aprobada", "Rechazada"

**Ejemplo:**
```
GET /api/solicitudesoferente?estatus=Pendiente
```

**Response (200 OK):**
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

---

### Obtener Solicitud por ID (Público)

**Endpoint:** `GET /api/solicitudesoferente/{id}`

**Auth:** No requerida

**Response (200 OK):**
```json
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
```

---

### Contar Solicitudes Pendientes

**Endpoint:** `GET /api/solicitudesoferente/pendientes/count`

**Auth:** No requerida

**Response (200 OK):**
```json
{
  "count": 5
}
```

---

### Listar Solicitudes (Admin)

**Endpoint:** `GET /api/admin/oferentes/solicitudes`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Query Params (opcional):**
- `estatus` (string): "Pendiente", "Aprobada", "Rechazada"

**Response (200 OK):**
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

---

### Aprobar Solicitud (Admin)

**Endpoint:** `POST /api/admin/oferentes/solicitudes/{id}/aprobar`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "id": "uuid-nuevo-usuario",
  "email": "juan@restaurante.com",
  "tipo": 2
}
```

**Notas:**
- Crea usuario Identity con contraseña temporal: `Temporal.123`
- Crea registro en tabla Oferentes con el tipo solicitado
- El oferente se crea con estado `Pendiente` por defecto
- Notifica al usuario aprobado con la contraseña temporal
- Actualiza solicitud a estado "Aprobada"
- Si el correo es nulo, se genera uno automático: `oferente{id}@arroyoseco.com`
- El teléfono se guarda en el campo PhoneNumber del usuario Identity

---

### Rechazar Solicitud (Admin)

**Endpoint:** `POST /api/admin/oferentes/solicitudes/{id}/rechazar`

**Auth:** Requerida (Rol: Admin)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (204 No Content)**

**Notas:**
- Actualiza solicitud a estado "Rechazada"
- NO crea usuario

---

## 🔔 Notificaciones

### Listar Notificaciones del Usuario

**Endpoint:** `GET /api/notificaciones`

**Auth:** Requerida

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "usuarioId": "uuid-usuario",
    "titulo": "Nueva solicitud de oferente",
    "mensaje": "Solicitud de Juan Pérez (Restaurante El Buen Sabor) - Gastronomia",
    "tipo": "SolicitudOferente",
    "urlAccion": "/admin/solicitudes/1",
    "fecha": "2025-11-21T18:00:00Z",
    "leida": false
  },
  {
    "id": 2,
    "usuarioId": "uuid-usuario",
    "titulo": "Reserva confirmada",
    "mensaje": "Tu reserva en Cabaña del Bosque ha sido confirmada",
    "tipo": "Reserva",
    "urlAccion": "/reservas/1",
    "fecha": "2025-11-20T15:00:00Z",
    "leida": true
  }
]
```

---

### Marcar Notificación como Leída

**Endpoint:** `PUT /api/notificaciones/{id}/leer`

**Auth:** Requerida (propietario de la notificación)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (204 No Content)**

---

### Contar Notificaciones No Leídas

**Endpoint:** `GET /api/notificaciones/noLeidas/count`

**Auth:** Requerida

**Headers:**
```
Authorization: Bearer {token}
```

**Response (200 OK):**
```json
{
  "count": 3
}
```

**Notas:**
- Retorna el número total de notificaciones no leídas del usuario actual
- Útil para mostrar badges/contadores en la UI

---

### Eliminar Notificación

**Endpoint:** `DELETE /api/notificaciones/{id}`

**Auth:** Requerida (propietario de la notificación)

**Headers:**
```
Authorization: Bearer {token}
```

**Response (204 No Content)**

---

## 📁 Storage (Archivos)

### Subir Archivo

**Endpoint:** `POST /api/storage/upload`

**Auth:** Requerida

**Headers:**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Request (Form Data):**
- `file`: Archivo a subir (imagen)

**Ejemplo usando curl:**
```bash
curl -X POST https://localhost:7190/api/storage/upload \
  -H "Authorization: Bearer {token}" \
  -F "file=@/ruta/a/imagen.jpg"
```

**Response (200 OK):**
```json
{
  "url": "/uploads/2025-11-21/abc123.jpg"
}
```

**Notas:**
- Los archivos se guardan en la carpeta `wwwroot/uploads`
- El nombre del archivo se genera automáticamente
- Retorna la URL relativa que puedes usar en `fotoPrincipal` o similar

---

## 📊 Tipos de Datos

### TipoOferente (Enum)
```
1 = Alojamiento
2 = Gastronomia
3 = Ambos
```

### Estados de Oferente
```
Pendiente  - Oferente recién registrado, requiere aprobación/verificación
Activo     - Oferente activo, puede operar normalmente
Inactivo   - Oferente temporalmente inactivo
Suspendido - Oferente suspendido por administrador
```

### Estados de Reserva
```
Pendiente
Confirmada
Cancelada
Completada
```

### Estados de Solicitud
```
Pendiente
Aprobada
Rechazada
```

### Tipos de Notificación
```
SolicitudOferente
Reserva
Oferente
General
```

---

## 🔑 Roles del Sistema

- **Cliente**: Puede crear reservas, ver alojamientos/restaurantes
- **Oferente**: Puede gestionar alojamientos/establecimientos (según su tipo)
- **Admin**: Acceso completo, gestiona solicitudes, aprueba oferentes

---

## ⚠️ Códigos de Error Comunes

| Código | Significado |
|--------|-------------|
| 200 | OK - Solicitud exitosa |
| 201 | Created - Recurso creado exitosamente |
| 204 | No Content - Solicitud exitosa sin contenido de respuesta |
| 400 | Bad Request - Datos inválidos o faltantes |
| 401 | Unauthorized - Token inválido o no proporcionado |
| 403 | Forbidden - No tienes permisos para esta acción |
| 404 | Not Found - Recurso no encontrado |
| 500 | Internal Server Error - Error del servidor |

---

## 📝 Notas Importantes

1. **Autenticación:** Todos los endpoints marcados como "Auth: Requerida" necesitan el header `Authorization: Bearer {token}`

2. **Formato de Fechas:** Usar formato ISO 8601: `2025-11-25T20:00:00Z`

3. **Subida de Archivos:**
   - Primero subir el archivo con `/api/storage/upload`
   - Luego usar la URL retornada en el campo `fotoPrincipal` o similar

4. **Oferentes y Tipos:**
   - Un oferente con `tipo=1` (Alojamiento) solo debería gestionar alojamientos
   - Un oferente con `tipo=2` (Gastronomía) solo debería gestionar restaurantes
   - Un oferente con `tipo=3` (Ambos) puede gestionar ambos tipos

5. **Notificaciones Automáticas:**
   - Nueva solicitud → Notifica a todos los admins
   - Solicitud aprobada → Notifica al usuario
   - Registro de oferente (manual por admin) → Notifica al oferente

---

## 🚀 Ejemplo de Flujo Completo

### 1. Usuario solicita ser oferente de gastronomía
```http
POST /api/solicitudesoferente
{
  "nombreSolicitante": "María",
  "telefono": "+54911...",
  "nombreNegocio": "Café Central",
  "correo": "maria@cafe.com",
  "tipoSolicitado": 2
}
```

### 2. Admin recibe notificación
```http
GET /api/notificaciones
[
  {
    "titulo": "Nueva solicitud de oferente",
    "mensaje": "Solicitud de María (Café Central) - Gastronomia"
  }
]
```

### 3. Admin aprueba
```http
POST /api/admin/oferentes/solicitudes/1/aprobar
```

### 4. María inicia sesión
```http
POST /api/auth/login
{
  "email": "maria@cafe.com",
  "password": "TempXXXXXXXX!"
}
```

### 5. María sube foto
```http
POST /api/storage/upload
Form: file=foto.jpg
→ Response: { "url": "/uploads/2025-11-21/abc.jpg" }
```

### 6. María crea su establecimiento
```http
POST /api/gastronomias
{
  "nombre": "Café Central",
  "ubicacion": "Av. Principal 123",
  "fotoPrincipal": "/uploads/2025-11-21/abc.jpg"
}
```

### 7. María crea un menú
```http
POST /api/gastronomias/1/menus
{
  "nombre": "Menú Principal"
}
```

### 8. María agrega items al menú
```http
POST /api/gastronomias/1/menus/1/items
{
  "nombre": "Café Americano",
  "precio": 1500
}
```

### 9. María crea mesas
```http
POST /api/gastronomias/1/mesas
{
  "numero": 1,
  "capacidad": 4
}
```

### 10. Cliente ve el restaurante
```http
GET /api/gastronomias
```

### 11. Cliente hace reserva
```http
POST /api/gastronomias/reservas
{
  "establecimientoId": 1,
  "mesaId": 1,
  "fecha": "2025-11-25T20:00:00",
  "numeroPersonas": 2
}
```

---

**Fin de la documentación**
