/* =====================================================================
   EventosPeruIA - Base de datos completa (SQL Server)
   Historias de usuario US-01 a US-16
   Ejecutar completo con F5. Borra la BD si existe y la crea desde cero.
   ===================================================================== */
USE master;
GO
IF DB_ID('EventosPeruIA') IS NOT NULL
BEGIN
    ALTER DATABASE EventosPeruIA SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE EventosPeruIA;
END
GO
CREATE DATABASE EventosPeruIA;
GO
USE EventosPeruIA;
GO

/* =====================================================================
   1. USUARIOS, ROLES E INTERESES   (US-01, US-05, US-14)
   ===================================================================== */
CREATE TABLE Rol (
    IdRol   INT IDENTITY(1,1) CONSTRAINT PK_Rol PRIMARY KEY,
    Nombre  VARCHAR(30) NOT NULL CONSTRAINT UQ_Rol_Nombre UNIQUE
);

CREATE TABLE Usuario (
    IdUsuario          INT IDENTITY(1,1) CONSTRAINT PK_Usuario PRIMARY KEY,
    Nombre             VARCHAR(50)  NOT NULL,
    Apellido           VARCHAR(50)  NOT NULL,
    Correo             VARCHAR(100) NOT NULL CONSTRAINT UQ_Usuario_Correo UNIQUE,
    Celular            CHAR(9)      NOT NULL,
    FechaNacimiento    DATE         NOT NULL,
    Departamento       VARCHAR(50)  NOT NULL,
    Distrito           VARCHAR(50)  NOT NULL,
    PasswordHash       VARCHAR(255) NOT NULL,
    NombreOrganizador  VARCHAR(80)  NULL,          -- solo si tiene rol Organizador
    Estado             VARCHAR(20)  NOT NULL CONSTRAINT DF_Usuario_Estado DEFAULT 'Activo',
    MotivoSuspension   VARCHAR(500) NULL,
    FechaRegistro      DATETIME2    NOT NULL CONSTRAINT DF_Usuario_FechaRegistro DEFAULT SYSDATETIME(),
    CONSTRAINT CK_Usuario_Celular CHECK (Celular LIKE '9[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'),
    CONSTRAINT CK_Usuario_Estado  CHECK (Estado IN ('Activo', 'Suspendido'))
);
CREATE UNIQUE INDEX UX_Usuario_NombreOrganizador
    ON Usuario(NombreOrganizador) WHERE NombreOrganizador IS NOT NULL;

-- Doble rol: un usuario puede ser Asistente y Organizador a la vez
CREATE TABLE UsuarioRol (
    IdUsuario       INT       NOT NULL,
    IdRol           INT       NOT NULL,
    FechaAsignacion DATETIME2 NOT NULL CONSTRAINT DF_UsuarioRol_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT PK_UsuarioRol PRIMARY KEY (IdUsuario, IdRol),
    CONSTRAINT FK_UsuarioRol_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_UsuarioRol_Rol     FOREIGN KEY (IdRol)     REFERENCES Rol(IdRol)
);

CREATE TABLE Interes (
    IdInteres INT IDENTITY(1,1) CONSTRAINT PK_Interes PRIMARY KEY,
    Nombre    VARCHAR(80) NOT NULL CONSTRAINT UQ_Interes_Nombre UNIQUE,
    Estado    BIT         NOT NULL CONSTRAINT DF_Interes_Estado DEFAULT 1
);

CREATE TABLE UsuarioInteres (
    IdUsuario     INT       NOT NULL,
    IdInteres     INT       NOT NULL,
    FechaRegistro DATETIME2 NOT NULL CONSTRAINT DF_UsuarioInteres_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT PK_UsuarioInteres PRIMARY KEY (IdUsuario, IdInteres),
    CONSTRAINT FK_UsuarioInteres_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_UsuarioInteres_Interes FOREIGN KEY (IdInteres) REFERENCES Interes(IdInteres)
);

/* =====================================================================
   2. CATEGORÍAS Y EVENTOS   (US-04, US-07, US-09, US-13)
   ===================================================================== */
