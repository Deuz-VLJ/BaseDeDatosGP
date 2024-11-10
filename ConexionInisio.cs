using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseDeDatosGP
{
    public class ConexionInisio
    {
        public string MensajeError { get; set; } = string.Empty;

        public void CapturarError(Exception ex)
        {
            if (ex is SqlException sqlEx)
            {
                MensajeError = $"Error en SQL Server: {sqlEx.Message}\nCódigo de error: {sqlEx.Number}";
            }
            else
            {
                MensajeError = $"Error general: {ex.Message}";
            }
        }

        public void LimpiarError()
        {
            MensajeError = string.Empty;
        }


        //private SqlConnection conexion;

        //// Método para abrir la conexión y devolver true si la conexión es exitosa, false en caso contrario.
        //public bool AbrirConexion(string usuario, string contrasena)
        //{
        //    try
        //    {
        //        string servidor = "localhost";
        //        string baseDeDatos = "GestionProductos";
        //        string cadenaConexion = $"Server={servidor};Database={baseDeDatos};User Id={usuario};Password={contrasena};";
        //        conexion = new SqlConnection(cadenaConexion);
        //        conexion.Open();  // Intenta abrir la conexión
        //        return true;      // Si la conexión se abrió correctamente, devuelve true
        //    }
        //    catch (SqlException ex)
        //    {
        //        Console.WriteLine("Error al conectar a la base de datos: " + ex.Message);
        //        return false;     // Si hay un error, devuelve false
        //    }
        //}

        //// Método para cerrar la conexión y devolver true si se cerró correctamente, false si ya estaba cerrada o falló.
        //public bool CerrarConexion()
        //{
        //    if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
        //    {
        //        conexion.Close();  // Cierra la conexión
        //        Console.WriteLine("Conexión cerrada con éxito.");
        //        return true;       // Si se cerró correctamente, devuelve true
        //    }
        //    else
        //    {
        //        Console.WriteLine("La conexión ya está cerrada o no se pudo cerrar.");
        //        return false;      // Si la conexión no estaba abierta o hubo un problema, devuelve false
        //    }
        //}

        private SqlConnection conexion;
        // Variables estáticas para almacenar el usuario y la contraseña al iniciar sesión
        private static string usuario;
        private static string contrasena;

        // Método para configurar el usuario y la contraseña al iniciar sesión
        public static void ConfigurarCredenciales(string user, string pass)
        {
            usuario = user;
            contrasena = pass;
        }
        public ConexionInisio()
        {
            // Configurar la conexión con los parámetros predeterminados y las credenciales del login
            string servidor = "localhost";
            string baseDeDatos = "GestionProductos";
            string cadenaConexion = $"Server={servidor};Database={baseDeDatos};User Id={usuario};Password={contrasena};";
            conexion = new SqlConnection(cadenaConexion);
        }



        // Método para abrir la conexión y devolver true si la conexión es exitosa, false en caso contrario.
        public bool AbrirConexion()
        {
            try
            {
                string servidor = "localhost";
                string baseDeDatos = "GestionProductos";
                string cadenaConexion = $"Server={servidor};Database={baseDeDatos};User Id={usuario};Password={contrasena};";
                conexion = new SqlConnection(cadenaConexion);
                conexion.Open();  // Intenta abrir la conexión
                return true;      // Si la conexión se abrió correctamente, devuelve true
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al conectar a la base de datos: " + ex.Message);
                return false;     // Si hay un error, devuelve false
            }
        }

        // Método para cerrar la conexión y devolver true si se cerró correctamente, false si ya estaba cerrada o falló.
        public bool CerrarConexion()
        {
            if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
            {
                conexion.Close();  // Cierra la conexión
                Console.WriteLine("Conexión cerrada con éxito.");
                return true;       // Si se cerró correctamente, devuelve true
            }
            else
            {
                Console.WriteLine("La conexión ya está cerrada o no se pudo cerrar.");
                return false;      // Si la conexión no estaba abierta o hubo un problema, devuelve false
            }
        }

        // Método para obtener la conexión actual
        public SqlConnection ObtenerConexion()
        {
            // Verifica si la conexión está abierta antes de devolverla
            if (conexion == null || conexion.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("La conexión no está abierta. Asegúrese de llamar a AbrirConexion() primero.");
            }
            return conexion;
        }



    }
}