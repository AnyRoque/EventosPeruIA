/* =====================================================================
   EventosPeruIA - Datos de prueba
   Ejecutar DESPUÉS de EventosPeruIA.sql, con la BD recién creada (vacía).
   Contraseña de todos los usuarios: Prueba123  (texto plano, igual que en clase)
   ===================================================================== */
USE EventosPeruIA;
GO

IF EXISTS (SELECT 1 FROM Usuario)
BEGIN
    RAISERROR('La BD ya tiene datos. Ejecuta primero EventosPeruIA.sql para recrearla vacía.', 16, 1);
    SET NOEXEC ON;
END
GO

DECLARE @hash VARCHAR(255) = 'Prueba123';

/* ============================ USUARIOS ============================ */
INSERT INTO Usuario (Nombre, Apellido, Correo, Celular, FechaNacimiento, Departamento, Distrito, PasswordHash, NombreOrganizador, Estado, MotivoSuspension, FechaRegistro) VALUES
('Admin',  'Sistema',  'admin@eventosperu.pe',     '900000001', '1990-01-15', 'Lima',  'San Isidro', @hash, NULL,                         'Activo',     NULL, '2026-08-01'),  -- 1 ADM
('Carlos', 'Mendoza',  'carlos.mendoza@gmail.com', '987654321', '1988-04-22', 'Lima',  'Barranco',   @hash, 'Lima Live Producciones',     'Activo',     NULL, '2026-08-05'),  -- 2 USU + ORG
('Lucía',  'Torres',   'lucia.torres@gmail.com',   '976543210', '1993-07-10', 'Lima',  'Miraflores', @hash, 'Talleres Creativos Perú',    'Activo',     NULL, '2026-08-07'),  -- 3 USU + ORG
('María',  'Quispe',   'maria.quispe@gmail.com',   '965432109', '2000-02-18', 'Lima',  'Miraflores', @hash, NULL,                         'Activo',     NULL, '2026-08-10'),  -- 4 USU
('José',   'Ramírez',  'jose.ramirez@gmail.com',   '954321098', '1998-11-03', 'Lima',  'Surco',      @hash, NULL,                         'Activo',     NULL, '2026-08-12'),  -- 5 USU
('Ana',    'Flores',   'ana.flores@gmail.com',     '943210987', '1995-05-27', 'Lima',  'Lince',      @hash, NULL,                         'Activo',     NULL, '2026-08-15'),  -- 6 USU
('Pedro',  'Salazar',  'pedro.salazar@gmail.com',  '932109876', '1997-09-14', 'Lima',  'Jesús María', @hash, NULL,                        'Suspendido', 'Publicó comentarios ofensivos de forma reiterada.', '2026-08-18'), -- 7 USU suspendido
('Rosa',   'Huamán',   'rosa.huaman@gmail.com',    '921098765', '1985-12-01', 'Cusco', 'Cusco',      @hash, 'Sabores y Artesanías del Sur','Activo',     NULL, '2026-08-20'); -- 8 USU + ORG

-- Roles: 1 Asistente, 2 Organizador, 3 Administrador
INSERT INTO UsuarioRol (IdUsuario, IdRol) VALUES
(1, 3),
(2, 1), (2, 2),
(3, 1), (3, 2),
(4, 1), (5, 1), (6, 1), (7, 1),
(8, 1), (8, 2);

-- Intereses: 1 Música, 2 Tecnología, 3 Gastronomía, 4 Arte, 5 Deporte, 6 Negocios, 7 Educación, 8 Cultura
INSERT INTO UsuarioInteres (IdUsuario, IdInteres) VALUES
(4, 1), (4, 3),
(5, 1), (5, 2), (5, 6),
(6, 4), (6, 8),
(7, 5),
(3, 2), (3, 7);