CREATE TABLE Categoria (
    IdCategoria   INT IDENTITY(1,1) CONSTRAINT PK_Categoria PRIMARY KEY,
    Nombre        VARCHAR(80)  NOT NULL CONSTRAINT UQ_Categoria_Nombre UNIQUE,
    Descripcion   VARCHAR(250) NULL,
    Estado        BIT          NOT NULL CONSTRAINT DF_Categoria_Estado DEFAULT 1,   -- 1 Activa / 0 Inactiva
    FechaCreacion DATETIME2    NOT NULL CONSTRAINT DF_Categoria_Fecha DEFAULT SYSDATETIME()
);

CREATE TABLE Evento (
    IdEvento          INT IDENTITY(1,1) CONSTRAINT PK_Evento PRIMARY KEY,
    IdOrganizador     INT           NOT NULL,
    IdCategoria       INT           NOT NULL,
    Nombre            VARCHAR(150)  NOT NULL,
    Descripcion       VARCHAR(1000) NOT NULL,
    FechaHoraInicio   DATETIME2     NOT NULL,
    FechaHoraFin      DATETIME2     NOT NULL,
    Departamento      VARCHAR(50)   NOT NULL,
    Distrito          VARCHAR(50)   NOT NULL,
    Direccion         VARCHAR(250)  NOT NULL,
    ImagenUrl         VARCHAR(500)  NULL,
    AforoMaximo       INT           NOT NULL,
    EntradasVendidas  INT           NOT NULL CONSTRAINT DF_Evento_EntradasVendidas DEFAULT 0,
    Precio            DECIMAL(10,2) NOT NULL,
    Estado            VARCHAR(20)   NOT NULL CONSTRAINT DF_Evento_Estado DEFAULT 'Borrador',
    MotivoCancelacion VARCHAR(500)  NULL,
    FechaCancelacion  DATETIME2     NULL,
    FechaCreacion     DATETIME2     NOT NULL CONSTRAINT DF_Evento_FechaCreacion DEFAULT SYSDATETIME(),
    FechaModificacion DATETIME2     NULL,
    CONSTRAINT FK_Evento_Organizador FOREIGN KEY (IdOrganizador) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Evento_Categoria   FOREIGN KEY (IdCategoria)   REFERENCES Categoria(IdCategoria),
    CONSTRAINT CK_Evento_Fechas   CHECK (FechaHoraFin > FechaHoraInicio),
    CONSTRAINT CK_Evento_Aforo    CHECK (AforoMaximo BETWEEN 1 AND 10000),
    -- Nunca vender más que el aforo ni bajar el aforo por debajo de lo vendido
    CONSTRAINT CK_Evento_EntradasVendidas CHECK (EntradasVendidas >= 0 AND EntradasVendidas <= AforoMaximo),
    CONSTRAINT CK_Evento_Precio   CHECK (Precio BETWEEN 0 AND 9999.99),
    CONSTRAINT CK_Evento_Estado   CHECK (Estado IN ('Borrador', 'Publicado', 'Finalizado', 'Cancelado', 'Suspendido'))
);
CREATE INDEX IX_Evento_Busqueda    ON Evento(Estado, FechaHoraInicio);
CREATE INDEX IX_Evento_Categoria   ON Evento(IdCategoria);
CREATE INDEX IX_Evento_Organizador ON Evento(IdOrganizador);
CREATE INDEX IX_Evento_Ubicacion   ON Evento(Departamento, Distrito);

-- Eventos que visitó el usuario (señal para recomendaciones, US-14)
CREATE TABLE EventoVisita (
    IdEventoVisita INT IDENTITY(1,1) CONSTRAINT PK_EventoVisita PRIMARY KEY,
    IdUsuario      INT       NOT NULL,
    IdEvento       INT       NOT NULL,
    FechaVisita    DATETIME2 NOT NULL CONSTRAINT DF_EventoVisita_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT FK_EventoVisita_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_EventoVisita_Evento  FOREIGN KEY (IdEvento)  REFERENCES Evento(IdEvento)
);
CREATE INDEX IX_EventoVisita_Usuario ON EventoVisita(IdUsuario, FechaVisita);

/* =====================================================================
   3. COMPRAS, ENTRADAS Y CHECK-IN   (US-06, US-10, US-13)
   ===================================================================== */
