using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseDeDatosGP
{
    public class Reportes
    {
        private SqlConnection conexion;

        public Reportes()
        {
            // Inicializar la conexión con los parámetros por defecto
            //string servidor = "localhost";
            //string baseDeDatos = "GestionProductos";
            //string usuario = "sa";
            //string contrasena = "J17u20a04n7";
            //string cadenaConexion = $"Server={servidor};Database={baseDeDatos};User Id={usuario};Password={contrasena};";
            //conexion = new SqlConnection(cadenaConexion);
        }
        // Método para abrir la conexión
        //public bool AbrirConexion()
        //{
        //    try
        //    {
        //        if (conexion.State == System.Data.ConnectionState.Closed)
        //        {
        //            conexion.Open();
        //            Console.WriteLine("Conexión abierta con éxito.");
        //            return true;
        //        }
        //    }
        //    catch (SqlException ex)
        //    {
        //        Console.WriteLine("Error al conectar a la base de datos: " + ex.Message);
        //        return false;
        //    }
        //    return true;
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

        public List<Cliente> ObtenerClientesPorNombreOID(string filtro)
        {
            List<Cliente> clientes = new List<Cliente>();
            ConexionInisio con = new ConexionInisio();

            // Consulta SQL que permite buscar por nombre o ID
            string consulta = @"
        SELECT ID_Cliente, Nombre, Telefono, Direccion
        FROM Cliente
        WHERE Nombre LIKE @Filtro OR CAST(ID_Cliente AS NVARCHAR) LIKE @Filtro";

            // Abrir la conexión
            if (con.AbrirConexion())
            {
                try
                {
                    using (SqlCommand comando = new SqlCommand(consulta, con.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@Filtro", "%" + filtro + "%");

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                Cliente cliente = new Cliente
                                {
                                    ID_Cliente = lector.GetInt32(0),
                                    Nombre = lector.GetString(1),
                                    Telefono = lector.IsDBNull(2) ? string.Empty : lector.GetString(2),
                                    Direccion = lector.IsDBNull(3) ? string.Empty : lector.GetString(3)
                                };
                                clientes.Add(cliente);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    CapturarError(ex);
                    Console.WriteLine("Error al obtener los clientes: " + ex.Message);
                }
                finally
                {
                    // Cerrar la conexión
                    con.CerrarConexion();
                }
            }

            return clientes;
        }

        public List<Proveedor> ObtenerProveedores(string criterioBusqueda)
        {
            List<Proveedor> proveedores = new List<Proveedor>();
            ConexionInisio con = new ConexionInisio();

            try
            {
                // Abrir la conexión
                if (con.AbrirConexion())
                {
                    // Consulta para buscar por ID o Nombre
                    string query = "SELECT ID_Proveedor, Nombre, Telefono, Direccion FROM Proveedor " +
                                   "WHERE CAST(ID_Proveedor AS NVARCHAR) LIKE @CriterioBusqueda " +
                                   "OR Nombre LIKE @CriterioBusqueda";

                    SqlCommand command = new SqlCommand(query, con.ObtenerConexion());
                    command.Parameters.AddWithValue("@CriterioBusqueda", "%" + criterioBusqueda + "%");

                    SqlDataReader reader = command.ExecuteReader();

                    // Leer los resultados
                    while (reader.Read())
                    {
                        Proveedor proveedor = new Proveedor
                        {
                            ID_Proveedor = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Telefono = reader.GetString(2),
                            Direccion = reader.GetString(3)
                        };
                        proveedores.Add(proveedor);
                    }

                    reader.Close();
                }
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al consultar proveedores: " + ex.Message);
            }
            finally
            {
                // Cerrar la conexión
                con.CerrarConexion();
            }

            return proveedores;
        }

        public List<ProductoStock> ConsultarStockProductos(string filtro)
        {
            List<ProductoStock> productos = new List<ProductoStock>();
            ConexionInisio con = new ConexionInisio();

            // Abrir la conexión
            if (!con.AbrirConexion())
            {
                return productos;
            }

            try
            {
                // Consulta SQL para obtener el nombre de los productos y su stock
                string query = "SELECT p.Nombre, s.Cantidad_Disponible " +
                               "FROM Producto p " +
                               "INNER JOIN Saldos s ON p.ID_Producto = s.ID_Producto " +
                               "WHERE p.Nombre LIKE @Filtro";

                SqlCommand comando = new SqlCommand(query, con.ObtenerConexion());
                comando.Parameters.AddWithValue("@Filtro", "%" + filtro + "%");

                SqlDataReader reader = comando.ExecuteReader();

                while (reader.Read())
                {
                    ProductoStock producto = new ProductoStock
                    {
                        Nombre = reader["Nombre"].ToString(),
                        CantidadDisponible = Convert.ToInt32(reader["Cantidad_Disponible"])
                    };

                    productos.Add(producto);
                }

                reader.Close();
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al consultar los productos: " + ex.Message);
            }
            finally
            {
                con.CerrarConexion();
            }

            return productos;
        }


        public List<Venta> ObtenerVentasPorCliente(string criterio)
        {
            List<Venta> ventas = new List<Venta>();
            ConexionInisio con = new ConexionInisio();

            // Abrir la conexión
            if (!con.AbrirConexion())
            {
                throw new Exception("No se pudo abrir la conexión a la base de datos.");
            }

            // Verificar si el criterio es un número (ID) o texto (nombre)
            string query = @"
        SELECT v.ID_Venta, v.Fecha, v.Importe, v.Iva, v.Total, v.Metodo_Pago, c.Nombre
        FROM Venta v
        INNER JOIN Cliente c ON v.ID_Cliente = c.ID_Cliente
        WHERE c.Nombre LIKE @Criterio OR c.ID_Cliente = @ID_Cliente";

            SqlCommand command = new SqlCommand(query, con.ObtenerConexion());
            command.Parameters.AddWithValue("@Criterio", "%" + criterio + "%");
            command.Parameters.AddWithValue("@ID_Cliente", int.TryParse(criterio, out int id) ? id : (object)DBNull.Value); // Si no es un número, se usa DBNull.Value

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ventas.Add(new Venta
                    {
                        ID_Venta = reader.GetInt32(0),
                        Fecha = reader.GetDateTime(1),
                        Importe = reader.GetDecimal(2),
                        IVA = reader.GetDecimal(3),
                        Total = reader.GetDecimal(4),
                        Metodo_Pago = reader.GetString(5),
                        Nombre_Cliente = reader.GetString(6)
                    });
                }
            }

            return ventas;
        }



        public List<Venta> ObtenerVentasPorRangoDeFechas(DateTime fechaInicio, DateTime fechaFinal)
        {
            List<Venta> ventas = new List<Venta>();
            ConexionInisio con = new ConexionInisio();

            // Abrir la conexión
            if (!con.AbrirConexion())
            {
                throw new Exception("No se pudo abrir la conexión a la base de datos.");
            }

            // Consulta SQL para obtener las ventas en un rango de fechas
            string query = @"
    SELECT v.ID_Venta, v.Fecha, v.Importe, v.Iva, v.Total, v.Metodo_Pago, c.Nombre
    FROM Venta v
    INNER JOIN Cliente c ON v.ID_Cliente = c.ID_Cliente
    WHERE CAST(v.Fecha AS DATE) >= @FechaInicio AND CAST(v.Fecha AS DATE) <= @FechaFinal";

            SqlCommand command = new SqlCommand(query, con.ObtenerConexion());
            command.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date); // Usar .Date para comparar solo la fecha
            command.Parameters.AddWithValue("@FechaFinal", fechaFinal.Date);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ventas.Add(new Venta
                    {
                        ID_Venta = reader.GetInt32(0),
                        Fecha = reader.GetDateTime(1),
                        Importe = reader.GetDecimal(2),
                        IVA = reader.GetDecimal(3),
                        Total = reader.GetDecimal(4),
                        Metodo_Pago = reader.GetString(5),
                        Nombre_Cliente = reader.GetString(6)
                    });
                }
            }

            // Cerrar la conexión después de usarla
            con.CerrarConexion();

            return ventas;
        }

        public List<Inventario> BuscarInventarioPorProveedor(string nombreProveedor)
        {
            List<Inventario> inventarios = new List<Inventario>();
            ConexionInisio con = new ConexionInisio();

            // Abrir la conexión
            if (!con.AbrirConexion())
            {
                return inventarios; // Retorna lista vacía si no se puede abrir la conexión
            }

            try
            {
                // Consulta para obtener el inventario basado en el nombre del proveedor
                string query = @"
            SELECT I.ID_Inventario, I.Fecha_Registro, I.Importe, I.IVA, I.Total, I.ID_Proveedor
            FROM Inventario I
            INNER JOIN Proveedor P ON I.ID_Proveedor = P.ID_Proveedor
            WHERE P.Nombre LIKE @NombreProveedor";

                using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                {
                    comando.Parameters.AddWithValue("@NombreProveedor", "%" + nombreProveedor + "%"); // Usar LIKE para buscar coincidencias

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Inventario inventario = new Inventario
                            {
                                ID_Inventario = reader.GetInt32(0),
                                Fecha_Registro = reader.GetDateTime(1),
                                Importe = reader.GetDecimal(2),
                                IVA = reader.GetDecimal(3),
                                Total = reader.GetDecimal(4),
                                ID_Proveedor = reader.GetInt32(5) // Puedes mapear otros campos si lo necesitas
                            };

                            inventarios.Add(inventario);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al buscar el inventario: " + ex.Message);
            }
            finally
            {
                con.CerrarConexion();
            }

            return inventarios; // Retorna la lista de inventarios encontrados
        }

        public List<Inventario> BuscarInventarioPorRangoDeFechas(DateTime fechaInicio, DateTime fechaFinal)
        {
            List<Inventario> inventarios = new List<Inventario>();
            ConexionInisio con = new ConexionInisio();

            // Abrir la conexión
            if (!con.AbrirConexion())
            {
                return inventarios; // Retorna lista vacía si no se puede abrir la conexión
            }

            try
            {
                // Consulta para obtener el inventario basado en un rango de fechas
                string query = @"
        SELECT ID_Inventario, Fecha_Registro, Importe, IVA, Total, ID_Proveedor
        FROM Inventario
        WHERE Fecha_Registro >= @FechaInicio AND Fecha_Registro <= @FechaFinal";

                using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                {
                    comando.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                    comando.Parameters.AddWithValue("@FechaFinal", fechaFinal);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Inventario inventario = new Inventario
                            {
                                ID_Inventario = reader.GetInt32(0),
                                Fecha_Registro = reader.GetDateTime(1),
                                Importe = reader.GetDecimal(2),
                                IVA = reader.GetDecimal(3),
                                Total = reader.GetDecimal(4),
                                ID_Proveedor = reader.GetInt32(5) // Puedes mapear otros campos si lo necesitas
                            };

                            inventarios.Add(inventario);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al buscar el inventario por rango de fechas: " + ex.Message);
            }
            finally
            {
                con.CerrarConexion();
            }

            return inventarios; // Retorna la lista de inventarios encontrados
        }

    }

    public class ProductoStock
    {
        public string Nombre { get; set; }
        public int CantidadDisponible { get; set; }
    }

    public class Venta
    {
        public int ID_Venta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public decimal IVA { get; set; }
        public decimal Total { get; set; }
        public string Metodo_Pago { get; set; }
        public string Nombre_Cliente { get; set; } // Agregar esta propiedad
    }
}
