using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseDeDatosGP
{
    public class BDInventario
    {
        private SqlConnection conexion;

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

        /*
        public void AgregarProductoAlInventario(DateTime fechaRegistro, string observaciones, decimal importe, decimal iva, decimal total, int idProveedor, int idProducto, int cantidadEntrante, decimal costoUnitario)
        {
            // Abrir la conexión
            ConexionInisio con = new ConexionInisio();

            if (!con.AbrirConexion())
            {
                throw new Exception("No se pudo abrir la conexión a la base de datos.");
            }
            conexion = con.ObtenerConexion();
            // Iniciar una transacción
            using (SqlTransaction transaction = conexion.BeginTransaction())
            {
                try
                {
                    // Insertar en la tabla Inventario
                    string queryInventario = @" BEGIN TRANSACTION; BEGIN TRY
                        INSERT INTO Inventario (Fecha_Registro, Observaciones, Importe, IVA, Total, ID_Proveedor) 
                        VALUES (@Fecha_Registro, @Observaciones, @Importe, @IVA, @Total, @ID_Proveedor);

                        -- Obtener el ID generado
                        DECLARE @NuevoID INT;
                        SET @NuevoID = SCOPE_IDENTITY();

                        COMMIT TRANSACTION;

                        -- Retornar el ID generado
                        SELECT @NuevoID AS NuevoID;
                    END TRY
                    BEGIN CATCH
                        ROLLBACK TRANSACTION;

                        -- Relanzar el error para manejo externo si es necesario
                        THROW;
                    END CATCH;"; // Obtener el ID del nuevo inventario

                    SqlCommand cmdInventario = new SqlCommand(queryInventario, conexion, transaction);
                    cmdInventario.Parameters.AddWithValue("@Fecha_Registro", fechaRegistro);
                    cmdInventario.Parameters.AddWithValue("@Observaciones", observaciones);
                    cmdInventario.Parameters.AddWithValue("@Importe", importe);
                    cmdInventario.Parameters.AddWithValue("@IVA", iva);
                    cmdInventario.Parameters.AddWithValue("@Total", total);
                    cmdInventario.Parameters.AddWithValue("@ID_Proveedor", idProveedor);

                    // Obtener el ID del inventario recién insertado
                    int idInventario = Convert.ToInt32(cmdInventario.ExecuteScalar());

                    // Insertar en la tabla DetalleInventario
                    string queryDetalleInventario = @"BEGIN TRANSACTION;

                        BEGIN TRY
                            INSERT INTO DetalleInventario (ID_Inventario, ID_Producto, Cantidad_Entrante, Costo_Unitario, Subtotal) 
                            VALUES (@ID_Inventario, @ID_Producto, @Cantidad_Entrante, @Costo_Unitario, @Subtotal);

                            COMMIT TRANSACTION;
                        END TRY
                        BEGIN CATCH
                            ROLLBACK TRANSACTION;
                            THROW; -- Opcional: relanza el error
                        END CATCH;";

                    SqlCommand cmdDetalleInventario = new SqlCommand(queryDetalleInventario, conexion, transaction);
                    cmdDetalleInventario.Parameters.AddWithValue("@ID_Inventario", idInventario);
                    cmdDetalleInventario.Parameters.AddWithValue("@ID_Producto", idProducto);
                    cmdDetalleInventario.Parameters.AddWithValue("@Cantidad_Entrante", cantidadEntrante);
                    cmdDetalleInventario.Parameters.AddWithValue("@Costo_Unitario", costoUnitario);
                    cmdDetalleInventario.Parameters.AddWithValue("@Subtotal", costoUnitario * cantidadEntrante); // Calcular subtotal

                    cmdDetalleInventario.ExecuteNonQuery();

                    // Verificar si el producto ya está en la tabla Saldos
                    string queryExisteSaldo = "SELECT COUNT(*) FROM Saldos WHERE ID_Producto = @ID_Producto;";
                    SqlCommand cmdExisteSaldo = new SqlCommand(queryExisteSaldo, conexion, transaction);
                    cmdExisteSaldo.Parameters.AddWithValue("@ID_Producto", idProducto);
                    int existeSaldo = Convert.ToInt32(cmdExisteSaldo.ExecuteScalar());

                    if (existeSaldo > 0)
                    {
                        // Si el producto ya está en Saldos, actualizar la cantidad entrante
                        string queryActualizarSaldo = @"
                                    BEGIN TRANSACTION;

                                    BEGIN TRY
                                        UPDATE Saldos 
                                        SET Cantidad_Entrante = Cantidad_Entrante + @Cantidad_Entrante 
                                        WHERE ID_Producto = @ID_Producto;

                                        COMMIT TRANSACTION;
                                    END TRY
                                    BEGIN CATCH
                                        ROLLBACK TRANSACTION;
                                        THROW; -- Opcional: relanza el error
                                    END CATCH;;";

                        SqlCommand cmdActualizarSaldo = new SqlCommand(queryActualizarSaldo, conexion, transaction);
                        cmdActualizarSaldo.Parameters.AddWithValue("@Cantidad_Entrante", cantidadEntrante);
                        cmdActualizarSaldo.Parameters.AddWithValue("@ID_Producto", idProducto);

                        cmdActualizarSaldo.ExecuteNonQuery();
                    }
                    else
                    {
                        // Si el producto no está en Saldos, insertar un nuevo registro
                        string queryInsertarSaldo = @"BEGIN TRANSACTION;

                            BEGIN TRY
                                INSERT INTO Saldos (ID_Producto, Cantidad_Entrante, Cantidad_Salida) 
                                VALUES (@ID_Producto, @Cantidad_Entrante, 0);

                                COMMIT TRANSACTION;
                            END TRY
                            BEGIN CATCH
                                ROLLBACK TRANSACTION;
                                THROW; -- Opcional: relanza el error
                            END CATCH;"; // Asumiendo que la salida inicial es 0

                        SqlCommand cmdInsertarSaldo = new SqlCommand(queryInsertarSaldo, conexion, transaction);
                        cmdInsertarSaldo.Parameters.AddWithValue("@ID_Producto", idProducto);
                        cmdInsertarSaldo.Parameters.AddWithValue("@Cantidad_Entrante", cantidadEntrante);

                        cmdInsertarSaldo.ExecuteNonQuery();
                    }

                    // Confirmar la transacción
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Deshacer la transacción en caso de error
                    CapturarError(ex);
                    transaction.Rollback();
                    throw new Exception("Error al agregar el producto al inventario: " + ex.Message);

                }
                finally
                {
                    // Cerrar la conexión
                    con.CerrarConexion();
                }
            }
        }*/

        //listo
        public void AgregarProductoAlInventario(DateTime fechaRegistro, string observaciones, decimal importe, decimal iva, decimal total, int idProveedor, int idProducto, int cantidadEntrante, decimal costoUnitario)
        {
            ConexionInisio con = new ConexionInisio();

            try
            {
                if (!con.AbrirConexion())
                {
                    throw new Exception("No se pudo abrir la conexión a la base de datos.");
                }

                using (SqlCommand cmd = new SqlCommand("sp_AltaInventario", con.ObtenerConexion()))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Agregar los parámetros al comando
                    cmd.Parameters.AddWithValue("@Fecha_Registro", fechaRegistro);
                    cmd.Parameters.AddWithValue("@Observaciones", observaciones);
                    cmd.Parameters.AddWithValue("@Importe", importe);
                    cmd.Parameters.AddWithValue("@IVA", iva);
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@ID_Proveedor", idProveedor);
                    cmd.Parameters.AddWithValue("@ID_Producto", idProducto);
                    cmd.Parameters.AddWithValue("@Cantidad_Entrante", cantidadEntrante);
                    cmd.Parameters.AddWithValue("@Costo_Unitario", costoUnitario);

                    // Ejecutar el procedimiento almacenado
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                throw new Exception("Error al agregar el producto al inventario: " + ex.Message);
            }
            finally
            {
                con.CerrarConexion();
            }
        }


        public bool EliminarInventario(int idInventario)
        {
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();

            try
            {
                if (con.AbrirConexion())
                {
                    // Crear el comando para ejecutar el procedimiento almacenado
                    using (SqlCommand cmd = new SqlCommand("sp_BajaInventario", con.ObtenerConexion()))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Agregar el parámetro necesario
                        cmd.Parameters.AddWithValue("@ID_Inventario", idInventario);

                        // Ejecutar el procedimiento almacenado
                        int filasAfectadas = cmd.ExecuteNonQuery();

                        // Verificar si se afectaron filas para considerar el borrado exitoso
                        resultado = filasAfectadas > 0;
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                CapturarError(ex);
                Console.WriteLine("Error al eliminar el inventario: " + ex.Message);
            }

            return resultado;
        }
        public List<Inventario> ObtenerInventarioPorID(int idInventario)
        {
            List<Inventario> listaInventarios = new List<Inventario>();
            ConexionInisio con = new ConexionInisio();

            try
            {
                if (con.AbrirConexion())
                {
                    // Consulta SQL para obtener el inventario por ID
                    string query = "SELECT ID_Inventario, Fecha_Registro, Observaciones, Importe, IVA, Total, ID_Proveedor " +
                                   "FROM Inventario WHERE ID_Inventario = @ID_Inventario;";

                    using (SqlCommand cmd = new SqlCommand(query, con.ObtenerConexion()))
                    {
                        cmd.Parameters.AddWithValue("@ID_Inventario", idInventario);

                        // Ejecutar la consulta
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Inventario inventario = new Inventario
                                {
                                    ID_Inventario = reader.GetInt32(reader.GetOrdinal("ID_Inventario")),
                                    Fecha_Registro = reader.GetDateTime(reader.GetOrdinal("Fecha_Registro")),
                                    Observaciones = reader.IsDBNull(reader.GetOrdinal("Observaciones")) ? null : reader.GetString(reader.GetOrdinal("Observaciones")),
                                    Importe = reader.GetDecimal(reader.GetOrdinal("Importe")),
                                    IVA = reader.GetDecimal(reader.GetOrdinal("IVA")),
                                    Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                                    //ID_Proveedor = reader.GetInt32(reader.GetOrdinal("ID_Proveedor"))
                                };

                                // Agregar el objeto inventario a la lista
                                listaInventarios.Add(inventario);
                            }
                        }
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                CapturarError(ex);
                throw new Exception("Error al obtener el inventario: " + ex.Message);
            }

            return listaInventarios;
        }


        public List<string> ObtenerNombresProveedores()
        {
            List<string> nombresProveedores = new List<string>();
            ConexionInisio con = new ConexionInisio();
            
            // Abrir la conexión
            if (!con.AbrirConexion())
            {
                throw new Exception("No se pudo abrir la conexión a la base de datos.");
            }
            conexion = con.ObtenerConexion();
            try
            {
                string query = "SELECT Nombre FROM Proveedor";

                // Crear el comando SQL
                using (SqlCommand command = new SqlCommand(query, conexion))
                {
                    // Ejecutar el comando y leer los resultados
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Agregar el nombre del proveedor a la lista
                            nombresProveedores.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                CapturarError(ex);
                throw new Exception("Error al obtener los nombres de los proveedores: " + ex.Message);
            }
            finally
            {
                // Cerrar la conexión
                con.CerrarConexion();
            }

            return nombresProveedores;
        }

        public int ObtenerIDProveedorPorNombre(string nombreProveedor)
        {
            int idProveedor = -1;
            ConexionInisio con = new ConexionInisio();
           
            // Abrir la conexión
            if (con.AbrirConexion())
            {
                conexion = con.ObtenerConexion();
                try
                {
                    string query = "SELECT ID_Proveedor FROM Proveedor WHERE Nombre = @NombreProveedor";
                    SqlCommand comando = new SqlCommand(query, conexion);
                    comando.Parameters.AddWithValue("@NombreProveedor", nombreProveedor);

                    SqlDataReader reader = comando.ExecuteReader();

                    if (reader.Read())
                    {
                        idProveedor = Convert.ToInt32(reader["ID_Proveedor"]);
                    }

                    reader.Close();
                }
                catch (SqlException ex)
                {
                    CapturarError(ex);
                    Console.WriteLine("Error al obtener el ID del proveedor: " + ex.Message);
                }
                finally
                {
                    con.CerrarConexion();
                }
            }

            return idProveedor;
        }


        public List<Inventario> ObtenerInventariosPorNombreProveedor(string nombreProveedor)
        {
            List<Inventario> listaInventarios = new List<Inventario>();
            ConexionInisio con = new ConexionInisio();
            
            // Abrir la conexión
            if (con.AbrirConexion())
            {
                conexion = con.ObtenerConexion();
                try
                {
                    // Consulta para obtener los inventarios por nombre de proveedor
                    string query = @"
                SELECT i.ID_Inventario, i.Fecha_Registro, i.Observaciones, i.Importe, i.IVA, i.Total 
                FROM Inventario i
                INNER JOIN Proveedor p ON i.ID_Proveedor = p.ID_Proveedor
                WHERE p.Nombre = @NombreProveedor";

                    SqlCommand comando = new SqlCommand(query, conexion);
                    comando.Parameters.AddWithValue("@NombreProveedor", nombreProveedor);

                    SqlDataReader reader = comando.ExecuteReader();

                    // Leer los inventarios asociados al proveedor
                    while (reader.Read())
                    {
                        Inventario inventario = new Inventario
                        {
                            ID_Inventario = Convert.ToInt32(reader["ID_Inventario"]),
                            Fecha_Registro = Convert.ToDateTime(reader["Fecha_Registro"]),
                            Observaciones = reader["Observaciones"] != DBNull.Value ? reader["Observaciones"].ToString() : string.Empty, // Asigna el valor
                            Importe = Convert.ToDecimal(reader["Importe"]),
                            IVA = Convert.ToDecimal(reader["IVA"]),
                            Total = Convert.ToDecimal(reader["Total"])
                        };

                        listaInventarios.Add(inventario);
                    }

                    reader.Close();
                }
                catch (SqlException ex)
                {
                    CapturarError(ex);
                    Console.WriteLine("Error al obtener los inventarios: " + ex.Message);
                }
                finally
                {
                    con.CerrarConexion();
                }
            }

            return listaInventarios;
        }

        public bool ActualizarInventario(int idInventario, DateTime fechaRegistro, string observaciones, decimal importe, decimal iva, decimal total)
        {
            bool resultado = false;
            ConexionInisio con = new ConexionInisio();

            try
            {
                if (con.AbrirConexion())
                {
                    // Llamada al procedimiento almacenado sp_ActualizarInventario
                    string spActualizarInventario = "sp_ActualizarInventario"; // Nombre del procedimiento almacenado

                    using (SqlCommand cmd = new SqlCommand(spActualizarInventario, con.ObtenerConexion()))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Agregar los parámetros necesarios para el procedimiento almacenado
                        cmd.Parameters.AddWithValue("@ID_Inventario", idInventario);
                        cmd.Parameters.AddWithValue("@Fecha_Registro", fechaRegistro);
                        cmd.Parameters.AddWithValue("@Observaciones", observaciones);
                        cmd.Parameters.AddWithValue("@Importe", importe);
                        cmd.Parameters.AddWithValue("@IVA", iva);
                        cmd.Parameters.AddWithValue("@Total", total);

                        // Ejecutar el comando y verificar si se actualizó algún registro
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        resultado = filasAfectadas > 0;
                    }

                    con.CerrarConexion();
                }
            }
            catch (Exception ex)
            {
                // Manejar el error, capturando detalles
                CapturarError(ex);
                Console.WriteLine("Error al actualizar inventario: " + ex.Message);
            }

            return resultado;
        }

        public List<DetalleInventario> ObtenerDetallesDeInventarioPorIDProducto(int idProducto)
        {
            List<DetalleInventario> listaDetalles = new List<DetalleInventario>();
            ConexionInisio con = new ConexionInisio();
            
            if (con.AbrirConexion())
            {
                conexion = con.ObtenerConexion();
                try
                {
                    string query = @"
                SELECT di.ID_Detalle_Inventario, di.ID_Inventario, di.ID_Producto, p.Nombre, di.Cantidad_Entrante, di.Costo_Unitario, di.Subtotal, 
                FROM DetalleInventario di
                JOIN Producto p ON di.ID_Producto = p.ID_Producto
                WHERE di.ID_Producto = @IDProducto";

                    SqlCommand comando = new SqlCommand(query, conexion);
                    comando.Parameters.AddWithValue("@IDProducto", idProducto);
                    SqlDataReader reader = comando.ExecuteReader();

                    while (reader.Read())
                    {
                        DetalleInventario detalle = new DetalleInventario
                        {
                            ID_Detalle_Inventario = Convert.ToInt32(reader["ID_Detalle_Inventario"]),
                            ID_Inventario = Convert.ToInt32(reader["ID_Inventario"]),
                            ID_Producto = Convert.ToInt32(reader["ID_Producto"]),
                            NombreProducto = reader["Nombre"].ToString(),
                            Cantidad_Entrante = Convert.ToInt32(reader["Cantidad_Entrante"]),
                            Costo_Unitario = Convert.ToDecimal(reader["Costo_Unitario"]),
                            Subtotal = Convert.ToDecimal(reader["Subtotal"])
                        };

                        listaDetalles.Add(detalle);
                    }
                    reader.Close();
                }
                catch (SqlException ex)
                {
                    CapturarError(ex);
                    Console.WriteLine("Error al obtener los detalles del inventario por ID del producto: " + ex.Message);
                }
                finally
                {
                    con.CerrarConexion();
                }
            }

            return listaDetalles;
        }


    }

    public class Inventario
    {
        // Propiedad para el ID del inventario
        public int ID_Inventario { get; set; }

        // Propiedad para la fecha de registro
        public DateTime Fecha_Registro { get; set; }

        // Propiedad para las observaciones
        public string Observaciones { get; set; }

        // Propiedad para el importe total (sin IVA)
        public decimal Importe { get; set; }

        // Propiedad para el IVA del inventario
        public decimal IVA { get; set; }

        // Propiedad para el total con IVA incluido
        public decimal Total { get; set; }

        // Propiedad para el ID del proveedor asociado al inventario
        public int ID_Proveedor { get; set; }
    }

    public class DetalleInventario
    {
        public int ID_Detalle_Inventario { get; set; }
        public int ID_Inventario { get; set; }
        public int ID_Producto { get; set; }
        public string NombreProducto { get; set; } // Nombre del producto
        public int Cantidad_Entrante { get; set; }
        public decimal Costo_Unitario { get; set; }
        public decimal Subtotal { get; set; }
    }


}
