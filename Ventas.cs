using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseDeDatosGP
{
    public class Ventas
    {
        private SqlConnection conexion;


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
        // Constructor
        public Ventas()
        {
            // Inicializar la conexión con los parámetros por defecto
            string servidor = "localhost";
            string baseDeDatos = "GestionProductos";
            string usuario = "sa";
            string contrasena = "J17u20a04n7";
            string cadenaConexion = $"Server={servidor};Database={baseDeDatos};User Id={usuario};Password={contrasena};";
            conexion = new SqlConnection(cadenaConexion);
        }

        //// Método para abrir la conexión
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

        // Método para obtener todos los clientes
        public List<Cliente> ObtenerClientes()
        {
            List<Cliente> clientes = new List<Cliente>();
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    string query = "SELECT ID_Cliente, Nombre FROM Cliente";
                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                Cliente cliente = new Cliente
                                {
                                    ID_Cliente = lector.GetInt32(0), // ID_Cliente
                                    Nombre = lector.GetString(1) // Nombre
                                };
                                clientes.Add(cliente);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al obtener clientes: " + ex.Message);
            }
            finally
            {
                con.CerrarConexion();
            }

            return clientes;
        }



        // Método para obtener todos los productos
        public List<Producto> ObtenerProductos()
        {
            List<Producto> productos = new List<Producto>();
            ConexionInisio con = new ConexionInisio();
            try
            {
                if (con.AbrirConexion())
                {
                    string query = "SELECT ID_Producto, Nombre, Precio, Descripcion FROM Producto";
                    using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                Producto producto = new Producto
                                {
                                    ID_Producto = lector.GetInt32(0), // ID_Producto
                                    Nombre = lector.GetString(1), // Nombre
                                    Precio = lector.GetDecimal(2), // Precio
                                    Descripcion = lector.GetString(3) // Descripción
                                };
                                productos.Add(producto);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al obtener productos: " + ex.Message);
            }
            finally
            {
                con.CerrarConexion();
            }

            return productos;
        }

        public decimal ObtenerPrecioProducto(string nombreProducto)
        {
            decimal precio = 0;
            ConexionInisio con = new ConexionInisio();
            // Abrir la conexión utilizando el método definido previamente
            if (con.AbrirConexion())
            {
                string query = "SELECT Precio FROM Producto WHERE Nombre = @Nombre";
                using (SqlCommand comando = new SqlCommand(query, con.ObtenerConexion()))
                {
                    comando.Parameters.AddWithValue("@Nombre", nombreProducto);
                    SqlDataReader reader = comando.ExecuteReader();
                    if (reader.Read())
                    {
                        precio = (decimal)reader["Precio"];
                    }
                }
                con.CerrarConexion(); // Asegúrate de cerrar la conexión después de la consulta
            }
            return precio;
        }



        // Método para insertar una nueva venta y sus detalles
        /*   public bool InsertarVenta(DateTime fecha, decimal importe, decimal iva, decimal total, string metodoPago, int idCliente, List<DetalleVenta> detalles)
           {
               // Abrir la conexión
               if (!AbrirConexion())
               {
                   return false;
               }

               SqlTransaction transaction = null;

               try
               {
                   // Iniciar la transacción
                   transaction = conexion.BeginTransaction();

                   // Insertar la venta
                   string queryVenta = "INSERT INTO Venta (Fecha, Importe, Iva, Total, Metodo_Pago, ID_Cliente) " +
                                       "VALUES (@Fecha, @Importe, @Iva, @Total, @Metodo_Pago, @ID_Cliente); " +
                                       "SELECT SCOPE_IDENTITY();"; // Para obtener el ID de la venta generada

                   SqlCommand comandoVenta = new SqlCommand(queryVenta, conexion, transaction);
                   comandoVenta.Parameters.AddWithValue("@Fecha", fecha);
                   comandoVenta.Parameters.AddWithValue("@Importe", importe);
                   comandoVenta.Parameters.AddWithValue("@Iva", iva);
                   comandoVenta.Parameters.AddWithValue("@Total", total);
                   comandoVenta.Parameters.AddWithValue("@Metodo_Pago", metodoPago);
                   comandoVenta.Parameters.AddWithValue("@ID_Cliente", idCliente);

                   // Ejecutar y obtener el ID de la nueva venta
                   int idVenta = Convert.ToInt32(comandoVenta.ExecuteScalar());

                   // Insertar los detalles de la venta
                   foreach (var detalle in detalles)
                   {
                       string queryDetalle = "INSERT INTO DetalleVenta (ID_Venta, ID_Producto, Cantidad, Precio_Unitario, Subtotal) " +
                                             "VALUES (@ID_Venta, @ID_Producto, @Cantidad, @Precio_Unitario, @Subtotal)";

                       SqlCommand comandoDetalle = new SqlCommand(queryDetalle, conexion, transaction);
                       comandoDetalle.Parameters.AddWithValue("@ID_Venta", idVenta);
                       comandoDetalle.Parameters.AddWithValue("@ID_Producto", detalle.ID_Producto);
                       comandoDetalle.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                       comandoDetalle.Parameters.AddWithValue("@Precio_Unitario", detalle.Precio_Unitario);
                       comandoDetalle.Parameters.AddWithValue("@Subtotal", detalle.Subtotal);

                       comandoDetalle.ExecuteNonQuery();
                   }

                   // Confirmar la transacción
                   transaction.Commit();

                   return true;
               }
               catch (SqlException ex)
               {
                   Console.WriteLine("Error al insertar la venta: " + ex.Message);
                   if (transaction != null)
                   {
                       transaction.Rollback();
                   }
                   return false;
               }
               finally
               {
                   CerrarConexion();
               }
           }*/
        public bool InsertarVenta(DateTime fecha, decimal importe, decimal iva, decimal total, string metodoPago, int idCliente, List<DetalleVenta> detalles)
        {
            ConexionInisio con = new ConexionInisio();
           
            // Abrir la conexión
            if (!con.AbrirConexion())
            {
                return false;
            }
            conexion = con.ObtenerConexion();
            SqlTransaction transaction = null;

            try
            {
                // Iniciar la transacción
                transaction = conexion.BeginTransaction();

                // Insertar la venta
                string queryVenta = "INSERT INTO Venta (Fecha, Importe, Iva, Total, Metodo_Pago, ID_Cliente) " +
                                    "VALUES (@Fecha, @Importe, @Iva, @Total, @Metodo_Pago, @ID_Cliente); " +
                                    "SELECT SCOPE_IDENTITY();"; // Para obtener el ID de la venta generada

                SqlCommand comandoVenta = new SqlCommand(queryVenta, conexion, transaction);
                comandoVenta.Parameters.AddWithValue("@Fecha", fecha);
                comandoVenta.Parameters.AddWithValue("@Importe", importe);
                comandoVenta.Parameters.AddWithValue("@Iva", iva);
                comandoVenta.Parameters.AddWithValue("@Total", total);
                comandoVenta.Parameters.AddWithValue("@Metodo_Pago", metodoPago);
                comandoVenta.Parameters.AddWithValue("@ID_Cliente", idCliente);

                // Ejecutar y obtener el ID de la nueva venta
                int idVenta = Convert.ToInt32(comandoVenta.ExecuteScalar());

                // Verificar el stock antes de procesar cada detalle de la venta
                foreach (var detalle in detalles)
                {
                    // Consultar la cantidad disponible del producto en la tabla Saldos
                    string queryConsultarStock = "SELECT Cantidad_Disponible FROM Saldos WHERE ID_Producto = @ID_Producto";

                    SqlCommand comandoConsultarStock = new SqlCommand(queryConsultarStock, conexion, transaction);
                    comandoConsultarStock.Parameters.AddWithValue("@ID_Producto", detalle.ID_Producto);

                    int cantidadDisponible = Convert.ToInt32(comandoConsultarStock.ExecuteScalar());

                    // Verificar si hay suficiente stock para la venta
                    if (cantidadDisponible < detalle.Cantidad)
                    {
                        // Si no hay suficiente stock, cancelar la venta y mostrar un mensaje
                        transaction.Rollback();
                        Console.WriteLine($"No hay suficiente stock del producto con ID: {detalle.ID_Producto}");
                        return false;
                    }

                    // Insertar el detalle de la venta
                    string queryDetalle = "INSERT INTO DetalleVenta (ID_Venta, ID_Producto, Cantidad, Precio_Unitario, Subtotal) " +
                                          "VALUES (@ID_Venta, @ID_Producto, @Cantidad, @Precio_Unitario, @Subtotal)";

                    SqlCommand comandoDetalle = new SqlCommand(queryDetalle, conexion, transaction);
                    comandoDetalle.Parameters.AddWithValue("@ID_Venta", idVenta);
                    comandoDetalle.Parameters.AddWithValue("@ID_Producto", detalle.ID_Producto);
                    comandoDetalle.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                    comandoDetalle.Parameters.AddWithValue("@Precio_Unitario", detalle.Precio_Unitario);
                    comandoDetalle.Parameters.AddWithValue("@Subtotal", detalle.Subtotal);

                    comandoDetalle.ExecuteNonQuery();

                    // Actualizar la cantidad salida en la tabla Saldos
                    string queryActualizarSaldo = "UPDATE Saldos SET Cantidad_Salida = Cantidad_Salida + @Cantidad " +
                                                  "WHERE ID_Producto = @ID_Producto";

                    SqlCommand comandoActualizarSaldo = new SqlCommand(queryActualizarSaldo, conexion, transaction);
                    comandoActualizarSaldo.Parameters.AddWithValue("@Cantidad", detalle.Cantidad);
                    comandoActualizarSaldo.Parameters.AddWithValue("@ID_Producto", detalle.ID_Producto);

                    comandoActualizarSaldo.ExecuteNonQuery();
                }

                // Confirmar la transacción
                transaction.Commit();

                return true;
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                Console.WriteLine("Error al insertar la venta: " + ex.Message);
                if (transaction != null)
                {
                    transaction.Rollback();
                }
                return false;
            }
            finally
            {
                con.CerrarConexion();
            }
        }


        // Método para obtener el ID del producto basado en su nombre
        public int ObtenerIDProductoPorNombre(string nombreProducto)
        {
            int idProducto = -1;
            ConexionInisio con = new ConexionInisio();
            // Abrir la conexión
            if (con.AbrirConexion())
            {
                try
                {
                    string query = "SELECT ID_Producto FROM Producto WHERE Nombre = @NombreProducto";
                    SqlCommand comando = new SqlCommand(query, con.ObtenerConexion());
                    comando.Parameters.AddWithValue("@NombreProducto", nombreProducto);

                    SqlDataReader reader = comando.ExecuteReader();

                    if (reader.Read())
                    {
                        idProducto = Convert.ToInt32(reader["ID_Producto"]);
                    }

                    reader.Close();
                }
                catch (SqlException ex)
                {
                    CapturarError (ex);
                    Console.WriteLine("Error al obtener el ID del producto: " + ex.Message);
                }
                finally
                {
                    con.CerrarConexion();
                }
            }

            return idProducto;
        }


        public List<VentaDetalleCliente> ObtenerComprasPorCliente(string nombreCliente)
        {
            List<VentaDetalleCliente> compras = new List<VentaDetalleCliente>();
            ConexionInisio con = new ConexionInisio();

            string consulta = @"
        SELECT v.ID_Venta, v.Fecha, v.Importe, v.Iva, v.Total, p.Nombre AS Producto, dv.Cantidad, dv.Precio_Unitario, dv.Subtotal
        FROM Venta v
        INNER JOIN DetalleVenta dv ON v.ID_Venta = dv.ID_Venta
        INNER JOIN Producto p ON dv.ID_Producto = p.ID_Producto
        INNER JOIN Cliente c ON v.ID_Cliente = c.ID_Cliente
        WHERE c.Nombre = @NombreCliente";

            // Abrir la conexión
            if (con.AbrirConexion())
            {
                try
                {
                    using (SqlCommand comando = new SqlCommand(consulta, con.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@NombreCliente", nombreCliente);

                        using (SqlDataReader lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                VentaDetalleCliente compra = new VentaDetalleCliente
                                {
                                    ID_Venta = lector.GetInt32(0),
                                    Fecha = lector.GetDateTime(1),
                                    Importe = lector.GetDecimal(2),
                                    Iva = lector.GetDecimal(3),
                                    Total = lector.GetDecimal(4),
                                    Producto = lector.GetString(5),
                                    Cantidad = lector.GetInt32(6),
                                    PrecioUnitario = lector.GetDecimal(7),
                                    Subtotal = lector.GetDecimal(8)
                                };
                                compras.Add(compra);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    CapturarError(ex);
                    Console.WriteLine("Error al obtener las compras del cliente: " + ex.Message);
                }
                finally
                {
                    // Cerrar la conexión
                    con.CerrarConexion();
                }
            }

            return compras;
        }



    }

    public class VentaDetalleCliente
    {
        public int ID_Venta { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Importe { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }


    // Clase para representar los detalles de una venta
    public class DetalleVenta
    {
        public int ID_Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio_Unitario { get; set; }
        public decimal Subtotal { get; set; }
    }



}
