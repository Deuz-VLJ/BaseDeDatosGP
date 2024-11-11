using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseDeDatosGP
{
    public class BDGraficas
    {
        ConexionInisio con = new ConexionInisio();
        public BDGraficas() 
        {
            
        }
        // Función para obtener datos de ventas de productos
        public List<VentaProducto> ObtenerVentasProductos()
        {
            List<VentaProducto> ventasProductos = new List<VentaProducto>();

            try
            {
                con.AbrirConexion();
                using (SqlCommand command = new SqlCommand("SELECT Producto, Total_Vendido FROM VistaProductosVendidos", con.ObtenerConexion()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ventasProductos.Add(new VentaProducto
                            {
                                Producto = reader["Producto"].ToString(),
                                TotalVendido = Convert.ToInt32(reader["Total_Vendido"])
                            });
                        }
                    }
                }
            }
            finally
            {
                con.CerrarConexion();
            }

            return ventasProductos;
        }

        // Función para obtener datos de ventas por mes y año
        public List<VentaMesAnio> ObtenerVentasPorMesAnio()
        {
            List<VentaMesAnio> ventasPorMesAnio = new List<VentaMesAnio>();

            try
            {
                con.AbrirConexion();
                using (SqlCommand command = new SqlCommand("SELECT Año, Mes, Total_Ventas FROM VistaPormesAño", con.ObtenerConexion()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ventasPorMesAnio.Add(new VentaMesAnio
                            {
                                Anio = Convert.ToInt32(reader["Año"]),
                                Mes = Convert.ToInt32(reader["Mes"]),
                                TotalVentas = Convert.ToDecimal(reader["Total_Ventas"])
                            });
                        }
                    }
                }
            }
            finally
            {
                con.CerrarConexion();
            }

            return ventasPorMesAnio;
        }


        // Función para obtener el total de ventas por cliente
        public List<VentaPorCliente> ObtenerVentasPorCliente()
        {
            List<VentaPorCliente> ventasPorCliente = new List<VentaPorCliente>();

            try
            {
                con.AbrirConexion();
                using (SqlCommand command = new SqlCommand("SELECT Cliente, Total_Ventas FROM VistaVentasTotalesPorCliente", con.ObtenerConexion()))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ventasPorCliente.Add(new VentaPorCliente
                            {
                                Cliente = reader["Cliente"].ToString(),
                                TotalVentas = Convert.ToDecimal(reader["Total_Ventas"])
                            });
                        }
                    }
                }
            }
            finally
            {
                con.CerrarConexion();
            }

            return ventasPorCliente;
        }

        // Función para obtener los ingresos diarios filtrados por fecha de inicio y fecha final
        public List<IngresoDiario> ObtenerIngresosDiariosPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            List<IngresoDiario> ingresos = new List<IngresoDiario>();

            try
            {
                // Abrir la conexión
                con.AbrirConexion(); // Asumiendo que tienes esta función también definida

                // SQL para obtener los datos filtrados por el rango de fechas
                string query = @"
                SELECT 
                    DATENAME(WEEKDAY, V.Fecha) AS Dia,
                    SUM(V.Total) AS Total_Ventas
                FROM 
                    Venta V
                WHERE 
                    V.Fecha BETWEEN @FechaInicio AND @FechaFin
                GROUP BY 
                    DATENAME(WEEKDAY, V.Fecha);";

                // Ejecutar la consulta
                SqlCommand cmd = new SqlCommand(query, con.ObtenerConexion());
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin);

                SqlDataReader reader = cmd.ExecuteReader();

                // Leer los resultados y almacenarlos en la lista
                while (reader.Read())
                {
                    ingresos.Add(new IngresoDiario
                    {
                        Dia = reader["Dia"].ToString(),
                        TotalVentas = Convert.ToDecimal(reader["Total_Ventas"])
                    });
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                throw new Exception("Error al obtener ingresos diarios por fecha: " + ex.Message);
            }
            finally
            {
                // Cerrar la conexión
                con.CerrarConexion(); // Asumiendo que tienes esta función también definida
            }

            return ingresos;
        }


    }

    // Clase que almacena la información de ingresos diarios
    public class IngresoDiario
    {
        public string Dia { get; set; }
        public decimal TotalVentas { get; set; }
    }
    public class VentaProducto
    {
        public string Producto { get; set; }
        public int TotalVendido { get; set; }
    }
    public class VentaMesAnio
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public decimal TotalVentas { get; set; }
    }
    public class VentaPorCliente
    {
        public string Cliente { get; set; }
        public decimal TotalVentas { get; set; }
    }
}