/* ============================ EVENTOS ============================ */
-- Categorías: 1 Concierto, 2 Taller, 3 Feria, 4 Charla
INSERT INTO Evento (IdOrganizador, IdCategoria, Nombre, Descripcion, FechaHoraInicio, FechaHoraFin, Departamento, Distrito, Direccion, ImagenUrl, AforoMaximo, Precio, Estado, MotivoCancelacion, FechaCancelacion, FechaCreacion, FechaModificacion) VALUES
-- 1 Publicado, con ventas y cambio de fecha
(2, 1, 'Noche de Rock en Barranco', 'Concierto con tres bandas de rock nacional en vivo. Incluye zona de comida y bebidas.',
 '2026-11-14 20:00', '2026-11-14 23:30', 'Lima', 'Barranco', 'Av. Pedro de Osma 135', NULL, 200, 60.00, 'Publicado', NULL, NULL, '2026-09-25', '2026-10-10'),
-- 2 Publicado, taller con poco aforo
(3, 2, 'Taller de Cerámica para Principiantes', 'Aprende las técnicas básicas del modelado en arcilla. Incluye materiales y horneado de tu pieza.',
 '2026-11-07 10:00', '2026-11-07 13:00', 'Lima', 'Miraflores', 'Calle Berlín 450', NULL, 20, 85.00, 'Publicado', NULL, NULL, '2026-09-28', NULL),
-- 3 Publicado, gratuito
(8, 3, 'Feria Gastronómica Sabores del Perú', 'Feria con más de 30 puestos de comida típica de todas las regiones del país. Ingreso libre con registro.',
 '2026-11-21 11:00', '2026-11-21 20:00', 'Lima', 'Surco', 'Parque de la Amistad, Av. Caminos del Inca s/n', NULL, 500, 0.00, 'Publicado', NULL, NULL, '2026-10-01', NULL),
-- 4 Publicado, charla
(3, 4, 'Charla: Inteligencia Artificial para Emprendedores', 'Conoce cómo usar herramientas de IA para automatizar tareas y hacer crecer tu negocio.',
 '2026-11-28 18:30', '2026-11-28 20:30', 'Lima', 'San Isidro', 'Av. Javier Prado Este 1050', NULL, 80, 25.00, 'Publicado', NULL, NULL, '2026-10-03', NULL),
-- 5 Finalizado, con check-in y reseñas
(2, 1, 'Jazz en el Parque', 'Noche de jazz al aire libre con músicos invitados. Lleva tu manta.',
 '2026-09-20 19:00', '2026-09-20 22:00', 'Lima', 'Miraflores', 'Parque Kennedy', NULL, 150, 45.00, 'Finalizado', NULL, NULL, '2026-08-20', NULL),
-- 6 Cancelado, con reembolsos
(2, 1, 'Festival de Salsa Lima 2026', 'Festival con orquestas de salsa nacionales e internacionales.',
 '2026-12-05 18:00', '2026-12-05 23:59', 'Lima', 'San Isidro', 'Av. Rivera Navarrete 800', NULL, 300, 70.00, 'Cancelado',
 'El local no obtuvo la licencia municipal a tiempo, por lo que no se puede realizar el evento.', '2026-10-15', '2026-09-15', NULL),
-- 7 Borrador
(3, 2, 'Taller de Fotografía Urbana', 'Recorrido fotográfico por el Centro de Lima con práctica de composición y luz natural.',
 '2026-12-12 09:00', '2026-12-12 13:00', 'Lima', 'Cercado de Lima', 'Plaza San Martín', NULL, 15, 50.00, 'Borrador', NULL, NULL, '2026-10-12', NULL),
-- 8 Suspendido por el administrador
(8, 3, 'Feria Artesanal Cusqueña', 'Exposición y venta de textiles, cerámica y joyería artesanal.',
 '2026-11-15 10:00', '2026-11-15 18:00', 'Cusco', 'Cusco', 'Plaza Regocijo', NULL, 100, 30.00, 'Suspendido', NULL, NULL, '2026-09-30', NULL),
