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
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using Proyecto_Final_Desarrollo_Web.Helpers;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class InventarioController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Inventario
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", string ubicacion = "", int? medicamentoId = null, int? categoriaId = null, bool? soloVencidos = null, bool? soloPorVencer = null)
        {
            try
            {
                // Carga listas para filtros
                ViewBag.Medicamentos = new SelectList(db.Medicamentos.Where(m => m.estado == "Activo").OrderBy(m => m.Nombre), "ID_Medicamento", "Nombre");
                ViewBag.Categorias = new SelectList(db.Categorias.OrderBy(c => c.Nombre), "ID_Categoría", "Nombre");

                // Consulta base
                var query = db.Inventario
                    .Include(i => i.Lotes)
                    .Include(i => i.Lotes.Medicamentos)
                    .Include(i => i.Lotes.Medicamentos.Categorias)
                    .Include(i => i.Lotes.Medicamentos.Laboratorios)
                    .AsQueryable();

                // Aplica filtros
                if (!string.IsNullOrWhiteSpace(ubicacion))
                {
                    query = query.Where(i => i.ubicacion.Contains(ubicacion));
                }

                if (medicamentoId.HasValue)
                {
                    query = query.Where(i => i.Lotes.ID_Medicamento == medicamentoId.Value);
                }

                if (categoriaId.HasValue)
                {
                    query = query.Where(i => i.Lotes.Medicamentos.ID_Categoría == categoriaId.Value);
                }

                if (soloVencidos.HasValue && soloVencidos.Value)
                {
                    query = query.Where(i => i.Lotes.fecha_vencimiento < DateTime.Now);
                }

                if (soloPorVencer.HasValue && soloPorVencer.Value)
                {
                    query = query.Where(i => i.Lotes.fecha_vencimiento > DateTime.Now &&
                                           i.Lotes.fecha_vencimiento <= DateTime.Now.AddDays(30));
                }

                // Busca por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(i => i.ubicacion.ToLower().Contains(searchValue) ||
                                            i.Lotes.Medicamentos.Nombre.ToLower().Contains(searchValue) ||
                                            i.Lotes.Medicamentos.Categorias.Nombre.ToLower().Contains(searchValue) ||
                                            i.Lotes.Medicamentos.Laboratorios.Nombre.ToLower().Contains(searchValue));
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var inventario = query
                    .OrderBy(i => i.Lotes.fecha_vencimiento)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Ubicacion = ubicacion;
                ViewBag.MedicamentoId = medicamentoId;
                ViewBag.CategoriaId = categoriaId;
                ViewBag.SoloVencidos = soloVencidos;
                ViewBag.SoloPorVencer = soloPorVencer;

                // Estadísticas
                ViewBag.TotalUnidades = query.Sum(i => i.Lotes.cantidad);
                ViewBag.ValorTotal = query.Sum(i => i.Lotes.cantidad * i.Lotes.Medicamentos.precio_compra);

                // Transforma a ViewModel
                var inventarioViewModel = inventario.Select(i => new InventarioTableViewModel
                {
                    id_Inventario = i.id_Inventario,
                    ID_Lote = i.ID_Lote,
                    NombreMedicamento = i.Lotes.Medicamentos.Nombre,
                    Categoria = i.Lotes.Medicamentos.Categorias.Nombre,
                    Laboratorio = i.Lotes.Medicamentos.Laboratorios.Nombre,
                    NumeroLote = i.ID_Lote.ToString(),
                    ubicacion = i.ubicacion,
                    Cantidad = i.Lotes.cantidad,
                    FechaVencimiento = i.Lotes.fecha_vencimiento,
                    Estado = GetEstadoLote(i.Lotes.fecha_vencimiento)
                }).ToList();

                return View(inventarioViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar inventario: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar el inventario: " + ex.Message;
                return View(new List<InventarioTableViewModel>());
            }
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

        // GET: Inventario/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Inventario inventario = db.Inventario
                .Include(i => i.Lotes)
                .Include(i => i.Lotes.Medicamentos)
                .Include(i => i.Lotes.Medicamentos.Categorias)
                .Include(i => i.Lotes.Medicamentos.Laboratorios)
                .FirstOrDefault(i => i.id_Inventario == id);

            if (inventario == null)
            {
                return HttpNotFound();
            }

            var loteViewModel = LoteViewModel.FromEntity(inventario.Lotes);
            loteViewModel.Ubicacion = inventario.ubicacion;

            return View(loteViewModel);
        }

        // GET: Inventario/Create
        public ActionResult Create(int? loteId = null)
        {
            var viewModel = new InventarioTableViewModel();

            if (loteId.HasValue)
            {
                var lote = db.Lotes
                    .Include(l => l.Medicamentos)
                    .Include(l => l.Medicamentos.Categorias)
                    .Include(l => l.Medicamentos.Laboratorios)
                    .FirstOrDefault(l => l.id_Lote == loteId);

                if (lote != null)
                {
                    viewModel.ID_Lote = lote.id_Lote;
                    viewModel.NombreMedicamento = lote.Medicamentos.Nombre;
                    viewModel.Categoria = lote.Medicamentos.Categorias.Nombre;
                    viewModel.Laboratorio = lote.Medicamentos.Laboratorios.Nombre;
                    viewModel.Cantidad = lote.cantidad;
                    viewModel.FechaVencimiento = lote.fecha_vencimiento;

                    ViewBag.ReadOnlyLote = true;
                }
            }

            if (!loteId.HasValue)
            {
                ViewBag.ID_Lote = new SelectList(db.Lotes
                    .Include(l => l.Medicamentos)
                    .Where(l => !db.Inventario.Any(i => i.ID_Lote == l.id_Lote))
                    .Select(l => new {
                        l.id_Lote,
                        NombreCompleto = l.Medicamentos.Nombre + " - Vence: " + l.fecha_vencimiento.ToString("dd/MM/yyyy")
                    }), "id_Lote", "NombreCompleto");
            }

            return View(viewModel);
        }

        // POST: Inventario/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InventarioTableViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var inventario = new Inventario
                {
                    ID_Lote = viewModel.ID_Lote,
                    ubicacion = viewModel.ubicacion
                };

                db.Inventario.Add(inventario);
                db.SaveChanges();

                TempData["Message"] = "Ubicación de inventario asignada correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            if (db.Inventario.Any(i => i.ID_Lote == viewModel.ID_Lote))
            {
                ModelState.AddModelError("ID_Lote", "Este lote ya tiene una ubicación asignada");
            }

            ViewBag.ID_Lote = new SelectList(db.Lotes
                .Include(l => l.Medicamentos)
                .Where(l => !db.Inventario.Any(i => i.ID_Lote == l.id_Lote))
                .Select(l => new {
                    l.id_Lote,
                    NombreCompleto = l.Medicamentos.Nombre + " - Vence: " + l.fecha_vencimiento.ToString("dd/MM/yyyy")
                }), "id_Lote", "NombreCompleto", viewModel.ID_Lote);

            return View(viewModel);
        }

        // GET: Inventario/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Inventario inventario = db.Inventario
                .Include(i => i.Lotes)
                .Include(i => i.Lotes.Medicamentos)
                .Include(i => i.Lotes.Medicamentos.Categorias)
                .Include(i => i.Lotes.Medicamentos.Laboratorios)
                .FirstOrDefault(i => i.id_Inventario == id);

            if (inventario == null)
            {
                return HttpNotFound();
            }

            var viewModel = new InventarioTableViewModel
            {
                id_Inventario = inventario.id_Inventario,
                ID_Lote = inventario.ID_Lote,
                NombreMedicamento = inventario.Lotes.Medicamentos.Nombre,
                Categoria = inventario.Lotes.Medicamentos.Categorias.Nombre,
                Laboratorio = inventario.Lotes.Medicamentos.Laboratorios.Nombre,
                NumeroLote = inventario.ID_Lote.ToString(),
                ubicacion = inventario.ubicacion,
                Cantidad = inventario.Lotes.cantidad,
                FechaVencimiento = inventario.Lotes.fecha_vencimiento
            };

            return View(viewModel);
        }

        // POST: Inventario/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(InventarioTableViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var inventario = db.Inventario.Find(viewModel.id_Inventario);
                if (inventario == null)
                {
                    return HttpNotFound();
                }

                inventario.ubicacion = viewModel.ubicacion;
                db.Entry(inventario).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Ubicación de inventario actualizada correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        // GET: Inventario/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Inventario inventario = db.Inventario
                .Include(i => i.Lotes)
                .Include(i => i.Lotes.Medicamentos)
                .Include(i => i.Lotes.Medicamentos.Categorias)
                .Include(i => i.Lotes.Medicamentos.Laboratorios)
                .FirstOrDefault(i => i.id_Inventario == id);

            if (inventario == null)
            {
                return HttpNotFound();
            }

            var viewModel = new InventarioTableViewModel
            {
                id_Inventario = inventario.id_Inventario,
                ID_Lote = inventario.ID_Lote,
                NombreMedicamento = inventario.Lotes.Medicamentos.Nombre,
                Categoria = inventario.Lotes.Medicamentos.Categorias.Nombre,
                Laboratorio = inventario.Lotes.Medicamentos.Laboratorios.Nombre,
                NumeroLote = inventario.ID_Lote.ToString(),
                ubicacion = inventario.ubicacion,
                Cantidad = inventario.Lotes.cantidad,
                FechaVencimiento = inventario.Lotes.fecha_vencimiento
            };

            return View(viewModel);
        }

        // POST: Inventario/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Inventario inventario = db.Inventario.Find(id);
            db.Inventario.Remove(inventario);
            db.SaveChanges();

            TempData["Message"] = "Ubicación de inventario eliminada correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Inventario/Reporte
        public ActionResult Reporte()
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

        // GET: Inventario/MovimientosInventario
        public ActionResult MovimientosInventario()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetMovimientos(MovimientoInventarioTableRequest request)
        {
            var response = new MovimientoInventarioTableResponse
            {
                draw = request.Start
            };

            // Consulta base
            var query = db.Movimientos_Inventario
                .Include(m => m.Medicamentos)
                .Include(m => m.Lotes)
                .AsQueryable();

            // Filtros
            if (request.FechaInicio.HasValue)
            {
                query = query.Where(m => m.fecha >= request.FechaInicio.Value);
            }

            if (request.FechaFin.HasValue)
            {
                var fechaFinAjustada = request.FechaFin.Value.AddDays(1).AddSeconds(-1);
                query = query.Where(m => m.fecha <= fechaFinAjustada);
            }

            if (request.MedicamentoId.HasValue)
            {
                query = query.Where(m => m.id_medicamento == request.MedicamentoId.Value);
            }

            if (request.LoteId.HasValue)
            {
                query = query.Where(m => m.id_Lote == request.LoteId.Value);
            }

            if (!string.IsNullOrEmpty(request.Tipo))
            {
                query = query.Where(m => m.tipo == request.Tipo);
            }

            // Busca por término
            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var searchValue = request.SearchValue.ToLower();
                query = query.Where(m => m.Medicamentos.Nombre.ToLower().Contains(searchValue) ||
                                        m.tipo.ToLower().Contains(searchValue));
            }

            // Total de registros sin filtrar
            response.recordsTotal = db.Movimientos_Inventario.Count();

            // Total después de filtrar
            response.recordsFiltered = query.Count();

            // Estadísticas
            response.TotalEntradas = query.Count(m => m.tipo.Contains("Entrada"));
            response.TotalSalidas = query.Count(m => m.tipo.Contains("Salida"));

            var entradasValor = query
                .Where(m => m.tipo.Contains("Entrada"))
                .ToList()
                .Sum(m => m.cantidad * m.Medicamentos.precio_compra);

            var salidasValor = query
                .Where(m => m.tipo.Contains("Salida"))
                .ToList()
                .Sum(m => m.cantidad * m.Medicamentos.precio_venta);

            response.ValorTotalEntradas = entradasValor;
            response.ValorTotalSalidas = salidasValor;

            // Ordena
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                if (request.SortDirection.ToLower() == "asc")
                {
                    switch (request.SortColumn)
                    {
                        case "fecha":
                            query = query.OrderBy(m => m.fecha);
                            break;
                        case "NombreMedicamento":
                            query = query.OrderBy(m => m.Medicamentos.Nombre);
                            break;
                        case "tipo":
                            query = query.OrderBy(m => m.tipo);
                            break;
                        case "cantidad":
                            query = query.OrderBy(m => m.cantidad);
                            break;
                        default:
                            query = query.OrderBy(m => m.fecha);
                            break;
                    }
                }
                else
                {
                    switch (request.SortColumn)
                    {
                        case "fecha":
                            query = query.OrderByDescending(m => m.fecha);
                            break;
                        case "NombreMedicamento":
                            query = query.OrderByDescending(m => m.Medicamentos.Nombre);
                            break;
                        case "tipo":
                            query = query.OrderByDescending(m => m.tipo);
                            break;
                        case "cantidad":
                            query = query.OrderByDescending(m => m.cantidad);
                            break;
                        default:
                            query = query.OrderByDescending(m => m.fecha);
                            break;
                    }
                }
            }
            else
            {
                query = query.OrderByDescending(m => m.fecha);
            }

            // Paginación
            query = query.Skip(request.Start).Take(request.Length);

            // Esto convierte y asigna a la respuesta
            response.data = query.ToList().Select(m => new MovimientoInventarioTableViewModel
            {
                ID_movimiento = m.ID_movimiento,
                fecha = m.fecha,
                NombreMedicamento = m.Medicamentos.Nombre,
                id_Lote = m.id_Lote,
                FechaVencimiento = m.Lotes.fecha_vencimiento,
                tipo = m.tipo,
                cantidad = m.cantidad,
                DocumentoReferencia = GetDocumentoReferencia(m),
                ValorMovimiento = m.tipo.Contains("Entrada")
                    ? m.cantidad * m.Medicamentos.precio_compra
                    : m.cantidad * m.Medicamentos.precio_venta
            }).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        private string GetDocumentoReferencia(Movimientos_Inventario movimiento)
        {
            if (movimiento.ID_Factura.HasValue)
            {
                return "Factura #" + movimiento.ID_Factura.Value;
            }
            else if (movimiento.id_Compra.HasValue)
            {
                return "Compra #" + movimiento.id_Compra.Value;
            }
            else
            {
                return "Ajuste manual";
            }
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