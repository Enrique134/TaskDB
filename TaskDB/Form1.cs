using System;
using System.Windows.Forms;

namespace TaskDB
{
    /// <summary>
    /// Formulario principal de TaskDB. Aquí se integrarán los formularios de las Personas 2, 3 y 4.
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>Prueba la conexión con TaskDB.mdf mediante DatabaseConnection.</summary>
        private void btnProbarConexion_Click(object sender, EventArgs e)
        {
            string error;
            if (DatabaseConnection.TestConnection(out error))
            {
                MessageBox.Show("Conexión exitosa con la base de datos TaskDB.\nArchivo: " + DatabaseConnection.RutaMdf,
                    "Probar conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se pudo conectar con la base de datos TaskDB.\n\n" + error,
                    "Probar conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