-- 9 Publicado y agotado
(2, 4, 'Charla de Finanzas Personales', 'Aprende a organizar tu presupuesto, ahorrar y empezar a invertir.',
 '2026-10-30 19:00', '2026-10-30 21:00', 'Lima', 'Lince', 'Av. Arequipa 2450', NULL, 10, 20.00, 'Publicado', NULL, NULL, '2026-10-02', NULL);

/* ============================ COMPRAS ============================ */
INSERT INTO Compra (IdUsuario, CodigoOperacion, MontoTotal, TitularTarjeta, UltimosDigitos, Estado, FechaCompra, FechaReembolso) VALUES
(4, NEWID(),  90.00, 'MARIA QUISPE',   '4521', 'Pagada',      '2026-09-01 10:15', NULL),  -- 1  ev5 x2
(5, NEWID(),  45.00, 'JOSE RAMIREZ',   '1188', 'Pagada',      '2026-09-05 18:40', NULL),  -- 2  ev5 x1
(6, NEWID(), 135.00, 'ANA FLORES',     '7302', 'Pagada',      '2026-09-10 12:05', NULL),  -- 3  ev5 x3
(4, NEWID(), 120.00, 'MARIA QUISPE',   '4521', 'Pagada',      '2026-10-05 09:30', NULL),  -- 4  ev1 x2
(5, NEWID(), 240.00, 'JOSE RAMIREZ',   '1188', 'Pagada',      '2026-10-08 21:10', NULL),  -- 5  ev1 x4
(6, NEWID(),  85.00, 'ANA FLORES',     '7302', 'Pagada',      '2026-10-10 16:45', NULL),  -- 6  ev2 x1
(4, NEWID(),   0.00, NULL,             NULL,   'Pagada',      '2026-10-12 11:00', NULL),  -- 7  ev3 x2 (gratis)
(3, NEWID(), 120.00, 'LUCIA TORRES',   '9015', 'Pagada',      '2026-10-12 20:20', NULL),  -- 8  ev1 x2 (organizadora comprando otro evento)
(5, NEWID(), 140.00, 'JOSE RAMIREZ',   '1188', 'Reembolsada', '2026-10-01 14:00', '2026-10-15 09:00'),  -- 9  ev6 x2
(6, NEWID(),  70.00, 'ANA FLORES',     '7302', 'Reembolsada', '2026-10-02 19:30', '2026-10-15 09:00'),  -- 10 ev6 x1
(4, NEWID(), 100.00, 'MARIA QUISPE',   '4521', 'Pagada',      '2026-10-06 08:50', NULL),  -- 11 ev9 x5
(5, NEWID(), 100.00, 'JOSE RAMIREZ',   '1188', 'Pagada',      '2026-10-07 13:25', NULL),  -- 12 ev9 x5 (agota el evento)
(2, NEWID(),  25.00, 'CARLOS MENDOZA', '0000', 'Rechazada',   '2026-10-14 17:00', NULL),  -- 13 ev4 x1 (pago rechazado)
(8, NEWID(),  50.00, 'ROSA HUAMAN',    '6634', 'Pagada',      '2026-10-14 18:30', NULL),  -- 14 ev4 x2
(6, NEWID(),  60.00, 'ANA FLORES',     '7302', 'Pagada',      '2026-10-05 10:10', NULL);  -- 15 ev8 x2 (evento luego suspendido)

INSERT INTO DetalleCompra (IdCompra, IdEvento, Cantidad, PrecioUnitario, Subtotal) VALUES
(1, 5, 2, 45.00,  90.00),
(2, 5, 1, 45.00,  45.00),
(3, 5, 3, 45.00, 135.00),
(4, 1, 2, 60.00, 120.00),
(5, 1, 4, 60.00, 240.00),
(6, 2, 1, 85.00,  85.00),
(7, 3, 2,  0.00,   0.00),
(8, 1, 2, 60.00, 120.00),
(9, 6, 2, 70.00, 140.00),
(10, 6, 1, 70.00, 70.00),
(11, 9, 5, 20.00, 100.00),
(12, 9, 5, 20.00, 100.00),
(13, 4, 1, 25.00,  25.00),
(14, 4, 2, 25.00,  50.00),
(15, 8, 2, 30.00,  60.00);

