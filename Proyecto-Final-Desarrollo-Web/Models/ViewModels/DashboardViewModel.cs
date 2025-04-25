using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class DashboardViewModel
    {
        // Estadísticas generales
        public int TotalProductos { get; set; }
        public int TotalClientes { get; set; }
        public int TotalProveedores { get; set; }
        public int TotalVentasMes { get; set; }
        public decimal MontoTotalVentasMes { get; set; }
        public int ProductosPorVencer { get; set; }
        public int ProductosBajoStock { get; set; }

        // Datos para gráficos
        public List<VentasDiariasViewModel> VentasUltimos7Dias { get; set; }
        public List<TopProductoViewModel> TopProductos { get; set; }

        public DashboardViewModel()
        {
            VentasUltimos7Dias = new List<VentasDiariasViewModel>();
            TopProductos = new List<TopProductoViewModel>();
        }

        // Método para cargar datos desde la base de datos
        public static DashboardViewModel CargarDatos(FarmaUEntities db)
        {
            var viewModel = new DashboardViewModel();

            DateTime fechaActual = DateTime.Now;
            DateTime inicioMes = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            DateTime finMes = inicioMes.AddMonths(1).AddDays(-1);

            // Estadísticas generales
            viewModel.TotalProductos = db.Productos.Count(p => p.estado == "Activo");
            viewModel.TotalClientes = db.Clientes.Count();
            viewModel.TotalProveedores = db.Proveedores.Count(p => p.activo);

            // Ventas del mes actual
            var ventasMes = db.Facturas
                .Where(f => f.fecha >= inicioMes && f.fecha <= finMes && f.estado == "Completada")
                .ToList();

            viewModel.TotalVentasMes = ventasMes.Count;
            viewModel.MontoTotalVentasMes = ventasMes.Sum(v => v.total);

            // Productos por vencer (próximos 30 días)
            DateTime fechaLimiteVencimiento = fechaActual.AddDays(30);
            viewModel.ProductosPorVencer = db.Lotes
                .Count(l => l.fecha_vencimiento > fechaActual &&
                            l.fecha_vencimiento <= fechaLimiteVencimiento &&
                            l.cantidad > 0);

            // Productos con bajo stock
            viewModel.ProductosBajoStock = db.Productos
                .Where(p => p.estado == "Activo")
                .Select(p => new {
                    Producto = p,
                    StockTotal = p.Lotes.Sum(l => l.cantidad)
                })
                .Count(x => x.StockTotal < 10);

            // Ventas de los últimos 7 días
            var fechaInicio7dias = fechaActual.AddDays(-6);
            var ventasUltimos7Dias = db.Facturas
                .Where(f => f.fecha >= fechaInicio7dias && f.estado == "Completada")
                .GroupBy(f => f.fecha.Value.Date)
                .Select(g => new {
                    Fecha = g.Key,
                    Total = g.Sum(f => f.total),
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Fecha)
                .ToList();

            // Asegura que se cree la lista para los 7 días, incluso sin ventas
            for (int i = 0; i < 7; i++)
            {
                var fecha = fechaInicio7dias.AddDays(i);
                var ventaDia = ventasUltimos7Dias.FirstOrDefault(v => v.Fecha == fecha.Date);

                viewModel.VentasUltimos7Dias.Add(new VentasDiariasViewModel
                {
                    Fecha = fecha,
                    FechaFormateada = fecha.ToString("dd/MM"),
                    Total = ventaDia != null ? ventaDia.Total : 0,
                    Cantidad = ventaDia != null ? ventaDia.Cantidad : 0
                });
            }

            // Top 5 productos más vendidos del mes
            var topProductos = db.Detalles_Factura
                .Where(d => d.Facturas.fecha >= inicioMes &&
                            d.Facturas.fecha <= finMes &&
                            d.Facturas.estado == "Completada")
                .GroupBy(d => new { d.Productos.ID_Producto, d.Productos.Nombre })
                .Select(g => new TopProductoViewModel
                {
                    ID_Producto = g.Key.ID_Producto,
                    Producto = g.Key.Nombre,
                    Cantidad = g.Sum(d => d.cantidad)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            viewModel.TopProductos = topProductos;

            return viewModel;
        }
    }

    public class VentasDiariasViewModel
    {
        public DateTime Fecha { get; set; }
        public string FechaFormateada { get; set; }
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
    }

    public class TopProductoViewModel
    {
        public int ID_Producto { get; set; }
        public string Producto { get; set; }
        public int Cantidad { get; set; }
    }
}
