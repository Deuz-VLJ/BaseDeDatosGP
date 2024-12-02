using System;
using System.Collections.Generic;
using System.Data;
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


        //listo
        public bool InsertarProducto(string nombre, decimal precio, string descripcion)
        {
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    // Usar el procedimiento almacenado sp_AltaProducto
                    using (SqlCommand cmd = new SqlCommand("sp_AltaProducto", con.ObtenerConexion()))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Agregar los parámetros
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Precio", precio);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);

                        // Ejecutar el procedimiento almacenado
                        cmd.ExecuteNonQuery();
                        resultado = true;
                    }

                    con.CerrarConexion();
                }
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al insertar producto: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error general: " + ex.Message);
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


        //listo
        public bool ModificarProducto(int idProducto, string nombre, decimal precio, string descripcion)
        {
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    // Usar el procedimiento almacenado sp_ActualizarProducto
                    using (SqlCommand cmd = new SqlCommand("sp_ActualizarProducto", con.ObtenerConexion()))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Agregar los parámetros
                        cmd.Parameters.AddWithValue("@ID_Producto", idProducto);
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Precio", precio);
                        cmd.Parameters.AddWithValue("@Descripcion", descripcion);

                        // Ejecutar el procedimiento almacenado
                        cmd.ExecuteNonQuery();
                        resultado = true;
                    }

                    con.CerrarConexion();
                }
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al modificar producto: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error general: " + ex.Message);
            }

            return resultado;
        }


       //listo
        public bool EliminarProducto(int idProducto)
        {
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    // Usar el procedimiento almacenado sp_BajaProducto
                    using (SqlCommand comando = new SqlCommand("sp_BajaProducto", con.ObtenerConexion()))
                    {
                        comando.CommandType = CommandType.StoredProcedure;

                        // Agregar el parámetro necesario
                        comando.Parameters.AddWithValue("@ID_Producto", idProducto);

                        // Ejecutar el procedimiento almacenado
                        int filasAfectadas = comando.ExecuteNonQuery();
                        resultado = filasAfectadas > 0;
                    }

                    con.CerrarConexion();
                }
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al eliminar el producto: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error general: " + ex.Message);
            }

            return resultado;
        }




    }
}