/* ============================ ENTRADAS (QR) ============================ */
-- Una entrada por cada unidad comprada (las compras rechazadas no generan entradas)
INSERT INTO Entrada (IdDetalleCompra, NumeroEntrada, Estado, FechaGeneracion)
SELECT d.IdDetalleCompra, n.Numero, 'Valida', c.FechaCompra
FROM DetalleCompra d
INNER JOIN Compra c ON c.IdCompra = d.IdCompra
CROSS APPLY (SELECT TOP (d.Cantidad) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS Numero
             FROM sys.all_objects) n
WHERE c.Estado <> 'Rechazada';

-- Evento cancelado (6): entradas anuladas
UPDATE e SET Estado = 'Anulada'
FROM Entrada e INNER JOIN DetalleCompra d ON d.IdDetalleCompra = e.IdDetalleCompra
WHERE d.IdEvento = 6;

-- Evento finalizado (5): 5 de 6 asistentes ingresaron (1 entrada de Ana no se usó)
UPDATE e SET Estado = 'Utilizada'
FROM Entrada e INNER JOIN DetalleCompra d ON d.IdDetalleCompra = e.IdDetalleCompra
WHERE d.IdEvento = 5 AND NOT (d.IdCompra = 3 AND e.NumeroEntrada = 3);

/* ============================ CHECK-IN ============================ */
INSERT INTO CheckIn (IdEntrada, IdUsuarioValidador, FechaHora)
SELECT e.IdEntrada, 2, DATEADD(MINUTE, e.IdEntrada * 3, '2026-09-20 18:30')
FROM Entrada e
INNER JOIN DetalleCompra d ON d.IdDetalleCompra = e.IdDetalleCompra
WHERE d.IdEvento = 5 AND e.Estado = 'Utilizada';

/* ============ ENTRADAS VENDIDAS (cupos) según las entradas válidas o usadas ============ */
UPDATE ev SET EntradasVendidas = ISNULL(x.Total, 0)
FROM Evento ev
LEFT JOIN (SELECT d.IdEvento, COUNT(*) AS Total
           FROM Entrada e INNER JOIN DetalleCompra d ON d.IdDetalleCompra = e.IdDetalleCompra
           WHERE e.Estado IN ('Valida', 'Utilizada')
           GROUP BY d.IdEvento) x ON x.IdEvento = ev.IdEvento;

/* ============================ VISITAS (recomendaciones) ============================ */
INSERT INTO EventoVisita (IdUsuario, IdEvento, FechaVisita) VALUES
(4, 1, '2026-10-04'), (4, 3, '2026-10-11'), (4, 2, '2026-10-13'),
(5, 1, '2026-10-07'), (5, 4, '2026-10-09'), (5, 9, '2026-10-06'),
(6, 2, '2026-10-09'), (6, 8, '2026-10-04'), (6, 4, '2026-10-16'),
(3, 1, '2026-10-12');

/* ============================ RESEÑAS ============================ */
-- Solo asistentes del evento finalizado (5)
INSERT INTO Resena (IdUsuario, IdEvento, Calificacion, Comentario, Visible, FechaPublicacion) VALUES
(4, 5, 5, 'Excelente ambiente y muy buena música. Volvería sin dudarlo.', 1, '2026-09-21 10:00'),
(5, 5, 4, 'Muy bonito evento, aunque el sonido falló un poco al inicio.', 1, '2026-09-22 15:30'),
(6, 5, 1, 'Comentario retirado por el administrador por lenguaje inapropiado.', 0, '2026-09-23 20:00');

