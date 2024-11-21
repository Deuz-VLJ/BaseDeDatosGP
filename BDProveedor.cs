using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseDeDatosGP
{

    public class Proveedor
    {
        public int ID_Proveedor { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
    }
    public class BDProveedor
    {

        private SqlConnection conexion;

        //// Método para abrir la conexión con las credenciales predeterminadas
        //public bool AbrirConexion()
        //{
        //    string servidor = "localhost";
        //    string baseDeDatos = "GestionProductos";
        //    string usuario = "sa";
        //    string contrasena = "J17u20a04n7";

        //    try
        //    {
        //        string cadenaConexion = $"Server={servidor};Database={baseDeDatos};User Id={usuario};Password={contrasena};";
        //        conexion = new SqlConnection(cadenaConexion);
        //        conexion.Open();
        //        Console.WriteLine("Conexión abierta con éxito.");
        //        return true;
        //    }
        //    catch (SqlException ex)
        //    {
        //        Console.WriteLine("Error al conectar a la base de datos: " + ex.Message);
        //        return false;
        //    }
        //}

        //// Método para cerrar la conexión
        //public void CerrarConexion()
        //{
        //    if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
        //    {
        //        conexion.Close();
        //        Console.WriteLine("Conexión cerrada con éxito.");
        //    }
        //}

        // Método para dar de alta a un Proveedor

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


        public bool InsertarProveedor(string nombre, string telefono, string direccion)
        {
            ConexionInisio con = new ConexionInisio();
            if (con.AbrirConexion())
            {
                try
                {
                    // Consulta SQL para insertar un nuevo Proveedor
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    -- Intentar realizar el INSERT\r\n    INSERT INTO Proveedor (Nombre, Telefono, Direccion) \r\n    VALUES (@Nombre, @Telefono, @Direccion);\r\n\r\n    -- Confirmar la transacción\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    -- Revertir la transacción si ocurre un error\r\n    ROLLBACK TRANSACTION;\r\n\r\n    -- Relanzar el error para manejo adicional (opcional)\r\n    THROW;\r\nEND CATCH;";

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        // Asignar los valores de los parámetros
                        comando.Parameters.AddWithValue("@Nombre", nombre);
                        comando.Parameters.AddWithValue("@Telefono", telefono);
                        comando.Parameters.AddWithValue("@Direccion", direccion);

                        // Ejecutar el comando
                        int filasAfectadas = comando.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            Console.WriteLine("Proveedor insertado con éxito.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("No se pudo insertar el Proveedor.");
                            return false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    CapturarError(ex);
                    Console.WriteLine("Error al insertar el Proveedor: " + ex.Message);
                    return false;
                }
                finally
                {
                    con.CerrarConexion();
                }
            }
            return false;
        }

        // Método para consultar los clientes por nombre o ID
        public List<Proveedor> ConsultarProveedor(string input)
        {
            List<Proveedor> listaProveedores = new List<Proveedor>();
            ConexionInisio con = new ConexionInisio();
            if (con.AbrirConexion())
            {
                try
                {
                    // Consulta para buscar por ID o Nombre
                    string query = "SELECT ID_Proveedor, Nombre, Telefono, Direccion FROM Proveedor WHERE ID_Proveedor = @ID OR Nombre LIKE @Nombre";

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        // Verificar si el input es un número (ID) o un nombre
                        int id;
                        if (int.TryParse(input, out id))
                        {
                            comando.Parameters.AddWithValue("@ID", id);
                            comando.Parameters.AddWithValue("@Nombre", DBNull.Value);
                        }
                        else
                        {
                            comando.Parameters.AddWithValue("@ID", DBNull.Value);
                            comando.Parameters.AddWithValue("@Nombre", "%" + input + "%");
                        }

                        using (SqlDataReader reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Proveedor proveedor = new Proveedor
                                {
                                    ID_Proveedor = reader.GetInt32(0),
                                    Nombre = reader.GetString(1),
                                    Telefono = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    Direccion = reader.IsDBNull(3) ? null : reader.GetString(3)
                                };
                                listaProveedores.Add(proveedor);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    CapturarError(ex);
                    Console.WriteLine("Error al consultar proveedores: " + ex.Message);
                }
                finally
                {
                    con.CerrarConexion();
                }
            }

            return listaProveedores;
        }

        public bool ModificarProveedor(Proveedor proveedor)
        {
            ConexionInisio con = new ConexionInisio();
            if (con.AbrirConexion())
            {
                try
                {
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    -- Intentar realizar el UPDATE\r\n    UPDATE Proveedor \r\n    SET Nombre = @Nombre, Telefono = @Telefono, Direccion = @Direccion \r\n    WHERE ID_Proveedor = @ID_Proveedor;\r\n\r\n    -- Confirmar la transacción\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    -- Revertir la transacción si ocurre un error\r\n    ROLLBACK TRANSACTION;\r\n\r\n    -- Relanzar el error para manejo adicional (opcional)\r\n    THROW;\r\nEND CATCH;";

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@ID_Proveedor", proveedor.ID_Proveedor);
                        comando.Parameters.AddWithValue("@Nombre", proveedor.Nombre);
                        comando.Parameters.AddWithValue("@Telefono", proveedor.Telefono);
                        comando.Parameters.AddWithValue("@Direccion", proveedor.Direccion);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        return filasAfectadas > 0; // Retorna true si se actualizó al menos una fila
                    }
                }
                catch (SqlException ex)
                {
                    CapturarError(ex);
                    Console.WriteLine("Error al modificar el Proveedor: " + ex.Message);
                    return false;
                }
                finally
                {
                    con.CerrarConexion();
                }
            }
            return false;
        }


        //public bool EliminarProveedor(string input)
        //{
        //    if (AbrirConexion())
        //    {
        //        try
        //        {
        //            // Eliminar por ID o Nombre
        //            string query = "DELETE FROM Proveedor WHERE ID_Proveedor = @ID_Proveedor OR Nombre = @Nombre";

        //            using (SqlCommand comando = new SqlCommand(query, conexion))
        //            {
        //                // Asumimos que si hay un ID, lo usa, si no, busca por Nombre
        //                int id;
        //                if (int.TryParse(input, out id))
        //                {
        //                    comando.Parameters.AddWithValue("@ID_Proveedor", id);
        //                    comando.Parameters.AddWithValue("@Nombre", DBNull.Value); // Para evitar problemas al buscar por nombre
        //                }
        //                else
        //                {
        //                    comando.Parameters.AddWithValue("@ID_Proveedor", DBNull.Value); // Si no hay ID, no lo usas
        //                    comando.Parameters.AddWithValue("@Nombre", input); // Busca por nombre
        //                }

        //                int filasAfectadas = comando.ExecuteNonQuery();
        //                return filasAfectadas > 0; // Retorna true si se eliminó al menos una fila
        //            }
        //        }
        //        catch (SqlException ex)
        //        {
        //            Console.WriteLine("Error al eliminar el Proveedor: " + ex.Message);
        //            return false;
        //        }
        //        finally
        //        {
        //            CerrarConexion();
        //        }
        //    }
        //    return false;
        //}

        public bool EliminarProveedor(int idProveedor)
        {
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    -- Intentar realizar el DELETE\r\n    DELETE FROM Proveedor \r\n    WHERE ID_Proveedor = @ID_Proveedor;\r\n\r\n    -- Confirmar la transacción\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    -- Revertir la transacción si ocurre un error\r\n    ROLLBACK TRANSACTION;\r\n\r\n    -- Relanzar el error para manejo adicional (opcional)\r\n    THROW;\r\nEND CATCH;";

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@ID_Proveedor", idProveedor);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        resultado = filasAfectadas > 0;
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                CapturarError (ex);
                Console.WriteLine("Error al eliminar el proveedor: " + ex.Message);
            }

            return resultado;
        }

    }
}
