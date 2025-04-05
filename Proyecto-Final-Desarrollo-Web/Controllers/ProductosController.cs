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
    public class ProductosController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Productos
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", int? categoriaId = null, string estado = "Activo")
        {
            try
            {
                // Carga listas para filtros
                ViewBag.Categorias = new SelectList(db.Categorias.OrderBy(c => c.Nombre), "ID_Categoría", "Nombre");
                // Laboratorios ya no se usa

                // Consulta base
                var query = db.Productos
                    .Include(p => p.Categorias)
                    //.Include(p => p.Laboratorios) // Eliminada
                    .Include(p => p.Lotes)
                    .AsQueryable();

                // Aplica filtros
                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(p => p.estado == estado);
                }

                if (categoriaId.HasValue)
                {
                    query = query.Where(p => p.ID_Categoría == categoriaId.Value);
                }

                // Busca por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(p => p.Nombre.ToLower().Contains(searchValue));
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var productos = query
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CategoriaId = categoriaId;
                ViewBag.Estado = estado;

                // Transforma a ViewModel
                var productosViewModel = productos.Select(p => ProductoViewModel.FromEntity(p)).ToList();

                return View(productosViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar productos: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los productos: " + ex.Message;
                return View(new List<ProductoViewModel>());
            }
        }

        // GET: Productos/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Productos producto = db.Productos
                .Include(p => p.Categorias)
                //.Include(p => p.Laboratorios) // Eliminada
                .Include(p => p.Lotes)
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            var viewModel = ProductoViewModel.FromEntity(producto);

            return View(viewModel);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre");
            return View(new ProductoViewModel());
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var producto = viewModel.ToEntity();
                producto.estado = "Activo";

                db.Productos.Add(producto);
                db.SaveChanges();

                TempData["Message"] = "Producto creado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", viewModel.ID_Categoría);
            return View(viewModel);
        }

        // GET: Productos/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Productos producto = db.Productos.Find(id);
            if (producto == null)
            {
                return HttpNotFound();
            }

            var viewModel = ProductoViewModel.FromEntity(producto);

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", producto.ID_Categoría);

            return View(viewModel);
        }

        // POST: Productos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var producto = viewModel.ToEntity();

                db.Entry(producto).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Producto actualizado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", viewModel.ID_Categoría);

            return View(viewModel);
        }

        // GET: Productos/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Productos producto = db.Productos
                .Include(p => p.Categorias)
                //.Include(p => p.Laboratorios) // Eliminada
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            // Verifica si tiene lotes asociados
            if (producto.Lotes.Any())
            {
                ViewBag.TieneLotes = true;
                ViewBag.TotalLotes = producto.Lotes.Count;
            }
            else
            {
                ViewBag.TieneLotes = false;
            }

            var viewModel = ProductoViewModel.FromEntity(producto);
            return View(viewModel);
        }

        // POST: Productos/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Productos producto = db.Productos
                .Include(p => p.Lotes)
                .FirstOrDefault(p => p.ID_Producto == id);

            // En lugar de eliminar físicamente, cambia el estado
            producto.estado = "Inactivo";
            db.Entry(producto).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Message"] = "Producto eliminado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Productos/Inventario
        public ActionResult Inventario(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var producto = db.Productos
                .Include(p => p.Categorias)
                //.Include(p => p.Laboratorios) // Eliminada
                .Include(p => p.Lotes)
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            var viewModel = ProductoViewModel.FromEntity(producto);

            // Obtiene los lotes del producto
            var lotes = db.Lotes
                .Where(l => l.ID_Producto == id)
                .ToList()
                .Select(l => LoteViewModel.FromEntity(l))
                .ToList();

            ViewBag.Lotes = lotes;

            return View(viewModel);
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