CREATE TABLE Compra (
    IdCompra        INT IDENTITY(1,1) CONSTRAINT PK_Compra PRIMARY KEY,
    IdUsuario       INT              NOT NULL,
    -- Código único por intento de compra: evita compras/cobros duplicados
    CodigoOperacion UNIQUEIDENTIFIER NOT NULL CONSTRAINT UQ_Compra_CodigoOperacion UNIQUE,
    MontoTotal      DECIMAL(10,2)    NOT NULL,
    TitularTarjeta  VARCHAR(100)     NULL,         -- NULL si el evento es gratis
    UltimosDigitos  CHAR(4)          NULL,         -- nunca se guarda la tarjeta completa
    Estado          VARCHAR(20)      NOT NULL CONSTRAINT DF_Compra_Estado DEFAULT 'Pendiente',
    FechaCompra     DATETIME2        NOT NULL CONSTRAINT DF_Compra_Fecha DEFAULT SYSDATETIME(),
    FechaReembolso  DATETIME2        NULL,
    CONSTRAINT FK_Compra_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    CONSTRAINT CK_Compra_Monto  CHECK (MontoTotal >= 0),
    CONSTRAINT CK_Compra_Estado CHECK (Estado IN ('Pendiente', 'Pagada', 'Rechazada', 'Reembolsada'))
);
CREATE INDEX IX_Compra_Usuario ON Compra(IdUsuario);
CREATE INDEX IX_Compra_Fecha   ON Compra(FechaCompra);

CREATE TABLE DetalleCompra (
    IdDetalleCompra INT IDENTITY(1,1) CONSTRAINT PK_DetalleCompra PRIMARY KEY,
    IdCompra        INT           NOT NULL,
    IdEvento        INT           NOT NULL,
    Cantidad        INT           NOT NULL,
    PrecioUnitario  DECIMAL(10,2) NOT NULL,
    Subtotal        DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_DetalleCompra_Compra FOREIGN KEY (IdCompra) REFERENCES Compra(IdCompra),
    CONSTRAINT FK_DetalleCompra_Evento FOREIGN KEY (IdEvento) REFERENCES Evento(IdEvento),
    CONSTRAINT CK_DetalleCompra_Cantidad CHECK (Cantidad BETWEEN 1 AND 10),
    CONSTRAINT CK_DetalleCompra_Subtotal CHECK (Subtotal = Cantidad * PrecioUnitario)
);
CREATE INDEX IX_DetalleCompra_Compra ON DetalleCompra(IdCompra);
CREATE INDEX IX_DetalleCompra_Evento ON DetalleCompra(IdEvento);

CREATE TABLE Entrada (
    IdEntrada       INT IDENTITY(1,1) CONSTRAINT PK_Entrada PRIMARY KEY,
    IdDetalleCompra INT              NOT NULL,
    CodigoEntrada   UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Entrada_Codigo DEFAULT NEWID()
                                              CONSTRAINT UQ_Entrada_Codigo UNIQUE,   -- contenido del QR
    NumeroEntrada   INT              NOT NULL,          -- "2 de 4"
    Estado          VARCHAR(20)      NOT NULL CONSTRAINT DF_Entrada_Estado DEFAULT 'Valida',
    FechaGeneracion DATETIME2        NOT NULL CONSTRAINT DF_Entrada_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Entrada_DetalleCompra FOREIGN KEY (IdDetalleCompra) REFERENCES DetalleCompra(IdDetalleCompra),
    CONSTRAINT CK_Entrada_Estado CHECK (Estado IN ('Valida', 'Utilizada', 'Anulada'))
);
CREATE INDEX IX_Entrada_DetalleCompra ON Entrada(IdDetalleCompra);

-- Una entrada solo puede registrar un ingreso (QR de un solo uso)
CREATE TABLE CheckIn (
    IdCheckIn          INT IDENTITY(1,1) CONSTRAINT PK_CheckIn PRIMARY KEY,
    IdEntrada          INT       NOT NULL CONSTRAINT UQ_CheckIn_Entrada UNIQUE,
    IdUsuarioValidador INT       NOT NULL,          -- organizador que escaneó
    FechaHora          DATETIME2 NOT NULL CONSTRAINT DF_CheckIn_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT FK_CheckIn_Entrada   FOREIGN KEY (IdEntrada)          REFERENCES Entrada(IdEntrada),
    CONSTRAINT FK_CheckIn_Validador FOREIGN KEY (IdUsuarioValidador) REFERENCES Usuario(IdUsuario)
);

/* =====================================================================
   4. RESEÑAS   (US-15)
   ===================================================================== */
