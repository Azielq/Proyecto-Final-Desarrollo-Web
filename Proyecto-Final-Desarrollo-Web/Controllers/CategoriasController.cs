using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Filters;
using Proyecto_Final_Desarrollo_Web.Helpers;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    [AuthorizeRoles("Administrador", "Supervisor")]

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

        // GET: Categorías/GetCategorias (para DataTables)
        [HttpPost]
        public ActionResult GetCategorias(CategoriaTableRequest request)
        {
            var response = new CategoriaTableResponse
            {
                draw = request.Start
            };

            try
            {
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
                    TotalProductos = c.Productos.Count(p => p.estado == "Activo")
                }).ToList();

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetCategorias: {ex.Message}");
                response.data = new List<CategoriaTableViewModel>();
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        // Método de Categorías con corrección para el error de 'codigo'
        [HttpGet]
        public ActionResult GetProductosByCategoria(int id)
        {
            try
            {
                // El problema estaba aquí: estábamos usando 'codigo' en lugar de acceder a las propiedades correctas
                var productos = db.Productos
                    .Where(p => p.ID_Categoría == id)
                    .Select(p => new {
                        ID_Producto = p.ID_Producto,
                        Nombre = p.Nombre,               // <-- Se cambió 'nombre' a 'Nombre' para que coincida con el modelo
                        precio_venta = p.precio_venta,
                        stock = p.Lotes.Sum(l => l.cantidad),
                        estado = p.estado
                    })
                    .ToList();

                return Json(productos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener productos por categoría: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Categorías/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar detalles de categoría: {ex.Message}");
                TempData["Message"] = "Error al cargar detalles de la categoría: " + ex.Message;
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // GET: Categorías/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categorías/Create (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Categoría,Nombre,Descripcion")] Categorias categoria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica si ya existe una categoría con el mismo nombre
                    if (db.Categorias.Any(c => c.Nombre.ToLower() == categoria.Nombre.ToLower()))
                    {
                        return Json(new { success = false, message = "Ya existe una categoría con este nombre" });
                    }

                    db.Categorias.Add(categoria);
                    db.SaveChanges();

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = true, message = "Categoría creada correctamente" });
                    }

                    TempData["Message"] = "Categoría creada correctamente";
                    TempData["MessageType"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al crear categoría: {ex.Message}");

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Error al crear la categoría: " + ex.Message });
                    }

                    ModelState.AddModelError("", "Error al crear la categoría: " + ex.Message);
                }
            }

            if (Request.IsAjaxRequest())
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Error de validación: " + string.Join(", ", errors) });
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

            try
            {
                Categorias categoria = db.Categorias.Find(id);
                if (categoria == null)
                {
                    return HttpNotFound();
                }

                return View(categoria);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar categoría para editar: {ex.Message}");
                TempData["Message"] = "Error al cargar la categoría para editar: " + ex.Message;
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // POST: Categorías/Edit (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Categoría,Nombre,Descripcion")] Categorias categoria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica si ya existe otra categoría con el mismo nombre
                    if (db.Categorias.Any(c => c.Nombre.ToLower() == categoria.Nombre.ToLower() && c.ID_Categoría != categoria.ID_Categoría))
                    {
                        if (Request.IsAjaxRequest())
                        {
                            return Json(new { success = false, message = "Ya existe otra categoría con este nombre" });
                        }

                        ModelState.AddModelError("Nombre", "Ya existe otra categoría con este nombre");
                        return View(categoria);
                    }

                    db.Entry(categoria).State = EntityState.Modified;
                    db.SaveChanges();

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = true, message = "Categoría actualizada correctamente" });
                    }

                    TempData["Message"] = "Categoría actualizada correctamente";
                    TempData["MessageType"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al actualizar categoría: {ex.Message}");

                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Error al actualizar la categoría: " + ex.Message });
                    }

                    ModelState.AddModelError("", "Error al actualizar la categoría: " + ex.Message);
                }
            }

            if (Request.IsAjaxRequest())
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Error de validación: " + string.Join(", ", errors) });
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

            try
            {
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar categoría para eliminar: {ex.Message}");
                TempData["Message"] = "Error al cargar la categoría para eliminar: " + ex.Message;
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // POST: Categorías/Delete (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                Categorias categoria = db.Categorias
                    .Include(c => c.Productos)
                    .FirstOrDefault(c => c.ID_Categoría == id);

                if (categoria == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "La categoría no existe" });
                    }

                    TempData["Message"] = "La categoría no existe";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index");
                }

                // Verifica si tiene productos asociados
                if (categoria.Productos.Any())
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new
                        {
                            success = false,
                            message = "No se puede eliminar la categoría porque tiene productos asociados",
                            productos = categoria.Productos.Count
                        });
                    }

                    TempData["Message"] = "No se puede eliminar la categoría porque tiene productos asociados";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Delete", new { id = id });
                }

                db.Categorias.Remove(categoria);
                db.SaveChanges();

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = "Categoría eliminada correctamente" });
                }

                TempData["Message"] = "Categoría eliminada correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al eliminar categoría: {ex.Message}");

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Error al eliminar la categoría: " + ex.Message });
                }

                TempData["Message"] = "Error al eliminar la categoría: " + ex.Message;
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // GET: Categorías/ProductosPorCategoria (para gráficos)
        public ActionResult ProductosPorCategoria()
        {
            try
            {
                var categorias = db.Categorias
                    .Include(c => c.Productos)
                    .ToList()
                    .Select(c => new
                    {
                        Categoria = c.Nombre,
                        TotalProductos = c.Productos.Count(p => p.estado == "Activo"),
                        ProductosActivos = c.Productos.Where(p => p.estado == "Activo").Count(),
                        ProductosInactivos = c.Productos.Where(p => p.estado != "Activo").Count()
                    })
                    .OrderByDescending(c => c.TotalProductos)
                    .ToList();

                return Json(categorias, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener estadísticas: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Categorías/GetCategoriasList (para selectores)
        public ActionResult GetCategoriasList()
        {
            try
            {
                var categorias = db.Categorias
                    .OrderBy(c => c.Nombre)
                    .Select(c => new { ID = c.ID_Categoría, Nombre = c.Nombre })
                    .ToList();

                return Json(categorias, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener lista de categorías: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
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