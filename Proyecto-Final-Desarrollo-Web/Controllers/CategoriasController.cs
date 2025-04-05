using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Helpers;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class CategoriasController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Categorías
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            try
            {
                // Consulta base: se incluye la relación con Productos
                var query = db.Categorias
                    .Include(c => c.Productos)
                    .AsQueryable();

                // Búsqueda por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(c => c.Nombre.ToLower().Contains(searchValue) ||
                                           c.Descripcion.ToLower().Contains(searchValue));
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var categorias = query
                    .OrderBy(c => c.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;

                return View(categorias);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar categorías: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar las categorías: " + ex.Message;
                return View(new List<Categorias>());
            }
        }

        [HttpPost]
        public ActionResult GetCategorias(CategoriaTableRequest request)
        {
            var response = new CategoriaTableResponse
            {
                draw = request.Start
            };

            // Consulta base: se incluye la relación con Productos
            var query = db.Categorias
                .Include(c => c.Productos)
                .AsQueryable();

            // Búsqueda por término
            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var searchValue = request.SearchValue.ToLower();
                query = query.Where(c => c.Nombre.ToLower().Contains(searchValue) ||
                                       c.Descripcion.ToLower().Contains(searchValue));
            }

            // Total de registros sin filtrar
            response.recordsTotal = db.Categorias.Count();

            // Total después de filtrar
            response.recordsFiltered = query.Count();

            // Ordenamiento según columna
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                if (request.SortDirection.ToLower() == "asc")
                {
                    switch (request.SortColumn)
                    {
                        case "Nombre":
                            query = query.OrderBy(c => c.Nombre);
                            break;
                        case "Descripcion":
                            query = query.OrderBy(c => c.Descripcion);
                            break;
                        case "TotalProductos":
                            query = query.OrderBy(c => c.Productos.Count);
                            break;
                        default:
                            query = query.OrderBy(c => c.Nombre);
                            break;
                    }
                }
                else
                {
                    switch (request.SortColumn)
                    {
                        case "Nombre":
                            query = query.OrderByDescending(c => c.Nombre);
                            break;
                        case "Descripcion":
                            query = query.OrderByDescending(c => c.Descripcion);
                            break;
                        case "TotalProductos":
                            query = query.OrderByDescending(c => c.Productos.Count);
                            break;
                        default:
                            query = query.OrderByDescending(c => c.Nombre);
                            break;
                    }
                }
            }
            else
            {
                query = query.OrderBy(c => c.Nombre);
            }

            // Paginación
            query = query.Skip(request.Start).Take(request.Length);

            // Conversión a ViewModel
            response.data = query.ToList().Select(c => new CategoriaTableViewModel
            {
                ID_Categoría = c.ID_Categoría,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                TotalProductos = c.Productos.Count(m => m.estado == "Activo")
            }).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // GET: Categorías/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Se incluye la relación con Productos
            Categorias categoria = db.Categorias
                .Include(c => c.Productos)
                .FirstOrDefault(c => c.ID_Categoría == id);

            if (categoria == null)
            {
                return HttpNotFound();
            }

            // Se obtienen los productos activos de esta categoría
            var productos = db.Productos
                .Where(p => p.ID_Categoría == id && p.estado == "Activo")
                .ToList();

            ViewBag.Productos = productos;
            ViewBag.TotalProductos = productos.Count;

            return View(categoria);
        }

        // GET: Categorías/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categorías/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Categoría,Nombre,Descripcion")] Categorias categoria)
        {
            if (ModelState.IsValid)
            {
                db.Categorias.Add(categoria);
                db.SaveChanges();

                TempData["Message"] = "Categoría creada correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        // GET: Categorías/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Categorias categoria = db.Categorias.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }

            return View(categoria);
        }

        // POST: Categorías/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Categoría,Nombre,Descripcion")] Categorias categoria)
        {
            if (ModelState.IsValid)
            {
                db.Entry(categoria).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Categoría actualizada correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(categoria);
        }

        // GET: Categorías/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Categorias categoria = db.Categorias
                .Include(c => c.Productos)
                .FirstOrDefault(c => c.ID_Categoría == id);

            if (categoria == null)
            {
                return HttpNotFound();
            }

            // Verifica si tiene productos asociados
            if (categoria.Productos.Any())
            {
                ViewBag.TieneProductos = true;
                ViewBag.TotalProductos = categoria.Productos.Count;
            }
            else
            {
                ViewBag.TieneProductos = false;
            }

            return View(categoria);
        }

        // POST: Categorías/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Categorias categoria = db.Categorias
                .Include(c => c.Productos)
                .FirstOrDefault(c => c.ID_Categoría == id);

            // Verifica si tiene productos asociados
            if (categoria.Productos.Any())
            {
                TempData["Message"] = "No se puede eliminar la categoría porque tiene productos asociados";
                TempData["MessageType"] = "error";
                return RedirectToAction("Delete", new { id = id });
            }

            db.Categorias.Remove(categoria);
            db.SaveChanges();

            TempData["Message"] = "Categoría eliminada correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Categorías/ProductosPorCategoria
        public ActionResult ProductosPorCategoria()
        {
            var categorias = db.Categorias
                .Include(c => c.Productos)
                .ToList()
                .Select(c => new
                {
                    Categoria = c.Nombre,
                    TotalProductos = c.Productos.Count(m => m.estado == "Activo"),
                    ProductosActivos = c.Productos.Where(m => m.estado == "Activo").Count(),
                    ProductosInactivos = c.Productos.Where(m => m.estado != "Activo").Count()
                })
                .OrderByDescending(c => c.TotalProductos)
                .ToList();

            return Json(categorias, JsonRequestBehavior.AllowGet);
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
