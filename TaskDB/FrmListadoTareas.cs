using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TaskDB
{
    public partial class FrmListadoTareas : Form
    {
        private readonly string cadenaConexion = @"Server=localhost;Database=GestionTareasDB;Integrated Security=True;";
        public FrmListadoTareas()
        {
            InitializeComponent();
        }

        private void FrmListadoTareas_Load(object sender, EventArgs e)
        {
            ConfigurarGrilla();
            CargarListadoTareas();
        }
        
        private void ConfigurarGrilla()
        {
            // RNF3.1: Ajustar columnas al ancho de la grilla
            dgvTareas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Buenas prácticas adicionales
            dgvTareas.ReadOnly = true;
            dgvTareas.AllowUserToAddRows = false;
            dgvTareas.AllowUserToDeleteRows = false;
            dgvTareas.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void CargarListadoTareas()
        {
            // RNF3.2: Control de excepciones de lectura
            try
            {
                // RF3.2 y RNF3.1: Consulta con alias para obtener encabezados limpios
                string query = @"SELECT 
                                    TareaID AS [Código], 
                                    Titulo AS [Título de la Tarea], 
                                    Descripcion AS [Descripción], 
                                    FechaVencimiento AS [Fecha de Vencimiento], 
                                    Estado AS [Estado Actual]
                                 FROM Tareas";

                // RF3.2: Uso de SqlDataAdapter y DataTable
                using (SqlConnection conexion = new SqlConnection(cadenaConexion))
                {
                    using (SqlCommand comando = new SqlCommand(query, conexion))
                    {
                        using (SqlDataAdapter adaptador = new SqlDataAdapter(comando))
                        {
                            DataTable dtTareas = new DataTable();
                            adaptador.Fill(dtTareas);

                            // Poblar la grilla
                            dgvTareas.DataSource = dtTareas;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Error de lectura en la base de datos:\n{ex.Message}",
                                "Error de BD",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado al consultar los datos:\n{ex.Message}",
                                "Error Inesperado",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
    }
}
