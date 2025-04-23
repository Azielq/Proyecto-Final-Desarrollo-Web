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
using Proyecto_Final_Desarrollo_Web.Filters;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    [AuthorizeRoles("Administrador", "Supervisor")]

    public class LotesController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Lotes
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", int? productoId = null, int? categoriaId = null, bool? soloVencidos = null, bool? soloPorVencer = null, bool? soloConStock = null)
        {
            try
            {
                // Calcula las fechas FUERA de las consultas LINQ
                DateTime fechaActual = DateTime.Now;
                DateTime fechaLimite = fechaActual.AddDays(30);

                // Carga listas para filtros
                ViewBag.Productos = new SelectList(db.Productos.Where(p => p.estado == "Activo").OrderBy(p => p.Nombre), "ID_Producto", "Nombre");
                ViewBag.Categorias = new SelectList(db.Categorias.OrderBy(c => c.Nombre), "ID_Categoría", "Nombre");

                // Consulta base
                var query = db.Lotes
                    .Include(l => l.Productos)
                    .Include(l => l.Productos.Categorias)
                    //.Include(l => l.Productos.Laboratorios) // Eliminada Laboratorios
                    .Include(l => l.Inventario)
                    .AsQueryable();

                // Aplica filtros
                if (productoId.HasValue)
                {
                    query = query.Where(l => l.ID_Producto == productoId.Value);
                }

                if (categoriaId.HasValue)
                {
                    query = query.Where(l => l.Productos.ID_Categoría == categoriaId.Value);
                }

                if (soloVencidos.HasValue && soloVencidos.Value)
                {
                    query = query.Where(l => l.fecha_vencimiento < fechaActual);
                }

                if (soloPorVencer.HasValue && soloPorVencer.Value)
                {
                    // Usa las variables calculadas previamente
                    query = query.Where(l => l.fecha_vencimiento > fechaActual &&
                                            l.fecha_vencimiento <= fechaLimite);
                }

                if (soloConStock.HasValue && soloConStock.Value)
                {
                    query = query.Where(l => l.cantidad > 0);
                }

                // Busca por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(l => l.Productos.Nombre.ToLower().Contains(searchValue) ||
                                             l.Productos.Categorias.Nombre.ToLower().Contains(searchValue) ||
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
                ViewBag.ProductoId = productoId;
                ViewBag.CategoriaId = categoriaId;
                ViewBag.SoloVencidos = soloVencidos;
                ViewBag.SoloPorVencer = soloPorVencer;
                ViewBag.SoloConStock = soloConStock;

                // Estadísticas
                ViewBag.TotalUnidades = query.Sum(l => l.cantidad);
                ViewBag.TotalLotes = totalRecords;
                ViewBag.LotesVencidos = query.Count(l => l.fecha_vencimiento < fechaActual);
                ViewBag.LotesPorVencer = query.Count(l => l.fecha_vencimiento > fechaActual &&
                                                        l.fecha_vencimiento <= fechaLimite);

                // Transforma a ViewModel
                var lotesViewModel = lotes.Select(l => new LoteTableViewModel
                {
                    id_Lote = l.id_Lote,
                    NombreProducto = l.Productos.Nombre,
                    Categoria = l.Productos.Categorias.Nombre,
                    // Se elimina Laboratorio ya que la entidad fue removida
                    cantidad = l.cantidad,
                    fecha_vencimiento = l.fecha_vencimiento,
                    Ubicacion = l.Inventario.FirstOrDefault()?.ubicacion,
                    DiasParaVencer = (int)(l.fecha_vencimiento - fechaActual).TotalDays
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
                .Include(l => l.Productos)
                .Include(l => l.Productos.Categorias)
                //.Include(l => l.Productos.Laboratorios) // Eliminada Laboratorios
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
        public ActionResult Create(int? productoId = null)
        {
            var viewModel = new LoteViewModel
            {
                fecha_vencimiento = DateTime.Now.AddYears(1) // Por defecto 1 año
            };

            if (productoId.HasValue)
            {
                viewModel.ID_Producto = productoId.Value;
                ViewBag.ID_Producto = new SelectList(db.Productos
                    .Where(p => p.ID_Producto == productoId && p.estado == "Activo"),
                    "ID_Producto", "Nombre", productoId);
                ViewBag.ReturnToProducto = true;
            }
            else
            {
                ViewBag.ID_Producto = new SelectList(db.Productos
                    .Where(p => p.estado == "Activo")
                    .OrderBy(p => p.Nombre),
                    "ID_Producto", "Nombre");
                ViewBag.ReturnToProducto = false;
            }

            return View(viewModel);
        }

        // POST: Lotes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LoteViewModel viewModel, bool returnToProducto = false, string ubicacion = null)
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
                    id_Producto = lote.ID_Producto,
                    id_Lote = lote.id_Lote,
                    tipo = "Entrada",
                    cantidad = lote.cantidad,
                    fecha = DateTime.Now
                };

                db.Movimientos_Inventario.Add(movimiento);
                db.SaveChanges();

                TempData["Message"] = "Lote creado correctamente";
                TempData["MessageType"] = "success";

                if (returnToProducto)
                {
                    return RedirectToAction("Inventario", "Productos", new { id = lote.ID_Producto });
                }

                return RedirectToAction("Index");
            }

            if (returnToProducto)
            {
                ViewBag.ID_Producto = new SelectList(db.Productos
                    .Where(p => p.ID_Producto == viewModel.ID_Producto && p.estado == "Activo"),
                    "ID_Producto", "Nombre", viewModel.ID_Producto);
                ViewBag.ReturnToProducto = true;
            }
            else
            {
                ViewBag.ID_Producto = new SelectList(db.Productos
                    .Where(p => p.estado == "Activo"),
                    "ID_Producto", "Nombre", viewModel.ID_Producto);
                ViewBag.ReturnToProducto = false;
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

            ViewBag.ID_Producto = new SelectList(db.Productos
                .Where(p => p.estado == "Activo"),
                "ID_Producto", "Nombre", lote.ID_Producto);

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
                        id_Producto = lote.ID_Producto,
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

            ViewBag.ID_Producto = new SelectList(db.Productos
                .Where(p => p.estado == "Activo"),
                "ID_Producto", "Nombre", viewModel.ID_Producto);

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
            // Productos con poco stock (menos de 10 unidades en total sumando todos sus lotes)
            var productosPoco = db.Productos
                .Where(p => p.estado == "Activo")
                .Select(p => new {
                    Producto = p,
                    StockTotal = p.Lotes.Sum(l => l.cantidad)
                })
                .Where(x => x.StockTotal < 10)
                .OrderBy(x => x.StockTotal)
                .ToList();

            var viewModels = productosPoco
                .Select(x => ProductoViewModel.FromEntity(x.Producto))
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
                .Include(l => l.Productos)
                .Include(l => l.Productos.Categorias)
                //.Include(l => l.Productos.Laboratorios) // Eliminada Laboratorios
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