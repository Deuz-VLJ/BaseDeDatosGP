using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseDeDatosGP
{

    public class Producto 
    {
        public int ID_Producto { get; set; }      // Clave primaria, autoincremental
        public string Nombre { get; set; }         // Nombre del producto
        public decimal Precio { get; set; }        // Precio del producto
        public string Descripcion { get; set; }
    }

    public class BDProductos
    {
        //public bool AbrirConexion()
        //{
        //    try
        //    {
        //        string cadenaConexion = "Server=localhost;Database=GestionProductos;User Id=sa;Password=J17u20a04n7;";
        //        conexion = new SqlConnection(cadenaConexion);
        //        conexion.Open();
        //        return true; // Conexión abierta con éxito
        //    }
        //    catch (SqlException ex)
        //    {
        //        Console.WriteLine("Error al abrir la conexión: " + ex.Message);
        //        return false; // Error al abrir la conexión
        //    }
        //}

        //public void CerrarConexion()
        //{
        //    if (conexion != null && conexion.State == System.Data.ConnectionState.Open)
        //    {
        //        conexion.Close();
        //        Console.WriteLine("Conexión cerrada con éxito.");
        //    }
        //}

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



        public bool InsertarProducto(string nombre, decimal precio, string descripcion)
        {
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    -- Intentar realizar el INSERT\r\n    INSERT INTO Producto (Nombre, Precio, Descripcion) \r\n    VALUES (@Nombre, @Precio, @Descripcion);\r\n\r\n    -- Confirmar la transacción\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    -- Revertir la transacción si ocurre un error\r\n    ROLLBACK TRANSACTION;\r\n\r\n    -- Relanzar el error para manejo adicional (opcional)\r\n    THROW;\r\nEND CATCH;";
                    using (SqlCommand cmd = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Precio", precio);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                        cmd.ExecuteNonQuery();
                        resultado = true;
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al insertar producto: " + ex.Message);
            }

            return resultado;
        }



        public List<Producto> ConsultarProductos(string textoBusqueda)
        {
            List<Producto> listaProductos = new List<Producto>();
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    // Consulta SQL que busca productos por ID, Nombre o Descripción
                    string query = @"SELECT ID_Producto, Nombre, Precio, Descripcion 
                             FROM Producto
                             WHERE CAST(ID_Producto AS NVARCHAR) LIKE @Busqueda
                             OR Nombre LIKE @Busqueda
                             OR Descripcion LIKE @Busqueda";

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@Busqueda", "%" + textoBusqueda + "%");

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                Producto producto = new Producto
                                {
                                    ID_Producto = lector.GetInt32(0),
                                    Nombre = lector.GetString(1),
                                    Precio = lector.GetDecimal(2),
                                    Descripcion = lector.GetString(3)
                                };

                                listaProductos.Add(producto); // Agregar el producto a la lista
                            }
                        }
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                CapturarError(ex);
               Console.WriteLine("Error al consultar productos: " + ex.Message);
            }

            return listaProductos;
        }



        public bool ModificarProducto(int idProducto, string nombre, decimal precio, string descripcion)
        {
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();  
            try
            {
                if (con.AbrirConexion())
                {
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    -- Intentar realizar el UPDATE\r\n    UPDATE Producto \r\n    SET Nombre = @Nombre, Precio = @Precio, Descripcion = @Descripcion \r\n    WHERE ID_Producto = @ID_Producto;\r\n\r\n    -- Confirmar la transacción\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    -- Revertir la transacción si ocurre un error\r\n    ROLLBACK TRANSACTION;\r\n\r\n    -- Relanzar el error para manejo adicional (opcional)\r\n    THROW;\r\nEND CATCH;";
                    using (SqlCommand cmd = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        cmd.Parameters.AddWithValue("@ID_Producto", idProducto);
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Precio", precio);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                        cmd.ExecuteNonQuery();
                        resultado = true;
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                CapturarError (ex);
                Console.WriteLine("Error al modificar producto: " + ex.Message);
            }

            return resultado;
        }



        public bool EliminarProducto(int idProducto)
        {
            //try
            //{
            //    using (SqlConnection conexion = new SqlConnection("Server=localhost;Database=GestionProductos;User Id=sa;Password=J17u20a04n7;"))
            //    {
            //        conexion.Open();
            //        string query;
            //        if (int.TryParse(nombreOId, out int id))
            //        {
            //            query = "DELETE FROM Producto WHERE ID_Producto = @ID_Producto";
            //        }
            //        else
            //        {
            //            query = "DELETE FROM Producto WHERE Nombre = @Nombre";
            //        }

            //        SqlCommand cmd = new SqlCommand(query, conexion);
            //        if (int.TryParse(nombreOId, out id))
            //        {
            //            cmd.Parameters.AddWithValue("@ID_Producto", id);
            //        }
            //        else
            //        {
            //            cmd.Parameters.AddWithValue("@Nombre", nombreOId);
            //        }

            //        cmd.ExecuteNonQuery();
            //        return true;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Error al eliminar producto: " + ex.Message);
            //    return false;
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    string query = "BEGIN TRANSACTION;\r\n\r\nBEGIN TRY\r\n    -- Intentar realizar el DELETE\r\n    DELETE FROM Producto \r\n    WHERE ID_Producto = @ID_Producto;\r\n\r\n    -- Confirmar la transacción\r\n    COMMIT TRANSACTION;\r\nEND TRY\r\nBEGIN CATCH\r\n    -- Revertir la transacción si ocurre un error\r\n    ROLLBACK TRANSACTION;\r\n\r\n    -- Relanzar el error para manejo adicional (opcional)\r\n    THROW;\r\nEND CATCH;";

                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@ID_Producto", idProducto);

                        int filasAfectadas = comando.ExecuteNonQuery();
                        resultado = filasAfectadas > 0;
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                // Manejar el error según sea necesario
                CapturarError(ex);
                Console.WriteLine("Error al eliminar el producto: " + ex.Message);
            }

            return resultado;
        }
        



    }
}
