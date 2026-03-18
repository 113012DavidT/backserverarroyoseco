-- MENU ITEMS
-- La Toscana - Pizzas Clásicas (MenuId = 9)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Pizza Margherita', 'Tomate, mozzarella fresca y albahaca', 12.50, 9),
('Pizza Quattro Formaggi', 'Mozzarella, gorgonzola, parmesano y ricotta', 14.00, 9),
('Pizza Prosciutto', 'Jamón serrano, rúcula y parmesano', 15.50, 9);

-- La Toscana - Pizzas Especiales (MenuId = 10)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Pizza Tartufo', 'Crema de trufa, champiñones y mozzarella', 18.00, 10),
('Pizza Diavola', 'Salami picante, aceitunas negras y chile', 16.00, 10);

-- La Toscana - Pastas (MenuId = 11)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Spaghetti Carbonara', 'Panceta, huevo, pecorino y pimienta negra', 13.50, 11),
('Lasagna Bolognese', 'Capas de pasta con ragú de carne y bechamel', 14.50, 11),
('Ravioli de Ricotta', 'Con salsa de mantequilla y salvia', 15.00, 11);

-- La Toscana - Postres (MenuId = 12)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Tiramisú', 'Clásico postre italiano con café y mascarpone', 6.50, 12),
('Panna Cotta', 'Con coulis de frutos rojos', 6.00, 12);

-- Sakura - Sushi & Sashimi (MenuId = 13)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Nigiri Mix (10 piezas)', 'Salmón, atún, pez mantequilla y anguila', 18.00, 13),
('Sashimi Premium', '15 cortes de pescado fresco del día', 22.00, 13),
('Combo Sushi Variado', '20 piezas de nigiri y maki', 25.00, 13);

-- Sakura - Rolls Especiales (MenuId = 14)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Dragon Roll', 'Aguacate, anguila, pepino y salsa teriyaki', 16.00, 14),
('California Roll', 'Cangrejo, aguacate, pepino y tobiko', 14.00, 14),
('Spicy Tuna Roll', 'Atún picante, cebollín y tempura crunch', 15.00, 14);

-- Sakura - Platos Calientes (MenuId = 15)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Ramen Tonkotsu', 'Caldo de cerdo con fideos, chashu y huevo', 14.00, 15),
('Pollo Teriyaki', 'Con arroz y vegetales salteados', 13.50, 15),
('Tempura Mix', 'Langostinos y vegetales en tempura crujiente', 15.50, 15);

-- The Burger House - Burgers Gourmet (MenuId = 16)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Classic Burger', 'Res angus, queso cheddar, lechuga, tomate y cebolla', 12.00, 16),
('Bacon BBQ Burger', 'Doble carne, bacon, cebolla caramelizada y salsa BBQ', 15.00, 16),
('Blue Cheese Burger', 'Res angus, queso azul, rúcula y cebolla crispy', 14.50, 16);

-- The Burger House - Chicken & Veggie (MenuId = 17)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Crispy Chicken Burger', 'Pollo empanizado, mayonesa de chipotle y ensalada', 11.50, 17),
('Veggie Burger', 'Hamburguesa de garbanzos, aguacate y hummus', 10.50, 17);

-- The Burger House - Acompañamientos (MenuId = 18)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Papas Fritas Clásicas', 'Crujientes y doradas', 4.50, 18),
('Aros de Cebolla', 'Con salsa ranch', 5.00, 18),
('Papas con Queso y Bacon', 'Cheddar fundido y bacon crujiente', 6.50, 18);

-- The Burger House - Bebidas (MenuId = 19)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Cerveza Artesanal IPA', '500ml', 5.00, 19),
('Batido de Oreo', 'Cremoso y delicioso', 5.50, 19),
('Limonada Casera', 'Refrescante', 3.50, 19);

-- Casa Azteca - Tacos & Quesadillas (MenuId = 20)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Tacos al Pastor (3 uds)', 'Cerdo marinado, piña, cilantro y cebolla', 10.00, 20),
('Tacos de Barbacoa (3 uds)', 'Res deshebrada con consomé', 11.00, 20),
('Quesadilla de Champiñones', 'Con queso Oaxaca y epazote', 9.50, 20);

-- Casa Azteca - Platos Principales (MenuId = 21)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Enchiladas Verdes', 'Pollo con salsa verde y crema', 12.50, 21),
('Fajitas Mixtas', 'Res, pollo y camarones con tortillas', 16.00, 21),
('Mole Poblano', 'Pollo con mole tradicional y arroz', 14.00, 21);

-- Casa Azteca - Antojitos (MenuId = 22)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Guacamole con Totopos', 'Preparado al momento', 7.50, 22),
('Nachos Supreme', 'Con queso, jalapeños, crema y guacamole', 9.00, 22),
('Ceviche de Camarón', 'Fresco y picante', 11.50, 22);

-- Coffee & More - Cafés (MenuId = 23)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Espresso', 'Intenso y aromático', 2.50, 23),
('Capuchino', 'Con espuma de leche cremosa', 3.50, 23),
('Café Latte', 'Suave y equilibrado', 4.00, 23),
('Cold Brew', 'Café frío de extracción lenta', 4.50, 23);

