using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseDeDatosGP
{

    public class Cliente
    {
        public int ID_Cliente { get; set; }
        public string Nombre { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
    }
    public class BDCliente
    {

        public  string MensajeError { get;  set; } = string.Empty;

        public  void CapturarError(Exception ex)
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

        public  void LimpiarError()
        {
            MensajeError = string.Empty;
        }

        // Método para abrir la conexión con las credenciales predeterminadas
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

        // Método para dar de alta a un cliente
        public bool InsertarCliente(string nombre, string telefono, string direccion)
        {
            ConexionInisio con = new ConexionInisio();
            if (con.AbrirConexion())
            {
                try
                {
                    // Consulta SQL para insertar un nuevo cliente
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    INSERT INTO Cliente (Nombre, Telefono, Direccion) \r\n    VALUES (@Nombre, @Telefono, @Direccion);\r\n\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    ROLLBACK TRANSACTION;\r\n    THROW; -- Opcional: Relanza el error para que sea manejado por el cliente\r\nEND CATCH;";

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
                            Console.WriteLine("Cliente insertado con éxito.");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("No se pudo insertar el cliente.");
                            return false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error al insertar el cliente: " + ex.Message);
                    CapturarError(ex);
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
        public List<Cliente> ConsultarClientes(string input)
        {
            List<Cliente> clientes = new List<Cliente>();
            ConexionInisio con = new ConexionInisio();
            if (con.AbrirConexion())
            {
                try
                {
                    string query = "SELECT ID_Cliente, Nombre, Telefono, Direccion FROM Cliente WHERE ";

                    // Verificar si el input es un número (ID)
                    if (int.TryParse(input, out int idCliente))
                    {
                        query += "ID_Cliente = @ID_Cliente"; // Buscar por ID
                    }
                    else
                    {
                        query += "Nombre LIKE @Nombre"; // Buscar por Nombre
                    }

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        if (int.TryParse(input, out idCliente))
                        {
                            comando.Parameters.AddWithValue("@ID_Cliente", idCliente); // Añadir parámetro para ID
                        }
                        else
                        {
                            comando.Parameters.AddWithValue("@Nombre", "%" + input + "%"); // Añadir parámetro para nombre
                        }

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                Cliente cliente = new Cliente
                                {
                                    ID_Cliente = lector.GetInt32(0),
                                    Nombre = lector.GetString(1),
                                    Telefono = lector.IsDBNull(2) ? "Sin teléfono" : lector.GetString(2),
                                    Direccion = lector.IsDBNull(3) ? "Sin dirección" : lector.GetString(3)
                                };

                                clientes.Add(cliente);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error al consultar los clientes: " + ex.Message);
                    CapturarError(ex);
                }
                finally
                {
                    con.CerrarConexion();
                }
            }
            return clientes;
        }

        public bool ModificarCliente(Cliente cliente)
        {
            ConexionInisio con = new ConexionInisio();
            if (con.AbrirConexion())
            {
                try
                {
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    UPDATE Cliente\r\n    SET Nombre = @Nombre, Telefono = @Telefono, Direccion = @Direccion\r\n    WHERE ID_Cliente = @ID_Cliente;\r\n\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    ROLLBACK TRANSACTION;\r\n    THROW; -- Opcional: Relanza el error para que sea manejado por el cliente\r\nEND CATCH;";

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@ID_Cliente", cliente.ID_Cliente);
                        comando.Parameters.AddWithValue("@Nombre", cliente.Nombre);
                        comando.Parameters.AddWithValue("@Telefono", cliente.Telefono);
                        comando.Parameters.AddWithValue("@Direccion", cliente.Direccion);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        return filasAfectadas > 0; // Retorna true si se actualizó al menos una fila
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error al modificar el cliente: " + ex.Message);
                    CapturarError(ex);
                    return false;
                }
                finally
                {
                    con.CerrarConexion();
                }
            }
            return false;
        }


        public bool EliminarCliente(int idCliente)
        {
            //if (AbrirConexion())
            //{
            //    try
            //    {
            //        // Eliminar por ID o Nombre
            //        string query = "DELETE FROM Cliente WHERE ID_Cliente = @ID_Cliente OR Nombre = @Nombre";

            //        using (SqlCommand comando = new SqlCommand(query, conexion))
            //        {
            //            // Asumimos que si hay un ID, lo usa, si no, busca por Nombre
            //            int id;
            //            if (int.TryParse(input, out id))
            //            {
            //                comando.Parameters.AddWithValue("@ID_Cliente", id);
            //                comando.Parameters.AddWithValue("@Nombre", DBNull.Value); // Para evitar problemas al buscar por nombre
            //            }
            //            else
            //            {
            //                comando.Parameters.AddWithValue("@ID_Cliente", DBNull.Value); // Si no hay ID, no lo usas
            //                comando.Parameters.AddWithValue("@Nombre", input); // Busca por nombre
            //            }

            //            int filasAfectadas = comando.ExecuteNonQuery();
            //            return filasAfectadas > 0; // Retorna true si se eliminó al menos una fila
            //        }
            //    }
            //    catch (SqlException ex)
            //    {
            //        Console.WriteLine("Error al eliminar el cliente: " + ex.Message);
            //        return false;
            //    }
            //    finally
            //    {
            //        CerrarConexion();
            //    }
            //}
            //return false;

            bool resultado = false;
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    DELETE FROM Cliente\r\n    WHERE ID_Cliente = @ID_Cliente;\r\n\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    ROLLBACK TRANSACTION;\r\n    THROW; -- Opcional: Relanza el error para que sea manejado por el cliente\r\nEND CATCH;";

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@ID_Cliente", idCliente);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        resultado = filasAfectadas > 0;
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                // Manejar el error según sea necesario
                Console.WriteLine("Error al eliminar el Cliente: " + ex.Message);
                CapturarError(ex);
            }

            return resultado;
        }


    }
}
