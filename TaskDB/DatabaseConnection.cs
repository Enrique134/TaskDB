using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace TaskDB
{
    /// <summary>
    /// RF1.2: Clase de acceso a la base de datos TaskDB.
    /// Lee la cadena de conexión "TaskDBConnection" desde App.config (RNF1.1: usa |DataDirectory|)
    /// y crea la base de datos TaskDB.mdf en LocalDB si todavía no existe (RF1.1).
    /// Uso desde los formularios (Personas 2, 3 y 4):
    ///     using (SqlConnection cn = DatabaseConnection.GetConnection()) { cn.Open(); ... }
    /// </summary>
    public static class DatabaseConnection
    {
        /// <summary>Nombre de la cadena de conexión en App.config.</summary>
        public const string NombreCadena = "TaskDBConnection";

        /// <summary>Servidor LocalDB usado para crear la base de datos.</summary>
        private const string CadenaMaster = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30";

        /// <summary>
        /// Cadena de conexión dinámica: se obtiene de App.config y |DataDirectory|
        /// se resuelve en tiempo de ejecución según la carpeta de la aplicación.
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings config = ConfigurationManager.ConnectionStrings[NombreCadena];
                if (config == null || string.IsNullOrWhiteSpace(config.ConnectionString))
                {
                    throw new ConfigurationErrorsException(
                        "No se encontró la cadena de conexión '" + NombreCadena + "' en App.config.");
                }
                return config.ConnectionString;
            }
        }

        /// <summary>Carpeta a la que apunta |DataDirectory|.</summary>
        public static string CarpetaDatos
        {
            get
            {
                string carpeta = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (string.IsNullOrEmpty(carpeta))
                {
                    carpeta = AppDomain.CurrentDomain.BaseDirectory;
                }
                return carpeta;
            }
        }

        /// <summary>Ruta completa del archivo TaskDB.mdf.</summary>
        public static string RutaMdf
        {
            get { return Path.Combine(CarpetaDatos, "TaskDB.mdf"); }
        }

        /// <summary>Devuelve una nueva conexión (cerrada) a TaskDB.</summary>
        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        /// <summary>Prueba si es posible abrir la conexión con la base de datos.</summary>
        public static bool TestConnection()
        {
            string error;
            return TestConnection(out error);
        }

        /// <summary>Prueba la conexión y devuelve el mensaje de error, si lo hay.</summary>
        public static bool TestConnection(out string mensajeError)
        {
            mensajeError = null;
            try
            {
                using (SqlConnection cn = GetConnection())
                {
                    cn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                mensajeError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// RF1.1: Si |DataDirectory|\TaskDB.mdf no existe, crea la base de datos en LocalDB,
        /// crea la tabla Tareas, inserta datos de ejemplo y separa (detach) la base de datos
        /// para que la cadena con AttachDbFilename la pueda adjuntar.
        /// </summary>
        public static void InicializarBaseDatos()
        {
            string rutaMdf = RutaMdf;
            if (File.Exists(rutaMdf))
            {
                return;
            }

            Directory.CreateDirectory(CarpetaDatos);
            string rutaLdf = Path.Combine(CarpetaDatos, "TaskDB_log.ldf");
            if (File.Exists(rutaLdf))
            {
                File.Delete(rutaLdf); // Un .ldf huérfano impediría crear la base de datos.
            }

            // Nombre temporal único para no chocar con otra base TaskDB registrada en LocalDB.
            string nombreTemporal = "TaskDB_" + Guid.NewGuid().ToString("N");

            using (SqlConnection cn = new SqlConnection(CadenaMaster))
            {
                cn.Open();
                string crear = string.Format(
                    "CREATE DATABASE [{0}] ON PRIMARY (NAME = N'TaskDB', FILENAME = N'{1}') " +
                    "LOG ON (NAME = N'TaskDB_log', FILENAME = N'{2}')",
                    nombreTemporal, EscaparSql(rutaMdf), EscaparSql(rutaLdf));
                using (SqlCommand cmd = new SqlCommand(crear, cn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            // Crear la tabla Tareas y los datos de ejemplo.
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(CadenaMaster);
            builder.InitialCatalog = nombreTemporal;
            using (SqlConnection cn = new SqlConnection(builder.ConnectionString))
            {
                cn.Open();
                foreach (string lote in DividirEnLotes(ObtenerScript()))
                {
                    using (SqlCommand cmd = new SqlCommand(lote, cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // Separar la base de datos para que AttachDbFilename la adjunte con |DataDirectory|.
            SqlConnection.ClearAllPools();
            using (SqlConnection cn = new SqlConnection(CadenaMaster))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand("EXEC sp_detach_db @dbname = @nombre", cn))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombreTemporal);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>Lee Database\TaskDB.sql; si no está disponible usa el script embebido.</summary>
        private static string ObtenerScript()
        {
            string rutaScript = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "TaskDB.sql");
            if (File.Exists(rutaScript))
            {
                return File.ReadAllText(rutaScript);
            }
            return ScriptPorDefecto;
        }

        /// <summary>Divide el script en lotes separados por líneas "GO".</summary>
        private static string[] DividirEnLotes(string script)
        {
            string[] partes = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            System.Collections.Generic.List<string> lotes = new System.Collections.Generic.List<string>();
            foreach (string parte in partes)
            {
                if (!string.IsNullOrWhiteSpace(parte))
                {
                    lotes.Add(parte);
                }
            }
            return lotes.ToArray();
        }

        private static string EscaparSql(string texto)
        {
            return texto.Replace("'", "''");
        }

        /// <summary>Copia del script de Database\TaskDB.sql por si el archivo no se copió a la salida.</summary>
        private const string ScriptPorDefecto = @"
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
IF NOT EXISTS (SELECT 1 FROM dbo.Tareas)
BEGIN
    INSERT INTO dbo.Tareas (Titulo, Descripcion, Estado) VALUES
        (N'Crear base de datos TaskDB.mdf', N'Crear la base de datos en LocalDB con la tabla Tareas', N'Completada'),
        (N'Diseñar UI de captura de datos', N'Diseñar el formulario FrmAgregarTarea', N'Pendiente'),
        (N'Probar consultas SQL parametrizadas', N'Usar parámetros como @Titulo para evitar SQL Injection', N'Pendiente');
END
GO
";
    }
}
