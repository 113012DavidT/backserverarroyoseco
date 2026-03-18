# Módulo de Gastronomía - ArroyoSeco API

## Resumen
Este módulo permite gestionar establecimientos gastronómicos (restaurantes), menús, mesas y reservas. Implementa tres roles:
- **Cliente**: busca restaurantes, ve menús y crea reservas.
- **Oferente (propietario)**: gestiona sus establecimientos, menús, mesas y reservas.
- **SuperAdmin**: (futuro) aprueba/gestiona establecimientos.

## Tipos de Oferentes

Los oferentes ahora se categorizan por tipo de servicio:

- **Alojamiento (1)**: Gestionan solo alojamientos
- **Gastronomía (2)**: Gestionan solo restaurantes  
- **Ambos (3)**: Gestionan alojamientos y restaurantes (valor por defecto)

### Registro con Tipo
```json
POST /api/auth/register
{
  "email": "restaurant@example.com",
  "password": "Password123!",
  "role": "Oferente",
  "tipoOferente": 2
}
```

### Filtrar Oferentes (Admin)
```http
GET /api/Oferentes?tipo=2              # Solo gastronomía
GET /api/Oferentes/alojamiento         # Alojamiento o Ambos
GET /api/Oferentes/gastronomia         # Gastronomía o Ambos
PUT /api/Oferentes/{id}/tipo           # Cambiar tipo (requiere Admin)
```

## Entidades de Dominio

### `Establecimiento`
Representa un restaurante o local gastronómico.
- `Id`, `OferenteId`, `Nombre`, `Ubicacion`, `Descripcion`, `FotoPrincipal`
- Relaciones: `Menus`, `Mesas`, `Reservas`

### `Menu`
Colección de platillos/bebidas de un establecimiento.
- `Id`, `EstablecimientoId`, `Nombre`
- Relación: `Items` (MenuItems)

### `MenuItem`
Item individual dentro de un menú.
- `Id`, `MenuId`, `Nombre`, `Descripcion`, `Precio`

### `Mesa`
Mesa disponible en el establecimiento.
- `Id`, `EstablecimientoId`, `Numero`, `Capacidad`, `Disponible`

### `ReservaGastronomia`
Reserva creada por un cliente.
- `Id`, `UsuarioId`, `EstablecimientoId`, `MesaId`, `Fecha`, `NumeroPersonas`, `Estado`, `Total`

## API Endpoints

### Públicos (sin autenticación)

#### Listar establecimientos
```http
GET /api/Gastronomias
```
Retorna todos los establecimientos con menús y mesas.

#### Detalle de establecimiento
```http
GET /api/Gastronomias/{id}
```
Retorna un establecimiento específico con menús y mesas.

#### Listar menús
```http
GET /api/Gastronomias/{id}/menus
```
Retorna los menús (con items) del establecimiento.

#### Verificar disponibilidad
```http
GET /api/Gastronomias/{id}/disponibilidad?fecha=2025-12-25T19:00:00
```
Retorna número de mesas disponibles.

---

### Oferente (rol `Oferente`)

#### Crear establecimiento
```http
POST /api/Gastronomias
Content-Type: application/json
Authorization: Bearer {token}

{
  "nombre": "La Trattoria",
  "ubicacion": "Centro Histórico",
  "descripcion": "Cocina italiana auténtica",
  "fotoPrincipal": "/comprobantes/fotos/abc123.jpg"
}
```

#### Crear menú
```http
POST /api/Gastronomias/{id}/menus
Content-Type: application/json
Authorization: Bearer {token}

{
  "nombre": "Menú del día"
}
```

#### Agregar item a menú
```http
POST /api/Gastronomias/{id}/menus/{menuId}/items
Content-Type: application/json
Authorization: Bearer {token}

{
  "nombre": "Pizza Margarita",
  "descripcion": "Tomate, mozzarella y albahaca",
  "precio": 120.50
}
```