CREATE TABLE Resena (
    IdResena          INT IDENTITY(1,1) CONSTRAINT PK_Resena PRIMARY KEY,
    IdUsuario         INT          NOT NULL,
    IdEvento          INT          NOT NULL,
    Calificacion      TINYINT      NOT NULL,
    Comentario        VARCHAR(500) NOT NULL,
    Visible           BIT          NOT NULL CONSTRAINT DF_Resena_Visible DEFAULT 1,  -- el ADM puede ocultarla
    FechaPublicacion  DATETIME2    NOT NULL CONSTRAINT DF_Resena_Fecha DEFAULT SYSDATETIME(),
    FechaModificacion DATETIME2    NULL,
    CONSTRAINT FK_Resena_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Resena_Evento  FOREIGN KEY (IdEvento)  REFERENCES Evento(IdEvento),
    CONSTRAINT UQ_Resena_UsuarioEvento UNIQUE (IdUsuario, IdEvento),          -- una reseña por evento
    CONSTRAINT CK_Resena_Calificacion CHECK (Calificacion BETWEEN 1 AND 5),
    CONSTRAINT CK_Resena_Comentario   CHECK (LEN(Comentario) BETWEEN 10 AND 500)
);
CREATE INDEX IX_Resena_Evento ON Resena(IdEvento);

/* =====================================================================
   5. NOTIFICACIONES   (US-12)
   ===================================================================== */
CREATE TABLE Notificacion (
    IdNotificacion INT IDENTITY(1,1) CONSTRAINT PK_Notificacion PRIMARY KEY,
    IdUsuario      INT           NOT NULL,
    IdEvento       INT           NULL,
    Tipo           VARCHAR(20)   NOT NULL,
    Titulo         VARCHAR(150)  NOT NULL,
    Mensaje        VARCHAR(1000) NOT NULL,
    Leida          BIT           NOT NULL CONSTRAINT DF_Notificacion_Leida DEFAULT 0,
    FechaCreacion  DATETIME2     NOT NULL CONSTRAINT DF_Notificacion_Fecha DEFAULT SYSDATETIME(),
    FechaLectura   DATETIME2     NULL,
    CONSTRAINT FK_Notificacion_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Notificacion_Evento  FOREIGN KEY (IdEvento)  REFERENCES Evento(IdEvento),
    CONSTRAINT CK_Notificacion_Tipo CHECK (Tipo IN ('Compra', 'Recordatorio', 'Cambio', 'Cancelacion', 'Agotado', 'Suspension'))
);
CREATE INDEX IX_Notificacion_Usuario ON Notificacion(IdUsuario, Leida);
-- Evita enviar dos veces el recordatorio de 24 h del mismo evento al mismo usuario
CREATE UNIQUE INDEX UX_Notificacion_Recordatorio
    ON Notificacion(IdUsuario, IdEvento) WHERE Tipo = 'Recordatorio';

/* =====================================================================
   6. MODERACIÓN   (US-16)
   ===================================================================== */
CREATE TABLE Moderacion (
    IdModeracion      INT IDENTITY(1,1) CONSTRAINT PK_Moderacion PRIMARY KEY,
    IdAdministrador   INT          NOT NULL,
    IdUsuarioAfectado INT          NULL,
    IdEventoAfectado  INT          NULL,
    IdResenaAfectada  INT          NULL,
    Accion            VARCHAR(30)  NOT NULL,
    Motivo            VARCHAR(500) NOT NULL,
    FechaAccion       DATETIME2    NOT NULL CONSTRAINT DF_Moderacion_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Moderacion_Administrador FOREIGN KEY (IdAdministrador)   REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Moderacion_Usuario       FOREIGN KEY (IdUsuarioAfectado) REFERENCES Usuario(IdUsuario),
    CONSTRAINT FK_Moderacion_Evento        FOREIGN KEY (IdEventoAfectado)  REFERENCES Evento(IdEvento),
    CONSTRAINT FK_Moderacion_Resena        FOREIGN KEY (IdResenaAfectada)  REFERENCES Resena(IdResena),
    CONSTRAINT CK_Moderacion_Accion CHECK (Accion IN ('SuspenderEvento', 'ReactivarEvento', 'SuspenderUsuario', 'ReactivarUsuario', 'OcultarResena')),
    CONSTRAINT CK_Moderacion_Motivo CHECK (LEN(Motivo) >= 10),
    -- Cada acción afecta exactamente a un elemento (usuario, evento o reseña)
    CONSTRAINT CK_Moderacion_Objetivo CHECK (
        (CASE WHEN IdUsuarioAfectado IS NULL THEN 0 ELSE 1 END
       + CASE WHEN IdEventoAfectado  IS NULL THEN 0 ELSE 1 END
       + CASE WHEN IdResenaAfectada  IS NULL THEN 0 ELSE 1 END) = 1)
);
CREATE INDEX IX_Moderacion_Fecha ON Moderacion(FechaAccion);

