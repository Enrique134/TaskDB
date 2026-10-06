using System;
using System.IO;
using System.Windows.Forms;

namespace TaskDB
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // RNF1.1: |DataDirectory| apunta a la carpeta "Data" junto al ejecutable.
            string carpetaDatos = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(carpetaDatos);
            AppDomain.CurrentDomain.SetData("DataDirectory", carpetaDatos);

            // RF1.1: Crear TaskDB.mdf con la tabla Tareas si todavía no existe.
            try
            {
                DatabaseConnection.InicializarBaseDatos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo crear la base de datos TaskDB.mdf.\n" +
                    "Verifique que SQL Server Express LocalDB esté instalado.\n\n" + ex.Message,
                    "TaskDB - Error de base de datos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Application.Run(new formAgregarTarea());
        }
    }
}