#### Crear mesa
```http
POST /api/Gastronomias/{id}/mesas
Content-Type: application/json
Authorization: Bearer {token}

{
  "numero": 5,
  "capacidad": 4
}
```

#### Cambiar disponibilidad de mesa
```http
PUT /api/Gastronomias/{id}/mesas/{mesaId}/disponible
Content-Type: application/json
Authorization: Bearer {token}

true
```

#### Listar reservas
```http
GET /api/Gastronomias/{id}/reservas
Authorization: Bearer {token}
```
Retorna las reservas del establecimiento (solo propietario).

---

### Cliente (autenticado)

#### Crear reserva
```http
POST /api/Gastronomias/{id}/reservas
Content-Type: application/json
Authorization: Bearer {token}

{
  "fecha": "2025-12-25T19:00:00",
  "numeroPersonas": 4,
  "mesaId": 3
}
```
Crea una reserva y notifica al propietario.

---

### Storage (subir imágenes)

#### Subir archivo
```http
POST /api/Storage/upload?folder=fotos
Content-Type: multipart/form-data
Authorization: Bearer {token}

file: [archivo]
```
Retorna:
```json
{
  "url": "/comprobantes/fotos/abc-123-456_imagen.jpg"
}
```

## Servicios

### `IStorageService` / `DiskStorageService`
Almacena archivos en disco (configurado en `StorageOptions.ComprobantesPath`).

### `CrearEstablecimientoCommandHandler`
Crea un establecimiento vinculado al oferente autenticado.

### `CrearMenuCommandHandler`
Crea un menú para un establecimiento (valida propiedad).

### `AgregarMenuItemCommandHandler`
Agrega un item a un menú existente.

### `CrearMesaCommandHandler`
Crea una mesa para un establecimiento.

### `CrearReservaGastronomiaCommandHandler`
Crea una reserva, verifica disponibilidad de mesa (si aplica) y notifica al propietario.

## Migraciones

Para aplicar los cambios a la base de datos:
```powershell
cd arroyoSeco
dotnet ef migrations add AddGastronomiaModule --context AppDbContext
dotnet ef database update --context AppDbContext
```

## Flujo de uso típico

1. **Oferente crea establecimiento** → `POST /api/Gastronomias`
2. **Oferente sube foto** → `POST /api/Storage/upload?folder=fotos` → obtiene URL
3. **Oferente crea menú** → `POST /api/Gastronomias/{id}/menus`
4. **Oferente agrega items** → `POST /api/Gastronomias/{id}/menus/{menuId}/items`
5. **Oferente crea mesas** → `POST /api/Gastronomias/{id}/mesas`
6. **Cliente busca restaurantes** → `GET /api/Gastronomias`
7. **Cliente ve menús** → `GET /api/Gastronomias/{id}/menus`
8. **Cliente verifica disponibilidad** → `GET /api/Gastronomias/{id}/disponibilidad`
9. **Cliente crea reserva** → `POST /api/Gastronomias/{id}/reservas`
10. **Oferente revisa reservas** → `GET /api/Gastronomias/{id}/reservas`

## Próximas mejoras

- [ ] Endpoint para oferente cambie estado de reserva (Confirmada/Cancelada).
- [ ] Validación de conflictos de horarios para mesas.
- [ ] Filtros en listado de establecimientos (ubicación, tipo de cocina).
- [ ] Paginación en listados.
- [ ] SuperAdmin: aprobar establecimientos antes de publicarlos.
- [ ] Sistema de calificaciones y reseñas.
- [ ] Envío de notificaciones push/email a clientes.

---

## Notas técnicas

- Las entidades están en `arroyoSeco.Domain/Entities/Gastronomia/`.
- Los comandos están en `arroyoSeco.Application/Features/Gastronomia/Commands/`.
- El controlador está en `arroyoSeco/Controllers/GastronomiasController.cs`.
- Las relaciones están configuradas en `AppDbContext.OnModelCreating`.
- El servicio de notificaciones ya está integrado (`INotificationService`).
