-- =============================================================
-- TaskDB.sql
-- Script de la base de datos TaskDB (Persona 1 - Dev Base de Datos & Git)
-- Crea la tabla Tareas y agrega datos de ejemplo.
-- Lo ejecuta automáticamente DatabaseConnection.InicializarBaseDatos()
-- cuando el archivo |DataDirectory|\TaskDB.mdf todavía no existe.
-- También se puede ejecutar manualmente en SQL Server Object Explorer.
-- =============================================================

IF OBJECT_ID(N'dbo.Tareas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tareas
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tareas PRIMARY KEY,
        Titulo        NVARCHAR(100) NOT NULL,
        Descripcion   NVARCHAR(500) NULL,
        Estado        NVARCHAR(20)  NOT NULL CONSTRAINT DF_Tareas_Estado DEFAULT (N'Pendiente'),
        FechaCreacion DATETIME      NOT NULL CONSTRAINT DF_Tareas_FechaCreacion DEFAULT (GETDATE()),
        CONSTRAINT CK_Tareas_Estado CHECK (Estado IN (N'Pendiente', N'Completada'))
    );
END
GO

-- Datos de ejemplo (según el mockup de FrmListadoTareas)
IF NOT EXISTS (SELECT 1 FROM dbo.Tareas)
BEGIN
    INSERT INTO dbo.Tareas (Titulo, Descripcion, Estado) VALUES
        (N'Crear base de datos TaskDB.mdf', N'Crear la base de datos en LocalDB con la tabla Tareas', N'Completada'),
        (N'Diseñar UI de captura de datos', N'Diseñar el formulario FrmAgregarTarea', N'Pendiente'),
        (N'Probar consultas SQL parametrizadas', N'Usar parámetros como @Titulo para evitar SQL Injection', N'Pendiente');
END
GO
