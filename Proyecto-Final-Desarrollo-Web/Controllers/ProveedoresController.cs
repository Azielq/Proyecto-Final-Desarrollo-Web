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
    public class ProveedoresController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Proveedores
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", bool? soloActivos = null)
        {
            try
            {
                // Consulta base
                var query = db.Proveedores
                    .Include(p => p.Compras_Farmacia)
                    .AsQueryable();

                // Aplica filtros
                if (soloActivos.HasValue && soloActivos.Value)
                {
                    query = query.Where(p => p.activo);
                }

                // Busca por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(p => p.Nombre.ToLower().Contains(searchValue) ||
                                            p.Correo.ToLower().Contains(searchValue) ||
                                            p.Telefono.Contains(searchValue) ||
                                            p.direccion.ToLower().Contains(searchValue));
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var proveedores = query
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;
                ViewBag.SoloActivos = soloActivos;

                // Transforma a ViewModel
                var proveedoresViewModel = proveedores.Select(p => new ProveedorTableViewModel
                {
                    Pk_Proveedor = p.Pk_Proveedor,
                    Nombre = p.Nombre,
                    Correo = p.Correo,
                    Telefono = p.Telefono,
                    direccion = p.direccion,
                    activo = p.activo,
                    TotalCompras = p.Compras_Farmacia.Sum(c => c.total),
                    UltimaCompra = p.Compras_Farmacia.OrderByDescending(c => c.fecha).Select(c => c.fecha).FirstOrDefault(),
                    NumeroCompras = p.Compras_Farmacia.Count
                }).ToList();

                return View(proveedoresViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar proveedores: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los proveedores: " + ex.Message;
                return View(new List<ProveedorTableViewModel>());
            }
        }

        // GET: Proveedores/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Proveedores proveedor = db.Proveedores
                .Include(p => p.Compras_Farmacia)
                .FirstOrDefault(p => p.Pk_Proveedor == id);

            if (proveedor == null)
            {
                return HttpNotFound();
            }

            // Obtiene las compras del proveedor ordenadas por fecha descendente
            var compras = db.Compras_Farmacia
                .Include(c => c.Detalles_Compras_Farmacia)
                .Where(c => c.ID_proveedor == id)
                .OrderByDescending(c => c.fecha)
                .Take(10)
                .ToList();

            ViewBag.Compras = compras;

            // Estadísticas de compras
            decimal totalCompras = proveedor.Compras_Farmacia.Sum(c => c.total);
            int numeroCompras = proveedor.Compras_Farmacia.Count;
            DateTime? ultimaCompra = proveedor.Compras_Farmacia
                .OrderByDescending(c => c.fecha)
                .Select(c => c.fecha)
                .FirstOrDefault();

            // Mapea la entidad a ProveedorTableViewModel
            var viewModel = new ProveedorTableViewModel
            {
                Pk_Proveedor = proveedor.Pk_Proveedor,
                Nombre = proveedor.Nombre,
                Correo = proveedor.Correo,
                Telefono = proveedor.Telefono,
                direccion = proveedor.direccion,
                activo = proveedor.activo,
                TotalCompras = totalCompras,
                NumeroCompras = numeroCompras,
                UltimaCompra = ultimaCompra
            };

            return View(viewModel);
        }


        // GET: Proveedores/Create
        public ActionResult Create()
        {
            var proveedor = new Proveedores
            {
                activo = true
            };

            return View(proveedor);
        }

        // POST: Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Pk_Proveedor,Nombre,Correo,Telefono,direccion,activo")] Proveedores proveedor)
        {
            if (ModelState.IsValid)
            {
                db.Proveedores.Add(proveedor);
                db.SaveChanges();

                TempData["Message"] = "Proveedor creado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(proveedor);
        }

        // GET: Proveedores/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Proveedores proveedor = db.Proveedores.Find(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }

            return View(proveedor);
        }

        // POST: Proveedores/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Pk_Proveedor,Nombre,Correo,Telefono,direccion,activo")] Proveedores proveedor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(proveedor).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Proveedor actualizado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(proveedor);
        }

        // GET: Proveedores/ComprasProveedor
        public ActionResult ComprasProveedor(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Proveedores proveedor = db.Proveedores
                .Include(p => p.Compras_Farmacia)
                .FirstOrDefault(p => p.Pk_Proveedor == id);

            if (proveedor == null)
            {
                return HttpNotFound();
            }

            // Esto obtiene las compras del proveedor ordenadas por fecha descendente
            var compras = db.Compras_Farmacia
                .Include(c => c.Detalles_Compras_Farmacia)
                .Where(c => c.ID_proveedor == id)
                .OrderByDescending(c => c.fecha)
                .ToList();

            ViewBag.Compras = compras;
            ViewBag.Proveedor = proveedor;

            return View(compras);
        }

        // GET: Proveedores/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Proveedores proveedor = db.Proveedores
                .Include(p => p.Compras_Farmacia)
                .FirstOrDefault(p => p.Pk_Proveedor == id);

            if (proveedor == null)
            {
                return HttpNotFound();
            }

            // Verifica si tiene compras asociadas
            if (proveedor.Compras_Farmacia.Any())
            {
                ViewBag.TieneCompras = true;
                ViewBag.TotalCompras = proveedor.Compras_Farmacia.Count;
            }
            else
            {
                ViewBag.TieneCompras = false;
            }

            return View(proveedor);
        }

        // POST: Proveedores/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Proveedores proveedor = db.Proveedores
                .Include(p => p.Compras_Farmacia)
                .FirstOrDefault(p => p.Pk_Proveedor == id);

            // Verifica si tiene compras asociadas
            if (proveedor.Compras_Farmacia.Any())
            {
                TempData["Message"] = "No se puede eliminar el proveedor porque tiene compras asociadas";
                TempData["MessageType"] = "error";
                return RedirectToAction("Delete", new { id = id });
            }

            db.Proveedores.Remove(proveedor);
            db.SaveChanges();

            TempData["Message"] = "Proveedor eliminado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // POST: Proveedores/ToggleActivo
        [HttpPost]
        public ActionResult ToggleActivo(int id)
        {
            var proveedor = db.Proveedores.Find(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }

            proveedor.activo = !proveedor.activo;
            db.Entry(proveedor).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { success = true, activo = proveedor.activo });
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