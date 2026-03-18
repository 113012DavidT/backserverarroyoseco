# Sistema de Tipos de Oferentes

## Descripción

Para evitar que se mezclen diferentes tipos de proveedores en el sistema administrativo, se implementó un sistema de categorización de oferentes mediante el enum `TipoOferente`.

## Enum TipoOferente

```csharp
public enum TipoOferente
{
    Alojamiento = 1,
    Gastronomia = 2,
    Ambos = 3
}
```

### Valores

- **Alojamiento (1)**: Oferentes que solo gestionan alojamientos (hoteles, cabañas, etc.)
- **Gastronomía (2)**: Oferentes que solo gestionan establecimientos gastronómicos (restaurantes, cafeterías, etc.)
- **Ambos (3)**: Oferentes que pueden gestionar tanto alojamientos como restaurantes (valor por defecto)

## Registro de Oferentes

### Endpoint de Registro
```
POST /api/auth/register
```

### Request Body
```json
{
  "email": "oferente@example.com",
  "password": "Password123!",
  "role": "Oferente",
  "tipoOferente": 2
}
```

**Notas:**
- Si `tipoOferente` no se especifica, se asigna automáticamente el valor `Ambos (3)`
- Solo se aplica cuando `role = "Oferente"`
- Los roles Cliente y Admin no usan este campo

### Ejemplos

#### Oferente de Alojamiento
```json
{
  "email": "hotel@example.com",
  "password": "Password123!",
  "role": "Oferente",
  "tipoOferente": 1
}
```

#### Oferente de Gastronomía
```json
{
  "email": "restaurant@example.com",
  "password": "Password123!",
  "role": "Oferente",
  "tipoOferente": 2
}
```

#### Oferente Mixto (sin especificar tipo)
```json
{
  "email": "mixto@example.com",
  "password": "Password123!",
  "role": "Oferente"
}
```
→ Se asigna automáticamente `Tipo = 3 (Ambos)`

## Gestión de Oferentes (Admin)

### Listar Todos los Oferentes
```http
GET /api/Oferentes
Authorization: Bearer {admin-token}
```

**Response:**
```json
[
  {
    "id": "uuid-1",
    "nombre": "Hotel Paradise",
    "numeroAlojamientos": 5,
    "tipo": 1
  },
  {
    "id": "uuid-2",
    "nombre": "Restaurante El Buen Sabor",
    "numeroAlojamientos": 0,
    "tipo": 2
  }
]
```

### Filtrar por Tipo Específico
```http
GET /api/Oferentes?tipo=2
Authorization: Bearer {admin-token}
```

**Response:** Solo oferentes con `Tipo = 2 (Gastronomía)` o `Tipo = 3 (Ambos)`

### Listar Oferentes de Alojamiento
```http
GET /api/Oferentes/alojamiento
Authorization: Bearer {admin-token}
```

**Response:** Oferentes con `Tipo = 1 (Alojamiento)` o `Tipo = 3 (Ambos)`

### Listar Oferentes de Gastronomía
```http
GET /api/Oferentes/gastronomia
Authorization: Bearer {admin-token}
```

**Response:** Oferentes con `Tipo = 2 (Gastronomía)` o `Tipo = 3 (Ambos)`

### Cambiar Tipo de Oferente
```http
PUT /api/Oferentes/{id}/tipo
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "nuevoTipo": 2
}
```

**Respuestas:**
- `200 OK`: Tipo actualizado correctamente
- `404 Not Found`: Oferente no encontrado
- `403 Forbidden`: Usuario no es Admin

## Migración de Base de Datos

### Migración Aplicada
```
20251121175812_AddTipoOferente
```

Esta migración agrega la columna `Tipo` a la tabla `Oferentes`:

```sql
ALTER TABLE Oferentes 
ADD COLUMN Tipo int NOT NULL DEFAULT 0;
```

### Actualización de Datos Existentes

Si tienes oferentes creados antes de esta migración, ejecuta:

```sql
UPDATE Oferentes SET Tipo = 3 WHERE Tipo = 0;
```

Esto asigna el valor `Ambos (3)` a todos los oferentes existentes.

## Casos de Uso

### Caso 1: Administrador Filtra Restaurantes
1. Admin inicia sesión y obtiene token
2. Llama a `GET /api/Oferentes/gastronomia`
3. Recibe solo oferentes que gestionan restaurantes
4. Puede gestionar/aprobar establecimientos gastronómicos sin ver hoteles

### Caso 2: Oferente Registra Restaurante
1. Usuario se registra con `tipoOferente: 2`
2. Sistema crea oferente de tipo Gastronomía
3. Oferente solo verá opciones relacionadas con restaurantes en el frontend
4. (Futuro) Sistema validará que no puede crear alojamientos

### Caso 3: Oferente Mixto
1. Usuario se registra sin especificar tipo (o con `tipoOferente: 3`)
2. Sistema asigna tipo `Ambos`
3. Oferente puede crear tanto alojamientos como restaurantes

## Validaciones Futuras (Recomendadas)

### En Crear Alojamiento
```csharp
if (oferente.Tipo == TipoOferente.Gastronomia)
{
    throw new UnauthorizedAccessException("Los oferentes de gastronomía no pueden crear alojamientos");
}
```

### En Crear Establecimiento
```csharp
if (oferente.Tipo == TipoOferente.Alojamiento)
{
    throw new UnauthorizedAccessException("Los oferentes de alojamiento no pueden crear establecimientos gastronómicos");
}
```

## Modelo de Datos

### Tabla Oferentes
```sql
CREATE TABLE Oferentes (
    Id VARCHAR(255) PRIMARY KEY,
    Nombre VARCHAR(255),
    NumeroAlojamientos INT,
    Tipo INT NOT NULL DEFAULT 3,
    FOREIGN KEY (Id) REFERENCES AspNetUsers(Id)
);
```

### Relaciones
- `Oferente.Tipo` → `TipoOferente` enum
- `Oferente.Alojamientos` → Colección de alojamientos
- `Oferente.Establecimientos` → Colección de restaurantes

## Resumen

Esta implementación permite:
✅ Separar oferentes por tipo de servicio
✅ Filtrar oferentes en el panel administrativo
✅ Registrar oferentes con tipo específico
✅ Cambiar tipo de oferente (solo Admin)
✅ Consultar oferentes por categoría (Alojamiento/Gastronomía)

**Beneficio principal:** Los administradores pueden gestionar alojamientos y restaurantes de forma separada, evitando confusión en las listas y procesos de aprobación.