/* ============================ NOTIFICACIONES ============================ */
INSERT INTO Notificacion (IdUsuario, IdEvento, Tipo, Titulo, Mensaje, Leida, FechaCreacion, FechaLectura) VALUES
(4, 1, 'Compra',       'Compra confirmada',              'Tu compra de 2 entradas para «Noche de Rock en Barranco» fue realizada con éxito.', 1, '2026-10-05 09:31', '2026-10-05 09:40'),
(5, 1, 'Compra',       'Compra confirmada',              'Tu compra de 4 entradas para «Noche de Rock en Barranco» fue realizada con éxito.', 1, '2026-10-08 21:11', '2026-10-08 21:15'),
(4, 5, 'Recordatorio', 'Tu evento es mañana',            '«Jazz en el Parque» empieza mañana a las 19:00 en Parque Kennedy, Miraflores.', 1, '2026-09-19 19:00', '2026-09-19 20:10'),
(4, 1, 'Cambio',       'Cambio de horario',              '«Noche de Rock en Barranco» cambió su horario. Antes: 14/11 19:00 – Ahora: 14/11 20:00.', 0, '2026-10-10 12:00', NULL),
(5, 1, 'Cambio',       'Cambio de horario',              '«Noche de Rock en Barranco» cambió su horario. Antes: 14/11 19:00 – Ahora: 14/11 20:00.', 0, '2026-10-10 12:00', NULL),
(5, 6, 'Cancelacion',  'Evento cancelado',               '«Festival de Salsa Lima 2026» fue cancelado. Motivo: el local no obtuvo la licencia municipal a tiempo. Tus 2 entradas fueron anuladas y se te devolvió S/ 140.00.', 0, '2026-10-15 09:00', NULL),
(6, 6, 'Cancelacion',  'Evento cancelado',               '«Festival de Salsa Lima 2026» fue cancelado. Motivo: el local no obtuvo la licencia municipal a tiempo. Tu entrada fue anulada y se te devolvió S/ 70.00.', 1, '2026-10-15 09:00', '2026-10-15 13:20'),
(2, 9, 'Agotado',      '¡Tu evento se agotó!',           'Se vendieron todas las entradas de «Charla de Finanzas Personales».', 0, '2026-10-07 13:26', NULL),
(8, 8, 'Suspension',   'Tu evento fue suspendido',       '«Feria Artesanal Cusqueña» fue suspendido por el administrador. Motivo: la dirección del evento no coincide con el permiso presentado.', 0, '2026-10-16 10:00', NULL),
(6, 8, 'Suspension',   'Evento suspendido',              '«Feria Artesanal Cusqueña» fue suspendido temporalmente. Tus entradas siguen siendo válidas.', 0, '2026-10-16 10:00', NULL);

/* ============================ MODERACIÓN ============================ */
INSERT INTO Moderacion (IdAdministrador, IdUsuarioAfectado, IdEventoAfectado, IdResenaAfectada, Accion, Motivo, FechaAccion) VALUES
(1, NULL, NULL, 3,    'OcultarResena',    'La reseña contenía lenguaje ofensivo hacia el organizador.', '2026-09-24 09:00'),
(1, 7,    NULL, NULL, 'SuspenderUsuario', 'Publicó comentarios ofensivos de forma reiterada.',          '2026-10-01 11:00'),
(1, NULL, 8,    NULL, 'SuspenderEvento',  'La dirección del evento no coincide con el permiso presentado.', '2026-10-16 10:00');
GO

SET NOEXEC OFF;
GO

/* ============================ RESUMEN ============================ */
SELECT ev.IdEvento, ev.Nombre, ev.Estado, ev.AforoMaximo, ev.EntradasVendidas,
       ev.AforoMaximo - ev.EntradasVendidas AS CuposDisponibles
FROM Evento ev ORDER BY ev.IdEvento;
