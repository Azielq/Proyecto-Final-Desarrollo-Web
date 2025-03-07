using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class DashboardViewModel
    {
        // Estos son las estadísticas generales
        public int TotalMedicamentos { get; set; }
        public int TotalClientes { get; set; }
        public int TotalProveedores { get; set; }
        public int TotalVentasMes { get; set; }
        public decimal MontoTotalVentasMes { get; set; }
        public int MedicamentosPorVencer { get; set; }
        public int MedicamentosBajoStock { get; set; }

        // Estos son los datos para gráficos
        public List<VentasDiariasViewModel> VentasUltimos7Dias { get; set; }
        public List<TopMedicamentoViewModel> TopMedicamentos { get; set; }

        public DashboardViewModel()
        {
            VentasUltimos7Dias = new List<VentasDiariasViewModel>();
            TopMedicamentos = new List<TopMedicamentoViewModel>();
        }

        // Este de acá es el método para cargar datos desde la base de datos
        public static DashboardViewModel CargarDatos(FarmaUEntities db)
        {
            var viewModel = new DashboardViewModel();

            // Con esto se obtiene fecha actual y cálculos de fechas
            DateTime fechaActual = DateTime.Now;
            DateTime inicioMes = new DateTime(fechaActual.Year, fechaActual.Month, 1);
            DateTime finMes = inicioMes.AddMonths(1).AddDays(-1);

            // Estas son estadísticas generales
            viewModel.TotalMedicamentos = db.Medicamentos.Count(m => m.estado == "Activo");
            viewModel.TotalClientes = db.Clientes.Count();
            viewModel.TotalProveedores = db.Proveedores.Count(p => p.activo);

            // Para las ventas del mes actual
            var ventasMes = db.Facturas
                .Where(f => f.fecha >= inicioMes && f.fecha <= finMes && f.estado == "Completada")
                .ToList();

            viewModel.TotalVentasMes = ventasMes.Count;
            viewModel.MontoTotalVentasMes = ventasMes.Sum(v => v.total);

            // Para los medicamentos por vencer (próximos 30 días)
            DateTime fechaLimiteVencimiento = fechaActual.AddDays(30);
            viewModel.MedicamentosPorVencer = db.Lotes
                .Count(l => l.fecha_vencimiento > fechaActual &&
                            l.fecha_vencimiento <= fechaLimiteVencimiento &&
                            l.cantidad > 0);

            // Para los medicamentos con bajo stock
            viewModel.MedicamentosBajoStock = db.Medicamentos
                .Where(m => m.estado == "Activo")
                .Select(m => new {
                    Medicamento = m,
                    StockTotal = m.Lotes.Sum(l => l.cantidad)
                })
                .Count(x => x.StockTotal < 10);

            // Este de acá es para el gráfico de ventas de los últimos 7 días
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

            // Con esto de acá se crea el array para todos los días incluso sin ventas
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

            // Sirve para el top 5 medicamentos más vendidos del mes.
            var topMedicamentos = db.Detalles_Factura
                .Where(d => d.Facturas.fecha >= inicioMes &&
                            d.Facturas.fecha <= finMes &&
                            d.Facturas.estado == "Completada")
                .GroupBy(d => new { d.Medicamentos.ID_Medicamento, d.Medicamentos.Nombre })
                .Select(g => new TopMedicamentoViewModel
                {
                    ID_Medicamento = g.Key.ID_Medicamento,
                    Medicamento = g.Key.Nombre,
                    Cantidad = g.Sum(d => d.cantidad)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(5)
                .ToList();

            viewModel.TopMedicamentos = topMedicamentos;

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

    public class TopMedicamentoViewModel
    {
        public int ID_Medicamento { get; set; }
        public string Medicamento { get; set; }
        public int Cantidad { get; set; }
    }
}