-- Script de inserción de datos para Gastronomía
-- Asegúrate de que existen Oferentes con IDs 1-5

-- =============================================
-- ESTABLECIMIENTOS
-- =============================================
INSERT INTO Establecimientos (Nombre, Ubicacion, FotoPrincipal, Descripcion, OferenteId)
VALUES 
('La Toscana', 'Calle Italia 123, Centro', 'https://images.unsplash.com/photo-1514933651103-005eec06c04b', 'Auténtica pizzería italiana con horno de leña y recetas tradicionales de la Toscana', 'a92edb2d-c4c0-40eb-aa53-962e72e00cac'),
('Sakura Sushi Bar', 'Av. Japón 456, Zona Rosa', 'https://images.unsplash.com/photo-1579584425555-c3ce17fd4351', 'Restaurante japonés especializado en sushi fresco y cocina nikkei', 'a92edb2d-c4c0-40eb-aa53-962e72e00cac'),
('The Burger House', 'Calle Hamburguesa 789, Plaza Mayor', 'https://images.unsplash.com/photo-1568901346375-23c9450c58cd', 'Hamburguesas gourmet con carne premium y ingredientes artesanales', 'a92edb2d-c4c0-40eb-aa53-962e72e00cac'),
('Casa Azteca', 'Av. México 321, Barrio Latino', 'https://images.unsplash.com/photo-1565299585323-38d6b0865b47', 'Cocina mexicana auténtica con tacos, enchiladas y mezcal artesanal', 'a92edb2d-c4c0-40eb-aa53-962e72e00cac'),
('Coffee & More', 'Plaza del Café 654, Centro Histórico', 'https://images.unsplash.com/photo-1511920170033-f8396924c348', 'Cafetería moderna con café de especialidad, pasteles caseros y brunch', 'a92edb2d-c4c0-40eb-aa53-962e72e00cac'),
('Green Life', 'Calle Vegana 987, Eco Barrio', 'https://images.unsplash.com/photo-1512621776951-a57141f2eefd', 'Restaurante vegetariano y vegano con ingredientes orgánicos y locales', 'a92edb2d-c4c0-40eb-aa53-962e72e00cac');

-- =============================================
-- MENUS (Categorías)
-- =============================================
-- La Toscana (EstablecimientoId = 1)
INSERT INTO Menus (Nombre, EstablecimientoId)
VALUES 
('Pizzas Clásicas', 1),
('Pizzas Especiales', 1),
('Pastas', 1),
('Postres Italianos', 1);

-- Sakura Sushi Bar (EstablecimientoId = 2)
INSERT INTO Menus (Nombre, EstablecimientoId)
VALUES 
('Sushi & Sashimi', 2),
('Rolls Especiales', 2),
('Platos Calientes', 2);

-- The Burger House (EstablecimientoId = 3)
INSERT INTO Menus (Nombre, EstablecimientoId)
VALUES 
('Burgers Gourmet', 3),
('Chicken & Veggie', 3),
('Acompañamientos', 3),
('Bebidas', 3);

-- Casa Azteca (EstablecimientoId = 4)
INSERT INTO Menus (Nombre, EstablecimientoId)
VALUES 
('Tacos & Quesadillas', 4),
('Platos Principales', 4),
('Antojitos', 4);

-- Coffee & More (EstablecimientoId = 5)
INSERT INTO Menus (Nombre, EstablecimientoId)
VALUES 
('Cafés', 5),
('Desayunos & Brunch', 5),
('Repostería', 5);

-- Green Life (EstablecimientoId = 6)
INSERT INTO Menus (Nombre, EstablecimientoId)
VALUES 
('Bowls Veganos', 6),
('Ensaladas', 6),
('Smoothies & Jugos', 6);

-- =============================================
-- MENU ITEMS
-- =============================================
-- La Toscana - Pizzas Clásicas (MenuId = 1)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Pizza Margherita', 'Tomate, mozzarella fresca y albahaca', 12.50, 1),
('Pizza Quattro Formaggi', 'Mozzarella, gorgonzola, parmesano y ricotta', 14.00, 1),
('Pizza Prosciutto', 'Jamón serrano, rúcula y parmesano', 15.50, 1);

-- La Toscana - Pizzas Especiales (MenuId = 2)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Pizza Tartufo', 'Crema de trufa, champiñones y mozzarella', 18.00, 2),
('Pizza Diavola', 'Salami picante, aceitunas negras y chile', 16.00, 2);

-- La Toscana - Pastas (MenuId = 3)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Spaghetti Carbonara', 'Panceta, huevo, pecorino y pimienta negra', 13.50, 3),
('Lasagna Bolognese', 'Capas de pasta con ragú de carne y bechamel', 14.50, 3),
('Ravioli de Ricotta', 'Con salsa de mantequilla y salvia', 15.00, 3);

