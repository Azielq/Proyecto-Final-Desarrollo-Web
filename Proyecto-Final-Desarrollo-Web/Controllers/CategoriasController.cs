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
                // Consulta base
                var query = db.Categorias
                    .Include(c => c.Medicamentos)
                    .AsQueryable();

                // Esto busca por término
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

            // Consulta base
            var query = db.Categorias
                .Include(c => c.Medicamentos)
                .AsQueryable();

            // Esto busca por término
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

            // Ordena
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
                        case "TotalMedicamentos":
                            query = query.OrderBy(c => c.Medicamentos.Count);
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
                        case "TotalMedicamentos":
                            query = query.OrderByDescending(c => c.Medicamentos.Count);
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

            // Esto Convierte y asigna a la respuesta
            response.data = query.ToList().Select(c => new CategoriaTableViewModel
            {
                ID_Categoría = c.ID_Categoría,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion,
                TotalMedicamentos = c.Medicamentos.Count(m => m.estado == "Activo")
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

            Categorias categoria = db.Categorias
                .Include(c => c.Medicamentos)
                .FirstOrDefault(c => c.ID_Categoría == id);

            if (categoria == null)
            {
                return HttpNotFound();
            }

            // Esto obtiene medicamentos activos de esta categoría
            var medicamentos = db.Medicamentos
                .Include(m => m.Laboratorios)
                .Where(m => m.ID_Categoría == id && m.estado == "Activo")
                .ToList();

            ViewBag.Medicamentos = medicamentos;
            ViewBag.TotalMedicamentos = medicamentos.Count;

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
                .Include(c => c.Medicamentos)
                .FirstOrDefault(c => c.ID_Categoría == id);

            if (categoria == null)
            {
                return HttpNotFound();
            }

            // Esto verifica si tiene medicamentos asociados
            if (categoria.Medicamentos.Any())
            {
                ViewBag.TieneMedicamentos = true;
                ViewBag.TotalMedicamentos = categoria.Medicamentos.Count;
            }
            else
            {
                ViewBag.TieneMedicamentos = false;
            }

            return View(categoria);
        }

        // POST: Categorías/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Categorias categoria = db.Categorias
                .Include(c => c.Medicamentos)
                .FirstOrDefault(c => c.ID_Categoría == id);

            // Esto verifica si tiene medicamentos asociados
            if (categoria.Medicamentos.Any())
            {
                TempData["Message"] = "No se puede eliminar la categoría porque tiene medicamentos asociados";
                TempData["MessageType"] = "error";
                return RedirectToAction("Delete", new { id = id });
            }

            db.Categorias.Remove(categoria);
            db.SaveChanges();

            TempData["Message"] = "Categoría eliminada correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Categorías/MedicamentosPorCategoria
        public ActionResult MedicamentosPorCategoria()
        {
            var categorias = db.Categorias
                .Include(c => c.Medicamentos)
                .ToList()
                .Select(c => new
                {
                    Categoria = c.Nombre,
                    TotalMedicamentos = c.Medicamentos.Count(m => m.estado == "Activo"),
                    MedicamentosActivos = c.Medicamentos.Where(m => m.estado == "Activo").Count(),
                    MedicamentosInactivos = c.Medicamentos.Where(m => m.estado != "Activo").Count()
                })
                .OrderByDescending(c => c.TotalMedicamentos)
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