using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class ReportesController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Reportes/Inventario
        public ActionResult Inventario()
        {
            var viewModel = new ReporteInventarioViewModel
            {
                FechaReporte = DateTime.Now
            };

            // Esto obtiene todos los medicamentos con stock
            var medicamentos = db.Medicamentos
                .Include(m => m.Categorias)
                .Include(m => m.Laboratorios)
                .Include(m => m.Lotes)
                .Where(m => m.estado == "Activo" && m.Lotes.Any(l => l.cantidad > 0))
                .ToList();

            // Estadísticas
            viewModel.TotalMedicamentos = medicamentos.Count;
            viewModel.TotalUnidades = medicamentos.Sum(m => m.Lotes.Sum(l => l.cantidad));
            viewModel.ValorTotalInventario = medicamentos.Sum(m => m.Lotes.Sum(l => l.cantidad * m.precio_compra));

            var fechaActual = DateTime.Now;

            viewModel.MedicamentosVencidos = medicamentos
                .Count(m => m.Lotes.Any(l => l.fecha_vencimiento < fechaActual && l.cantidad > 0));

            viewModel.MedicamentosPorVencer = medicamentos
                .Count(m => m.Lotes.Any(l => l.fecha_vencimiento > fechaActual &&
                                          l.fecha_vencimiento <= fechaActual.AddDays(30) &&
                                          l.cantidad > 0));

            // Detalles por medicamento
            foreach (var medicamento in medicamentos)
            {
                var detalle = new ReporteInventarioDetalleViewModel
                {
                    ID_Medicamento = medicamento.ID_Medicamento,
                    NombreMedicamento = medicamento.Nombre,
                    Categoria = medicamento.Categorias.Nombre,
                    Laboratorio = medicamento.Laboratorios.Nombre,
                    PrecioCompra = medicamento.precio_compra,
                    StockTotal = medicamento.Lotes.Sum(l => l.cantidad),
                    ValorTotal = medicamento.Lotes.Sum(l => l.cantidad * medicamento.precio_compra)
                };

                // Detalles por lote
                foreach (var lote in medicamento.Lotes.Where(l => l.cantidad > 0))
                {
                    var ubicacion = db.Inventario
                        .FirstOrDefault(i => i.ID_Lote == lote.id_Lote)?.ubicacion ?? "Sin ubicación";

                    detalle.Lotes.Add(new LoteResumenViewModel
                    {
                        id_Lote = lote.id_Lote,
                        Cantidad = lote.cantidad,
                        FechaVencimiento = lote.fecha_vencimiento,
                        Ubicacion = ubicacion,
                        Estado = GetEstadoLote(lote.fecha_vencimiento)
                    });
                }

                viewModel.DetallesInventario.Add(detalle);
            }

            return View(viewModel);
        }

        // GET: Reportes/Ventas
        public ActionResult Ventas()
        {
            var viewModel = new ReporteVentasViewModel();
            return View(viewModel);
        }

        // POST: Reportes/Ventas
        [HttpPost]
        public ActionResult Ventas(ReporteVentasViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                viewModel.FechaReporte = DateTime.Now;

                // Esto asegura que la fecha fin es el final del día
                var fechaFin = viewModel.FechaFin.AddDays(1).AddSeconds(-1);

                // Esto obtiene las ventas en el rango de fechas
                var ventas = db.Facturas
                    .Include(f => f.Clientes.Personas)
                    .Include(f => f.Detalles_Factura)
                    .Where(f => f.fecha >= viewModel.FechaInicio && f.fecha <= fechaFin)
                    .ToList();

                // Estadísticas
                viewModel.TotalVentas = ventas.Count(f => f.estado == "Completada");
                viewModel.MontoTotalVentas = ventas.Where(f => f.estado == "Completada").Sum(f => f.total);
                viewModel.PromedioVenta = viewModel.TotalVentas > 0 ? viewModel.MontoTotalVentas / viewModel.TotalVentas : 0;

                // Detalle de ventas
                viewModel.Ventas = ventas
                    .OrderByDescending(f => f.fecha)
                    .Select(f => new VentaReporteViewModel
                    {
                        id_Factura = f.id_Factura,
                        Fecha = f.fecha,
                        Cliente = $"{f.Clientes.Personas.Nombre} {f.Clientes.Personas.Apellido_1}",
                        Total = f.total,
                        CantidadProductos = f.Detalles_Factura.Count,
                        Estado = f.estado
                    })
                    .ToList();

                // Ventas por día
                viewModel.VentasPorDia = ventas
                    .Where(f => f.estado == "Completada")
                    .GroupBy(f => f.fecha.Value.Date)
                    .Select(g => new VentaPorDiaViewModel
                    {
                        Fecha = g.Key,
                        Cantidad = g.Count(),
                        Total = g.Sum(f => f.total)
                    })
                    .OrderBy(v => v.Fecha)
                    .ToList();

                // Top medicamentos vendidos
                var detalles = ventas
                    .Where(f => f.estado == "Completada")
                    .SelectMany(f => f.Detalles_Factura)
                    .ToList();

                decimal totalVentas = detalles.Sum(d => d.subtotal);

                viewModel.TopMedicamentos = detalles
                    .GroupBy(d => new {
                        ID_Medicamento = d.ID_Medicamento,
                        Nombre = d.Medicamentos.Nombre,
                        Categoria = d.Medicamentos.Categorias.Nombre
                    })
                    .Select(g => new TopMedicamentoVentaViewModel
                    {
                        ID_Medicamento = g.Key.ID_Medicamento,
                        Nombre = g.Key.Nombre,
                        Categoria = g.Key.Categoria,
                        Cantidad = g.Sum(d => d.cantidad),
                        TotalVentas = g.Sum(d => d.subtotal),
                        Porcentaje = totalVentas > 0 ? g.Sum(d => d.subtotal) / totalVentas : 0
                    })
                    .OrderByDescending(m => m.TotalVentas)
                    .Take(10)
                    .ToList();
            }

            return View(viewModel);
        }

        // GET: Reportes/LotesVencidos
        public ActionResult LotesVencidos()
        {
            var fechaActual = DateTime.Now;

            // Lotes vencidos o próximos a vencer
            var lotes = db.Lotes
                .Include(l => l.Medicamentos)
                .Include(l => l.Medicamentos.Categorias)
                .Include(l => l.Medicamentos.Laboratorios)
                .Include(l => l.Inventario)
                .Where(l => l.fecha_vencimiento <= fechaActual.AddDays(60) && l.cantidad > 0)
                .OrderBy(l => l.fecha_vencimiento)
                .ToList();

            // Agrupa por estado
            var lotesVencidos = lotes.Where(l => l.fecha_vencimiento < fechaActual).ToList();
            var lotesPorVencer30 = lotes.Where(l => l.fecha_vencimiento >= fechaActual && l.fecha_vencimiento <= fechaActual.AddDays(30)).ToList();
            var lotesPorVencer60 = lotes.Where(l => l.fecha_vencimiento > fechaActual.AddDays(30) && l.fecha_vencimiento <= fechaActual.AddDays(60)).ToList();

            // Estadísticas
            ViewBag.TotalLotesVencidos = lotesVencidos.Count;
            ViewBag.TotalLotesPorVencer30 = lotesPorVencer30.Count;
            ViewBag.TotalLotesPorVencer60 = lotesPorVencer60.Count;
            ViewBag.ValorLotesVencidos = lotesVencidos.Sum(l => l.cantidad * l.Medicamentos.precio_compra);

            ViewBag.LotesVencidos = lotesVencidos;
            ViewBag.LotesPorVencer30 = lotesPorVencer30;
            ViewBag.LotesPorVencer60 = lotesPorVencer60;

            return View();
        }

        // GET: Reportes/Movimientos
        public ActionResult Movimientos(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            if (!fechaInicio.HasValue)
            {
                fechaInicio = DateTime.Now.AddDays(-30);
            }

            if (!fechaFin.HasValue)
            {
                fechaFin = DateTime.Now;
            }

            // Esto asegura que la fecha fin es el final del día
            var fechaFinAjustada = fechaFin.Value.AddDays(1).AddSeconds(-1);

            // Esto obtiene los movimientos en el rango de fechas
            var movimientos = db.Movimientos_Inventario
                .Include(m => m.Medicamentos)
                .Include(m => m.Lotes)
                .Include(m => m.Facturas)
                .Include(m => m.Compras_Farmacia)
                .Where(m => m.fecha >= fechaInicio && m.fecha <= fechaFinAjustada)
                .OrderByDescending(m => m.fecha)
                .ToList();

            // Agrupa por tipo
            var entradasCompra = movimientos.Where(m => m.tipo == "Entrada" && m.id_Compra != null).ToList();
            var entradasCancelacion = movimientos.Where(m => m.tipo == "Entrada por Cancelación").ToList();
            var entradasAjuste = movimientos.Where(m => m.tipo == "Ajuste Entrada").ToList();
            var salidasVenta = movimientos.Where(m => m.tipo == "Salida" && m.ID_Factura != null).ToList();
            var salidasAjuste = movimientos.Where(m => m.tipo == "Ajuste Salida").ToList();

            // Estadísticas
            ViewBag.TotalMovimientos = movimientos.Count;
            ViewBag.TotalEntradas = entradasCompra.Count + entradasCancelacion.Count + entradasAjuste.Count;
            ViewBag.TotalSalidas = salidasVenta.Count + salidasAjuste.Count;

            ViewBag.EntradasCompra = entradasCompra;
            ViewBag.EntradasCancelacion = entradasCancelacion;
            ViewBag.EntradasAjuste = entradasAjuste;
            ViewBag.SalidasVenta = salidasVenta;
            ViewBag.SalidasAjuste = salidasAjuste;

            ViewBag.FechaInicio = fechaInicio.Value.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin.Value.ToString("yyyy-MM-dd");

            return View();
        }

        private string GetEstadoLote(DateTime fechaVencimiento)
        {
            int diasParaVencer = (int)(fechaVencimiento - DateTime.Now).TotalDays;

            if (diasParaVencer < 0)
                return "Vencido";
            else if (diasParaVencer <= 30)
                return "Por Vencer";
            else
                return "Vigente";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}