-- La Toscana - Postres (MenuId = 4)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Tiramisú', 'Clásico postre italiano con café y mascarpone', 6.50, 4),
('Panna Cotta', 'Con coulis de frutos rojos', 6.00, 4);

-- Sakura - Sushi & Sashimi (MenuId = 5)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Nigiri Mix (10 piezas)', 'Salmón, atún, pez mantequilla y anguila', 18.00, 5),
('Sashimi Premium', '15 cortes de pescado fresco del día', 22.00, 5),
('Combo Sushi Variado', '20 piezas de nigiri y maki', 25.00, 5);

-- Sakura - Rolls Especiales (MenuId = 6)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Dragon Roll', 'Aguacate, anguila, pepino y salsa teriyaki', 16.00, 6),
('California Roll', 'Cangrejo, aguacate, pepino y tobiko', 14.00, 6),
('Spicy Tuna Roll', 'Atún picante, cebollín y tempura crunch', 15.00, 6);

-- Sakura - Platos Calientes (MenuId = 7)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Ramen Tonkotsu', 'Caldo de cerdo con fideos, chashu y huevo', 14.00, 7),
('Pollo Teriyaki', 'Con arroz y vegetales salteados', 13.50, 7),
('Tempura Mix', 'Langostinos y vegetales en tempura crujiente', 15.50, 7);

-- The Burger House - Burgers Gourmet (MenuId = 8)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Classic Burger', 'Res angus, queso cheddar, lechuga, tomate y cebolla', 12.00, 8),
('Bacon BBQ Burger', 'Doble carne, bacon, cebolla caramelizada y salsa BBQ', 15.00, 8),
('Blue Cheese Burger', 'Res angus, queso azul, rúcula y cebolla crispy', 14.50, 8);

-- The Burger House - Chicken & Veggie (MenuId = 9)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Crispy Chicken Burger', 'Pollo empanizado, mayonesa de chipotle y ensalada', 11.50, 9),
('Veggie Burger', 'Hamburguesa de garbanzos, aguacate y hummus', 10.50, 9);

-- The Burger House - Acompañamientos (MenuId = 10)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Papas Fritas Clásicas', 'Crujientes y doradas', 4.50, 10),
('Aros de Cebolla', 'Con salsa ranch', 5.00, 10),
('Papas con Queso y Bacon', 'Cheddar fundido y bacon crujiente', 6.50, 10);

-- The Burger House - Bebidas (MenuId = 11)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Cerveza Artesanal IPA', '500ml', 5.00, 11),
('Batido de Oreo', 'Cremoso y delicioso', 5.50, 11),
('Limonada Casera', 'Refrescante', 3.50, 11);

-- Casa Azteca - Tacos & Quesadillas (MenuId = 12)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Tacos al Pastor (3 uds)', 'Cerdo marinado, piña, cilantro y cebolla', 10.00, 12),
('Tacos de Barbacoa (3 uds)', 'Res deshebrada con consomé', 11.00, 12),
('Quesadilla de Champiñones', 'Con queso Oaxaca y epazote', 9.50, 12);

-- Casa Azteca - Platos Principales (MenuId = 13)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Enchiladas Verdes', 'Pollo con salsa verde y crema', 12.50, 13),
('Fajitas Mixtas', 'Res, pollo y camarones con tortillas', 16.00, 13),
('Mole Poblano', 'Pollo con mole tradicional y arroz', 14.00, 13);

-- Casa Azteca - Antojitos (MenuId = 14)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Guacamole con Totopos', 'Preparado al momento', 7.50, 14),
('Nachos Supreme', 'Con queso, jalapeños, crema y guacamole', 9.00, 14),
('Ceviche de Camarón', 'Fresco y picante', 11.50, 14);

-- Coffee & More - Cafés (MenuId = 15)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Espresso', 'Intenso y aromático', 2.50, 15),
('Capuchino', 'Con espuma de leche cremosa', 3.50, 15),
('Café Latte', 'Suave y equilibrado', 4.00, 15),
('Cold Brew', 'Café frío de extracción lenta', 4.50, 15);

-- Coffee & More - Desayunos & Brunch (MenuId = 16)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Tostadas con Aguacate', 'Pan integral, aguacate, huevo pochado', 8.50, 16),
('Pancakes con Frutos Rojos', 'Sirope de arce y mantequilla', 7.50, 16),
('Croissant de Jamón y Queso', 'Recién horneado', 5.50, 16);