/* =====================================================================
   7. IA - PeruIA con RAG   (US-08)
   ===================================================================== */
CREATE TABLE PreguntaFrecuente (
    IdPregunta INT IDENTITY(1,1) CONSTRAINT PK_PreguntaFrecuente PRIMARY KEY,
    Pregunta   VARCHAR(300)  NOT NULL,
    Respuesta  VARCHAR(2000) NOT NULL,
    Estado     BIT           NOT NULL CONSTRAINT DF_PreguntaFrecuente_Estado DEFAULT 1
);

-- Embeddings de eventos y preguntas frecuentes (vector guardado como JSON)
CREATE TABLE Embedding (
    IdEmbedding   INT IDENTITY(1,1) CONSTRAINT PK_Embedding PRIMARY KEY,
    TipoOrigen    VARCHAR(10)   NOT NULL,   -- 'Evento' o 'FAQ'
    IdOrigen      INT           NOT NULL,   -- IdEvento o IdPregunta
    Texto         NVARCHAR(MAX) NOT NULL,
    Vector        NVARCHAR(MAX) NOT NULL,   -- [0.012, -0.334, ...]
    FechaIndexado DATETIME2     NOT NULL CONSTRAINT DF_Embedding_Fecha DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Embedding_Origen UNIQUE (TipoOrigen, IdOrigen),
    CONSTRAINT CK_Embedding_Tipo CHECK (TipoOrigen IN ('Evento', 'FAQ'))
);
GO

/* =====================================================================
   8. DATOS INICIALES
   ===================================================================== */
INSERT INTO Rol (Nombre) VALUES
('Asistente'),       -- IdRol 1
('Organizador'),     -- IdRol 2
('Administrador');   -- IdRol 3

INSERT INTO Categoria (Nombre, Descripcion) VALUES
('Concierto', 'Presentaciones musicales en vivo'),
('Taller',    'Actividades prácticas de aprendizaje'),
('Feria',     'Exposiciones y venta de productos'),
('Charla',    'Conferencias y conversatorios');

INSERT INTO Interes (Nombre) VALUES
('Música'), ('Tecnología'), ('Gastronomía'), ('Arte'),
('Deporte'), ('Negocios'), ('Educación'), ('Cultura');

INSERT INTO PreguntaFrecuente (Pregunta, Respuesta) VALUES
('¿Cómo compro una entrada?', 'Ingresa al evento, elige la cantidad (máximo 10), presiona Comprar y completa el pago. Recibirás un código QR por cada entrada en Mis entradas.'),
('¿Cómo ingreso al evento?', 'Muestra el código QR de tu entrada en la puerta. El organizador lo escaneará y cada QR solo puede usarse una vez.'),
('¿Qué pasa si cancelan un evento?', 'Tus entradas se anulan, se te devuelve el monto total pagado y recibirás una notificación con el motivo.'),
('¿Puedo publicar eventos y también comprar entradas?', 'Sí. Con una sola cuenta puedes ser asistente y organizador; activa la opción Quiero publicar eventos desde tu perfil.'),
('¿Puedo dejar una reseña?', 'Sí, si asististe al evento. Puedes calificar de 1 a 5 estrellas hasta 30 días después de que termine.');
GO

PRINT 'Base de datos EventosPeruIA creada correctamente.';

/* ---- Crear el primer administrador ----
   1. Registra un usuario desde la API (registro normal), por ejemplo admin@eventosperu.pe
   2. Ejecuta:
INSERT INTO UsuarioRol (IdUsuario, IdRol)
SELECT IdUsuario, 3 FROM Usuario WHERE Correo = 'admin@eventosperu.pe';
------------------------------------------ */
