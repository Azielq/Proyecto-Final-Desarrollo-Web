using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class ProveedorController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetProveedores(ProveedorTableRequest request)
        {
            // Consulta de proveedores con filtros
            var query = db.Proveedores.AsQueryable();

            if (request.SoloActivos.HasValue)
            {
                query = query.Where(p => p.activo == request.SoloActivos.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where(p => p.Nombre.Contains(request.SearchValue) || p.Correo.Contains(request.SearchValue));
            }

            // Ordenamiento
            if (request.SortColumn == "Nombre")
            {
                query = request.SortDirection == "asc" ? query.OrderBy(p => p.Nombre) : query.OrderByDescending(p => p.Nombre);
            }
            else if (request.SortColumn == "Correo")
            {
                query = request.SortDirection == "asc" ? query.OrderBy(p => p.Correo) : query.OrderByDescending(p => p.Correo);
            }

            // Paginación
            var totalRecords = query.Count();
            var proveedores = query
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            // Convertir a ViewModel
            var proveedoresViewModel = proveedores.Select(p => new ProveedorTableViewModel
            {
                Pk_Proveedor = p.Pk_Proveedor,
                Nombre = p.Nombre,
                Correo = p.Correo,
                Telefono = p.Telefono,
                direccion = p.direccion,
                activo = p.activo,
                TotalCompras = p.Compras_Farmacia.Sum(c => c.MontoTotal), 
                UltimaCompra = p.Compras_Farmacia.OrderByDescending(c => c.FechaCompra).FirstOrDefault()?.FechaCompra,
                NumeroCompras = p.Compras_Farmacia.Count
            }).ToList();

            var response = new ProveedorTableResponse
            {
                draw = request.Start,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = proveedoresViewModel
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id)
        {
            var proveedor = db.Proveedores.Find(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }

            var proveedorViewModel = new ProveedorTableViewModel
            {
                Pk_Proveedor = proveedor.Pk_Proveedor,
                Nombre = proveedor.Nombre,
                Correo = proveedor.Correo,
                Telefono = proveedor.Telefono,
                direccion = proveedor.direccion,
                activo = proveedor.activo,
                TotalCompras = proveedor.Compras_Farmacia.Sum(c => c.MontoTotal),
                UltimaCompra = proveedor.Compras_Farmacia.OrderByDescending(c => c.FechaCompra).FirstOrDefault()?.FechaCompra,
                NumeroCompras = proveedor.Compras_Farmacia.Count
            };

            return View(proveedorViewModel);
        }

        public ActionResult Edit(int id)
        {
            var proveedor = db.Proveedores.Find(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }

            return View(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Proveedores proveedor)
        {
            if (ModelState.IsValid)
            {
                db.Entry(proveedor).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(proveedor);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Proveedores proveedor)
        {
            if (ModelState.IsValid)
            {
                db.Proveedores.Add(proveedor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(proveedor);
        }

        public ActionResult Delete(int id)
        {
            var proveedor = db.Proveedores.Find(id);
            if (proveedor == null)
            {
                return HttpNotFound();
            }

            db.Proveedores.Remove(proveedor);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