-- Coffee & More - Repostería (MenuId = 17)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Tarta de Zanahoria', 'Con frosting de queso crema', 4.50, 17),
('Brownie de Chocolate', 'Con nueces', 4.00, 17),
('Galletas Caseras (3 uds)', 'Chocolate chip o avena', 3.50, 17);

-- Green Life - Bowls Veganos (MenuId = 18)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Buddha Bowl', 'Quinoa, garbanzos, aguacate, vegetales asados', 11.50, 18),
('Poke Bowl Vegano', 'Tofu marinado, edamame, algas, arroz', 12.00, 18),
('Mexican Bowl', 'Frijoles negros, maíz, pico de gallo, guacamole', 10.50, 18);

-- Green Life - Ensaladas (MenuId = 19)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Ensalada Caesar Vegana', 'Lechuga romana, crutones, parmesano vegano', 9.50, 19),
('Ensalada de Quinoa', 'Con vegetales frescos y vinagreta de limón', 10.00, 19),
('Ensalada Mediterránea', 'Tomate, pepino, aceitunas, queso feta vegano', 9.00, 19);

-- Green Life - Smoothies & Jugos (MenuId = 20)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId)
VALUES 
('Green Detox Smoothie', 'Espinaca, manzana, plátano, jengibre', 5.50, 20),
('Berry Blast', 'Frutos rojos, leche de almendras, chía', 6.00, 20),
('Jugo Verde', 'Apio, pepino, manzana verde, limón', 5.00, 20);

-- =============================================
-- MESAS (Tables)
-- =============================================
-- La Toscana (EstablecimientoId = 1) - 15 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId)
VALUES 
(1, 2, 1, 1),
(2, 2, 1, 1),
(3, 2, 1, 1),
(4, 4, 1, 1),
(5, 4, 1, 1),
(6, 4, 1, 1),
(7, 4, 1, 1),
(8, 6, 1, 1),
(9, 6, 1, 1),
(10, 6, 1, 1),
(11, 8, 1, 1),
(12, 8, 1, 1),
(13, 2, 1, 1),
(14, 4, 1, 1),
(15, 10, 1, 1);

-- Sakura Sushi Bar (EstablecimientoId = 2) - 12 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId)
VALUES 
(1, 2, 1, 2),
(2, 2, 1, 2),
(3, 2, 1, 2),
(4, 2, 1, 2),
(5, 4, 1, 2),
(6, 4, 1, 2),
(7, 4, 1, 2),
(8, 6, 1, 2),
(9, 6, 1, 2),
(10, 8, 1, 2),
(11, 4, 1, 2),
(12, 2, 1, 2);

-- The Burger House (EstablecimientoId = 3) - 20 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId)
VALUES 
(1, 2, 1, 3),
(2, 2, 1, 3),
(3, 2, 1, 3),
(4, 2, 1, 3),
(5, 2, 1, 3),
(6, 4, 1, 3),
(7, 4, 1, 3),
(8, 4, 1, 3),
(9, 4, 1, 3),
(10, 4, 1, 3),
(11, 4, 1, 3),
(12, 6, 1, 3),
(13, 6, 1, 3),
(14, 6, 1, 3),
(15, 8, 1, 3),
(16, 8, 1, 3),
(17, 2, 1, 3),
(18, 4, 1, 3),
(19, 4, 1, 3),
(20, 10, 1, 3);

-- Casa Azteca (EstablecimientoId = 4) - 14 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId)
VALUES 
(1, 2, 1, 4),
(2, 2, 1, 4),
(3, 2, 1, 4),
(4, 4, 1, 4),
(5, 4, 1, 4),
(6, 4, 1, 4),
(7, 4, 1, 4),
(8, 6, 1, 4),
(9, 6, 1, 4),
(10, 6, 1, 4),
(11, 8, 1, 4),
(12, 8, 1, 4),
(13, 4, 1, 4),
(14, 10, 1, 4);

-- Coffee & More (EstablecimientoId = 5) - 10 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId)
VALUES 
(1, 2, 1, 5),
(2, 2, 1, 5),
(3, 2, 1, 5),
(4, 2, 1, 5),
(5, 4, 1, 5),
(6, 4, 1, 5),
(7, 4, 1, 5),
(8, 6, 1, 5),
(9, 4, 1, 5),
(10, 2, 1, 5);

-- Green Life (EstablecimientoId = 6) - 12 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId)
VALUES 
(1, 2, 1, 6),
(2, 2, 1, 6),
(3, 2, 1, 6),
(4, 4, 1, 6),
(5, 4, 1, 6),
(6, 4, 1, 6),
(7, 4, 1, 6),
(8, 6, 1, 6),
(9, 6, 1, 6),
(10, 8, 1, 6),
(11, 2, 1, 6),
(12, 4, 1, 6);
