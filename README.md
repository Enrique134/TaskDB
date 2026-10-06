# TaskDB — Gestión de Tareas

Aplicación de escritorio en **C# Windows Forms (.NET Framework 4.8)** con **SQL Server LocalDB** para crear, listar y cambiar el estado de tareas. Proyecto colaborativo de 4 personas con ramas independientes y Pull Requests en GitHub.

| Tecnología | Base de datos | Repositorio | Interfaz |
|---|---|---|---|
| .NET Framework 4.8 | SQL Server LocalDB | GitHub (Git Flow) | Windows Forms |

## Requisitos

- Visual Studio 2019/2022 con la carga de trabajo **Desarrollo de escritorio de .NET**.
- **SQL Server Express LocalDB** (`(LocalDB)\MSSQLLocalDB`), que se instala con Visual Studio.

## Cómo ejecutar

1. Clonar el repositorio: `git clone https://github.com/Enrique134/TaskDB.git`
2. Abrir `TaskDB.sln` en Visual Studio y presionar **F5**.
3. Al iniciar, la aplicación crea automáticamente `Data\TaskDB.mdf` (junto al ejecutable) con la tabla `Tareas` y 3 tareas de ejemplo.
4. En el formulario principal, el botón **Probar conexión** confirma que la base de datos funciona.

> Los archivos `*.mdf` y `*.ldf` están en el `.gitignore` de Visual Studio: cada integrante genera su propia base de datos local al ejecutar la aplicación, a partir del script `TaskDB/Database/TaskDB.sql`.

## Estructura

```
TaskDB/
├── .gitignore                    (plantilla oficial de Visual Studio)
├── README.md
├── TaskDB.sln
└── TaskDB/
    ├── App.config                (cadena de conexión "TaskDBConnection" con |DataDirectory|)
    ├── DatabaseConnection.cs     (conexión dinámica y creación de TaskDB.mdf)
    ├── Database/TaskDB.sql       (tabla Tareas y datos de ejemplo)
    ├── Form1.cs                  (formulario principal; aquí se integran los demás)
    ├── Program.cs                (asigna DataDirectory e inicializa la BD)
    └── Properties/
```

## Base de datos (Persona 1)

Tabla `Tareas`:

| Columna | Tipo | Detalle |
|---|---|---|
| Id | INT IDENTITY | Llave primaria |
| Titulo | NVARCHAR(100) | Obligatorio |
| Descripcion | NVARCHAR(500) | Opcional |
| Estado | NVARCHAR(20) | `'Pendiente'` por defecto; solo `'Pendiente'` o `'Completada'` |
| FechaCreacion | DATETIME | `GETDATE()` por defecto |

Cadena de conexión (App.config):

```xml
<add name="TaskDBConnection"
     connectionString="Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\TaskDB.mdf;Integrated Security=True;Connect Timeout=30"
     providerName="System.Data.SqlClient" />
```

Uso desde los formularios de las Personas 2, 3 y 4:

```csharp
using (SqlConnection cn = DatabaseConnection.GetConnection())
{
    cn.Open();
    SqlCommand cmd = new SqlCommand("INSERT INTO Tareas (Titulo, Descripcion, Estado) VALUES (@Titulo, @Descripcion, @Estado)", cn);
    cmd.Parameters.AddWithValue("@Titulo", txtTitulo.Text);
    // ...
}
```

## Roles y ramas

| Persona | Rol | Rama | Requerimientos |
|---|---|---|---|
| Persona 1 | Dev Base de Datos & Git | `feature/base-datos` | RF1.1 BD `TaskDB.mdf` con tabla `Tareas` · RF1.2 clase `DatabaseConnection` · RNF1.1 `\|DataDirectory\|` · RNF1.2 repositorio con `.gitignore` |
| Persona 2 | Dev Formulario Registro | `feature/form-registro` | RF2.1 `FrmAgregarTarea.cs` · RF2.2 inserción con `SqlCommand` · RNF2.1 validar Título · RNF2.2 parámetros SQL (`@Titulo`) |
| Persona 3 | Dev Formulario Listado | `feature/form-listado` | RF3.1 `FrmListadoTareas.cs` con `DataGridView` · RF3.2 `SqlDataAdapter` y `DataTable` · RNF3.1 `AutoSizeColumnsMode = Fill` · RNF3.2 `try-catch` |
| Persona 4 | Dev Integración & Filtros | `feature/integracion-filtros` | RF4.1 filtro por Estado con `ComboBox` · RF4.2 botón "Marcar como Completada" · RNF4.1 navegación principal · RNF4.2 revisar y fusionar PRs |

## Flujo de trabajo con Git

1. **Persona 1:** crea el repositorio en GitHub, sube la solución base con el `.gitignore` de Visual Studio e invita a los integrantes.
2. **Todos:** clonan el repositorio: `git clone https://github.com/Enrique134/TaskDB.git`
3. **Cada integrante:** crea su rama: `git checkout -b feature/nombre-modulo` (ej. `feature/form-registro`).
4. **Avances:** `git commit -m "feat: implementado formulario de registro"` y `git push origin feature/nombre-modulo`.
5. **Integración:** abrir un Pull Request hacia `main`. La Persona 4 revisa y realiza el Merge.

Antes de empezar a trabajar, actualizar `main`: `git checkout main && git pull origin main`.
