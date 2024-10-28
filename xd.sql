USE servicio_ventas;
GO
CREATE TABLE usuario (
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    Correo NVARCHAR(100) NOT NULL UNIQUE,
    Clave NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL,
);

CREATE TABLE Concerts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Fecha DATETIME NOT NULL,
    Lugar NVARCHAR(100) NOT NULL,
    Seccion NVARCHAR(50) NOT NULL,
    Precio DECIMAL(18, 2) NOT NULL,
    CantidadDisponibles INT NOT NULL
);

CREATE TABLE UsuarioEntradasConciertos (
    Id INT IDENTITY PRIMARY KEY,
    UsuarioId INT NOT NULL,                -- ID del usuario que realiza la compra
    ConciertoId INT NOT NULL,              -- ID del concierto comprado
    CantidadEntradas INT NOT NULL,         -- Cantidad de entradas compradas
    FOREIGN KEY (UsuarioId) REFERENCES usuario(Id),   -- Asumiendo que tienes una tabla Usuarios
    FOREIGN KEY (ConciertoId) REFERENCES Concerts(Id) -- Asumiendo que tienes una tabla Conciertos
);


USE servicio_ventas;
GO

-- Eliminar el procedimiento si ya existe
IF OBJECT_ID('sp_ValidarUsuario', 'P') IS NOT NULL
    DROP PROCEDURE sp_ValidarUsuario;
GO

CREATE PROCEDURE sp_ValidarUsuario
    @Correo VARCHAR(100),
    @Clave VARCHAR(500)
AS
BEGIN
    DECLARE @IdUsuario INT;

    -- Buscar el IdUsuario y el Rol si el usuario existe con el correo y clave dados
    SELECT @IdUsuario = IdUsuario
    FROM USUARIO
    WHERE Correo = @Correo AND Clave = @Clave;

    IF @IdUsuario IS NOT NULL
    BEGIN
        -- Si el usuario existe, devolver IdUsuario y Rol
        SELECT IdUsuario, Rol
        FROM USUARIO
        WHERE Correo = @Correo AND Clave = @Clave;
    END
    ELSE
    BEGIN
        -- Si no existe, devolver '0' como IdUsuario y un valor nulo para Rol
        SELECT 0 AS IdUsuario, NULL AS Rol;
    END
END;
GO


USE servicio_ventas;
GO

-- Eliminar el procedimiento si ya existe
IF OBJECT_ID('sp_RegistrarUsuario', 'P') IS NOT NULL
    DROP PROCEDURE sp_RegistrarUsuario;
GO

CREATE PROCEDURE sp_RegistrarUsuario
    @Correo VARCHAR(100),
    @Clave VARCHAR(500),
    @Registrado BIT OUTPUT,
    @Mensaje VARCHAR(100) OUTPUT
AS
BEGIN
    IF (NOT EXISTS (SELECT 1 FROM USUARIO WHERE Correo = @Correo))
    BEGIN
        INSERT INTO USUARIO (Correo, Clave) VALUES (@Correo, @Clave)
        SET @Registrado = 1
        SET @Mensaje = 'Usuario registrado'
    END
    ELSE
    BEGIN
        SET @Registrado = 0
        SET @Mensaje = 'Correo ya existe'
    END
END;
GO

CREATE PROCEDURE sp_ListarConciertos
AS
BEGIN
    SELECT Id, Nombre, Fecha, Lugar, Seccion, Precio, CantidadDisponibles
    FROM Conciertos
END

CREATE PROCEDURE sp_CrearConcierto
    @Nombre NVARCHAR(100),
    @Fecha DATETIME,
    @Lugar NVARCHAR(100),
    @Seccion NVARCHAR(50),
    @Precio DECIMAL(18, 2),
    @CantidadDisponibles INT,
    @Registrado BIT OUTPUT,
    @Mensaje NVARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM Conciertos WHERE Nombre = @Nombre AND Fecha = @Fecha)
    BEGIN
        SET @Registrado = 0
        SET @Mensaje = 'El concierto ya existe.'
        RETURN
    END

    INSERT INTO Conciertos (Nombre, Fecha, Lugar, Seccion, Precio, CantidadDisponibles)
    VALUES (@Nombre, @Fecha, @Lugar, @Seccion, @Precio, @CantidadDisponibles)

    SET @Registrado = 1
    SET @Mensaje = 'Concierto creado exitosamente.'