-- Coffee & More - Desayunos & Brunch (MenuId = 24)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Tostadas con Aguacate', 'Pan integral, aguacate, huevo pochado', 8.50, 24),
('Pancakes con Frutos Rojos', 'Sirope de arce y mantequilla', 7.50, 24),
('Croissant de Jamón y Queso', 'Recién horneado', 5.50, 24);

-- Coffee & More - Repostería (MenuId = 25)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Tarta de Zanahoria', 'Con frosting de queso crema', 4.50, 25),
('Brownie de Chocolate', 'Con nueces', 4.00, 25),
('Galletas Caseras (3 uds)', 'Chocolate chip o avena', 3.50, 25);

-- Green Life - Bowls Veganos (MenuId = 26)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Buddha Bowl', 'Quinoa, garbanzos, aguacate, vegetales asados', 11.50, 26),
('Poke Bowl Vegano', 'Tofu marinado, edamame, algas, arroz', 12.00, 26),
('Mexican Bowl', 'Frijoles negros, maíz, pico de gallo, guacamole', 10.50, 26);

-- Green Life - Ensaladas (MenuId = 27)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Ensalada Caesar Vegana', 'Lechuga romana, crutones, parmesano vegano', 9.50, 27),
('Ensalada de Quinoa', 'Con vegetales frescos y vinagreta de limón', 10.00, 27),
('Ensalada Mediterránea', 'Tomate, pepino, aceitunas, queso feta vegano', 9.00, 27);

-- Green Life - Smoothies & Jugos (MenuId = 28)
INSERT INTO MenuItems (Nombre, Descripcion, Precio, MenuId) VALUES 
('Green Detox Smoothie', 'Espinaca, manzana, plátano, jengibre', 5.50, 28),
('Berry Blast', 'Frutos rojos, leche de almendras, chía', 6.00, 28),
('Jugo Verde', 'Apio, pepino, manzana verde, limón', 5.00, 28);

-- MESAS
-- La Toscana (EstablecimientoId = 3) - 15 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId) VALUES 
(1, 2, 1, 3),(2, 2, 1, 3),(3, 2, 1, 3),(4, 4, 1, 3),(5, 4, 1, 3),
(6, 4, 1, 3),(7, 4, 1, 3),(8, 6, 1, 3),(9, 6, 1, 3),(10, 6, 1, 3),
(11, 8, 1, 3),(12, 8, 1, 3),(13, 2, 1, 3),(14, 4, 1, 3),(15, 10, 1, 3);

-- Sakura Sushi Bar (EstablecimientoId = 4) - 12 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId) VALUES 
(1, 2, 1, 4),(2, 2, 1, 4),(3, 2, 1, 4),(4, 2, 1, 4),(5, 4, 1, 4),
(6, 4, 1, 4),(7, 4, 1, 4),(8, 6, 1, 4),(9, 6, 1, 4),(10, 8, 1, 4),
(11, 4, 1, 4),(12, 2, 1, 4);

-- The Burger House (EstablecimientoId = 5) - 20 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId) VALUES 
(1, 2, 1, 5),(2, 2, 1, 5),(3, 2, 1, 5),(4, 2, 1, 5),(5, 2, 1, 5),
(6, 4, 1, 5),(7, 4, 1, 5),(8, 4, 1, 5),(9, 4, 1, 5),(10, 4, 1, 5),
(11, 4, 1, 5),(12, 6, 1, 5),(13, 6, 1, 5),(14, 6, 1, 5),(15, 8, 1, 5),
(16, 8, 1, 5),(17, 2, 1, 5),(18, 4, 1, 5),(19, 4, 1, 5),(20, 10, 1, 5);

-- Casa Azteca (EstablecimientoId = 6) - 14 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId) VALUES 
(1, 2, 1, 6),(2, 2, 1, 6),(3, 2, 1, 6),(4, 4, 1, 6),(5, 4, 1, 6),
(6, 4, 1, 6),(7, 4, 1, 6),(8, 6, 1, 6),(9, 6, 1, 6),(10, 6, 1, 6),
(11, 8, 1, 6),(12, 8, 1, 6),(13, 4, 1, 6),(14, 10, 1, 6);

-- Coffee & More (EstablecimientoId = 7) - 10 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId) VALUES 
(1, 2, 1, 7),(2, 2, 1, 7),(3, 2, 1, 7),(4, 2, 1, 7),(5, 4, 1, 7),
(6, 4, 1, 7),(7, 4, 1, 7),(8, 6, 1, 7),(9, 4, 1, 7),(10, 2, 1, 7);

-- Green Life (EstablecimientoId = 8) - 12 mesas
INSERT INTO Mesas (Numero, Capacidad, Disponible, EstablecimientoId) VALUES 
(1, 2, 1, 8),(2, 2, 1, 8),(3, 2, 1, 8),(4, 4, 1, 8),(5, 4, 1, 8),
(6, 4, 1, 8),(7, 4, 1, 8),(8, 6, 1, 8),(9, 6, 1, 8),(10, 8, 1, 8),
(11, 2, 1, 8),(12, 4, 1, 8);
