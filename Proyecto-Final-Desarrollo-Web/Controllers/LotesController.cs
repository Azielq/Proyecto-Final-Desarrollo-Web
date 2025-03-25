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
    public class LotesController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Lotes
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", int? medicamentoId = null, int? categoriaId = null, bool? soloVencidos = null, bool? soloPorVencer = null, bool? soloConStock = null)
        {
            try
            {
                // Carga listas para filtros
                ViewBag.Medicamentos = new SelectList(db.Medicamentos.Where(m => m.estado == "Activo").OrderBy(m => m.Nombre), "ID_Medicamento", "Nombre");
                ViewBag.Categorias = new SelectList(db.Categorias.OrderBy(c => c.Nombre), "ID_Categoría", "Nombre");

                // Consulta base
                var query = db.Lotes
                    .Include(l => l.Medicamentos)
                    .Include(l => l.Medicamentos.Categorias)
                    .Include(l => l.Medicamentos.Laboratorios)
                    .Include(l => l.Inventario)
                    .AsQueryable();

                // Aplica filtros
                if (medicamentoId.HasValue)
                {
                    query = query.Where(l => l.ID_Medicamento == medicamentoId.Value);
                }

                if (categoriaId.HasValue)
                {
                    query = query.Where(l => l.Medicamentos.ID_Categoría == categoriaId.Value);
                }

                if (soloVencidos.HasValue && soloVencidos.Value)
                {
                    query = query.Where(l => l.fecha_vencimiento < DateTime.Now);
                }

                if (soloPorVencer.HasValue && soloPorVencer.Value)
                {
                    query = query.Where(l => l.fecha_vencimiento > DateTime.Now &&
                                            l.fecha_vencimiento <= DateTime.Now.AddDays(30));
                }

                if (soloConStock.HasValue && soloConStock.Value)
                {
                    query = query.Where(l => l.cantidad > 0);
                }

                // Busca por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(l => l.Medicamentos.Nombre.ToLower().Contains(searchValue) ||
                                             l.Medicamentos.Categorias.Nombre.ToLower().Contains(searchValue) ||
                                             l.Medicamentos.Laboratorios.Nombre.ToLower().Contains(searchValue) ||
                                             l.Inventario.Any(i => i.ubicacion.ToLower().Contains(searchValue)));
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var lotes = query
                    .OrderBy(l => l.fecha_vencimiento)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;
                ViewBag.MedicamentoId = medicamentoId;
                ViewBag.CategoriaId = categoriaId;
                ViewBag.SoloVencidos = soloVencidos;
                ViewBag.SoloPorVencer = soloPorVencer;
                ViewBag.SoloConStock = soloConStock;

                // Estadísticas
                ViewBag.TotalUnidades = query.Sum(l => l.cantidad);
                ViewBag.TotalLotes = totalRecords;
                ViewBag.LotesVencidos = query.Count(l => l.fecha_vencimiento < DateTime.Now);
                ViewBag.LotesPorVencer = query.Count(l => l.fecha_vencimiento > DateTime.Now &&
                                                        l.fecha_vencimiento <= DateTime.Now.AddDays(30));

                // Transforma a ViewModel
                var lotesViewModel = lotes.Select(l => new LoteTableViewModel
                {
                    id_Lote = l.id_Lote,
                    NombreMedicamento = l.Medicamentos.Nombre,
                    Categoria = l.Medicamentos.Categorias.Nombre,
                    Laboratorio = l.Medicamentos.Laboratorios.Nombre,
                    cantidad = l.cantidad,
                    fecha_vencimiento = l.fecha_vencimiento,
                    Ubicacion = l.Inventario.FirstOrDefault()?.ubicacion,
                    DiasParaVencer = (int)(l.fecha_vencimiento - DateTime.Now).TotalDays
                }).ToList();

                return View(lotesViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar lotes: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los lotes: " + ex.Message;
                return View(new List<LoteTableViewModel>());
            }
        }

        // GET: Lotes/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Lotes lote = db.Lotes
                .Include(l => l.Medicamentos)
                .Include(l => l.Medicamentos.Categorias)
                .Include(l => l.Medicamentos.Laboratorios)
                .Include(l => l.Inventario)
                .Include(l => l.Movimientos_Inventario)
                .FirstOrDefault(l => l.id_Lote == id);

            if (lote == null)
            {
                return HttpNotFound();
            }

            var viewModel = LoteViewModel.FromEntity(lote);

            // Obtiene movimientos de inventario relacionados con este lote
            var movimientos = db.Movimientos_Inventario
                .Where(m => m.id_Lote == id)
                .OrderByDescending(m => m.fecha)
                .ToList();

            ViewBag.Movimientos = movimientos;

            return View(viewModel);
        }

        // GET: Lotes/Create
        public ActionResult Create(int? medicamentoId = null)
        {
            var viewModel = new LoteViewModel
            {
                fecha_vencimiento = DateTime.Now.AddYears(1) // Por defecto 1 año
            };

            if (medicamentoId.HasValue)
            {
                viewModel.ID_Medicamento = medicamentoId.Value;
                ViewBag.ID_Medicamento = new SelectList(db.Medicamentos
                    .Where(m => m.ID_Medicamento == medicamentoId && m.estado == "Activo"),
                    "ID_Medicamento", "Nombre", medicamentoId);
                ViewBag.ReturnToMedicamento = true;
            }
            else
            {
                ViewBag.ID_Medicamento = new SelectList(db.Medicamentos
                    .Where(m => m.estado == "Activo")
                    .OrderBy(m => m.Nombre),
                    "ID_Medicamento", "Nombre");
                ViewBag.ReturnToMedicamento = false;
            }

            return View(viewModel);
        }

        // POST: Lotes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LoteViewModel viewModel, bool returnToMedicamento = false, string ubicacion = null)
        {
            if (ModelState.IsValid)
            {
                var lote = viewModel.ToEntity();

                db.Lotes.Add(lote);
                db.SaveChanges();

                // Si se especificó una ubicación, crea registro en Inventario
                if (!string.IsNullOrWhiteSpace(ubicacion))
                {
                    var inventario = new Inventario
                    {
                        ID_Lote = lote.id_Lote,
                        ubicacion = ubicacion
                    };

                    db.Inventario.Add(inventario);
                }

                // Registra movimiento de inventario
                Movimientos_Inventario movimiento = new Movimientos_Inventario
                {
                    id_medicamento = lote.ID_Medicamento,
                    id_Lote = lote.id_Lote,
                    tipo = "Entrada",
                    cantidad = lote.cantidad,
                    fecha = DateTime.Now
                };

                db.Movimientos_Inventario.Add(movimiento);
                db.SaveChanges();

                TempData["Message"] = "Lote creado correctamente";
                TempData["MessageType"] = "success";

                if (returnToMedicamento)
                {
                    return RedirectToAction("Inventario", "Medicamentos", new { id = lote.ID_Medicamento });
                }

                return RedirectToAction("Index");
            }

            if (returnToMedicamento)
            {
                ViewBag.ID_Medicamento = new SelectList(db.Medicamentos
                    .Where(m => m.ID_Medicamento == viewModel.ID_Medicamento && m.estado == "Activo"),
                    "ID_Medicamento", "Nombre", viewModel.ID_Medicamento);
                ViewBag.ReturnToMedicamento = true;
            }
            else
            {
                ViewBag.ID_Medicamento = new SelectList(db.Medicamentos
                    .Where(m => m.estado == "Activo"),
                    "ID_Medicamento", "Nombre", viewModel.ID_Medicamento);
                ViewBag.ReturnToMedicamento = false;
            }

            return View(viewModel);
        }

        // GET: Lotes/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Lotes lote = db.Lotes
                .Include(l => l.Inventario)
                .FirstOrDefault(l => l.id_Lote == id);

            if (lote == null)
            {
                return HttpNotFound();
            }

            var viewModel = LoteViewModel.FromEntity(lote);

            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos
                .Where(m => m.estado == "Activo"),
                "ID_Medicamento", "Nombre", lote.ID_Medicamento);

            ViewBag.Ubicacion = lote.Inventario.FirstOrDefault()?.ubicacion;

            return View(viewModel);
        }

        // POST: Lotes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LoteViewModel viewModel, string ubicacion = null)
        {
            if (ModelState.IsValid)
            {
                // Obtiene el lote original para comparar cambios
                var loteOriginal = db.Lotes
                    .AsNoTracking()
                    .FirstOrDefault(l => l.id_Lote == viewModel.id_Lote);

                int diferenciaStock = viewModel.cantidad - loteOriginal.cantidad;

                // Actualiza el lote
                var lote = viewModel.ToEntity();
                db.Entry(lote).State = EntityState.Modified;

                // Actualiza o crea el registro de inventario
                var inventario = db.Inventario.FirstOrDefault(i => i.ID_Lote == lote.id_Lote);
                if (inventario != null)
                {
                    if (!string.IsNullOrWhiteSpace(ubicacion))
                    {
                        inventario.ubicacion = ubicacion;
                        db.Entry(inventario).State = EntityState.Modified;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(ubicacion))
                {
                    // Crea nuevo registro de inventario
                    db.Inventario.Add(new Inventario
                    {
                        ID_Lote = lote.id_Lote,
                        ubicacion = ubicacion
                    });
                }

                // Si hay diferencia en la cantidad, registra movimiento
                if (diferenciaStock != 0)
                {
                    Movimientos_Inventario movimiento = new Movimientos_Inventario
                    {
                        id_medicamento = lote.ID_Medicamento,
                        id_Lote = lote.id_Lote,
                        tipo = diferenciaStock > 0 ? "Ajuste Entrada" : "Ajuste Salida",
                        cantidad = Math.Abs(diferenciaStock),
                        fecha = DateTime.Now
                    };

                    db.Movimientos_Inventario.Add(movimiento);
                }

                db.SaveChanges();

                TempData["Message"] = "Lote actualizado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos
                .Where(m => m.estado == "Activo"),
                "ID_Medicamento", "Nombre", viewModel.ID_Medicamento);

            ViewBag.Ubicacion = ubicacion;

            return View(viewModel);
        }

        // GET: Lotes/LotesVencidos
        public ActionResult LotesVencidos()
        {
            return View();
        }

        // GET: Lotes/PocoStock
        public ActionResult PocoStock()
        {
            // Medicamentos con poco stock (menos de 10 unidades en total sumando todos sus lotes)
            var medicamentosPoco = db.Medicamentos
                .Where(m => m.estado == "Activo")
                .Select(m => new {
                    Medicamento = m,
                    StockTotal = m.Lotes.Sum(l => l.cantidad)
                })
                .Where(x => x.StockTotal < 10)
                .OrderBy(x => x.StockTotal)
                .ToList();

            var viewModels = medicamentosPoco
                .Select(x => MedicamentoViewModel.FromEntity(x.Medicamento))
                .ToList();

            return View(viewModels);
        }

        // GET: Lotes/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Lotes lote = db.Lotes
                .Include(l => l.Medicamentos)
                .Include(l => l.Medicamentos.Categorias)
                .Include(l => l.Medicamentos.Laboratorios)
                .Include(l => l.Inventario)
                .Include(l => l.Movimientos_Inventario)
                .FirstOrDefault(l => l.id_Lote == id);

            if (lote == null)
            {
                return HttpNotFound();
            }

            // Verifica si tiene movimientos asociados
            if (lote.Movimientos_Inventario.Any())
            {
                ViewBag.TieneMovimientos = true;
                ViewBag.TotalMovimientos = lote.Movimientos_Inventario.Count;
            }
            else
            {
                ViewBag.TieneMovimientos = false;
            }

            var viewModel = LoteViewModel.FromEntity(lote);
            return View(viewModel);
        }

        // POST: Lotes/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Lotes lote = db.Lotes
                .Include(l => l.Inventario)
                .Include(l => l.Movimientos_Inventario)
                .FirstOrDefault(l => l.id_Lote == id);

            // Verifica si tiene movimientos asociados
            if (lote.Movimientos_Inventario.Any())
            {
                TempData["Message"] = "No se puede eliminar el lote porque tiene movimientos asociados";
                TempData["MessageType"] = "error";
                return RedirectToAction("Delete", new { id = id });
            }

            // Elimina registros de inventario relacionados
            foreach (var inventario in lote.Inventario.ToList())
            {
                db.Inventario.Remove(inventario);
            }

            // Elimina el lote
            db.Lotes.Remove(lote);
            db.SaveChanges();

            TempData["Message"] = "Lote eliminado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
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