END

CREATE PROCEDURE sp_ObtenerConcierto
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Nombre, Fecha, Lugar, Seccion, Precio, CantidadDisponibles
    FROM Conciertos
    WHERE Id = @Id
END

CREATE PROCEDURE sp_ActualizarConcierto
    @Id INT,
    @Nombre NVARCHAR(100),
    @Fecha DATETIME,
    @Lugar NVARCHAR(100),
    @Seccion NVARCHAR(50),
    @Precio DECIMAL(18,2),
    @CantidadDisponibles INT,
    @Actualizado BIT OUTPUT,
    @Mensaje NVARCHAR(100) OUTPUT
AS
BEGIN
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM Concerts WHERE Id = @Id)
        BEGIN
            UPDATE Concerts
            SET Nombre = @Nombre,
                Fecha = @Fecha,
                Lugar = @Lugar,
                Seccion = @Seccion,
                Precio = @Precio,
                CantidadDisponibles = @CantidadDisponibles
            WHERE Id = @Id;

            SET @Actualizado = 1;
            SET @Mensaje = 'Concierto actualizado exitosamente.';
        END
        ELSE
        BEGIN
            SET @Actualizado = 0;
            SET @Mensaje = 'Concierto no encontrado.';
        END
    END TRY
    BEGIN CATCH
        SET @Actualizado = 0;
        SET @Mensaje = 'Error al actualizar el concierto.';
    END CATCH
END;

CREATE PROCEDURE sp_EliminarConcierto
    @Id INT,
    @Eliminado BIT OUTPUT,
    @Mensaje NVARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Conciertos WHERE Id = @Id)
    BEGIN
        SET @Eliminado = 0
        SET @Mensaje = 'Concierto no encontrado.'
        RETURN
    END

    DELETE FROM Conciertos WHERE Id = @Id

    SET @Eliminado = 1
    SET @Mensaje = 'Concierto eliminado exitosamente.'
END


CREATE PROCEDURE sp_GuardarCompraConcierto
    @UsuarioId INT,
    @ConciertoId INT,
    @CantidadComprada INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- Insertar el registro de la compra en la tabla UsuarioEntradasConciertos
        INSERT INTO UsuarioEntradasConciertos (UsuarioId, ConciertoId, CantidadEntradas)
        VALUES (@UsuarioId, @ConciertoId, @CantidadComprada);

        -- Actualizar la disponibilidad del concierto
        UPDATE Conciertos
        SET CantidadDisponibles = CantidadDisponibles - @CantidadComprada
        WHERE Id = @ConciertoId;

        -- Confirmar la transacción si todo es correcto
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        -- Revertir la transacción en caso de error
        ROLLBACK TRANSACTION;
        -- Manejar el error lanzando un mensaje
        THROW;
    END CATCH
END;

CREATE PROCEDURE sp_ObtenerHistorialCompras
    @UsuarioId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        U.IdUsuario AS UsuarioId,
        U.Correo AS CorreoUsuario,
        C.Id AS ConciertoId,
        C.Nombre AS NombreConcierto,
        C.Fecha AS FechaConcierto,
        C.Lugar AS LugarConcierto,
        C.Seccion AS SeccionConcierto,
        UEC.CantidadEntradas AS CantidadComprada,
        (C.Precio * UEC.CantidadEntradas) AS TotalCompra
    FROM 
        UsuarioEntradasConciertos UEC
    INNER JOIN 
        usuario U ON UEC.UsuarioId = U.IdUsuario
    INNER JOIN 
        Concerts C ON UEC.ConciertoId = C.Id
    WHERE 
        U.IdUsuario = @UsuarioId

END;
