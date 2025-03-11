using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class CompraController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult ListarCompras(CompraTableRequest request)
        {
            var query = db.Compras_Farmacia.AsQueryable();

            // filtros
            if (request.FechaInicio.HasValue)
                query = query.Where(c => c.fecha >= request.FechaInicio);

            if (request.FechaFin.HasValue)
                query = query.Where(c => c.fecha <= request.FechaFin);

            if (!string.IsNullOrEmpty(request.Estado))
                query = query.Where(c => c.estado == request.Estado);

            if (request.ProveedorId.HasValue)
                query = query.Where(c => c.ID_proveedor == request.ProveedorId);

            int totalRegistros = query.Count();

            // Ordenamiento
            query = query.OrderByDescending(c => c.fecha);

            var compras = query
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            var data = compras.Select(c => new CompraTableViewModel
            {
                id_compra = c.id_compra,
                fecha = c.fecha,
                NombreProveedor = c.Proveedores?.Nombre,
                total = c.total,
                estado = c.estado,
                NumeroProductos = c.Detalles_Compras_Farmacia?.Sum(d => d.Cantidad) ?? 0
            }).ToList();

            return Json(new CompraTableResponse
            {
                draw = request.Start / request.Length + 1,
                recordsTotal = totalRegistros,
                recordsFiltered = totalRegistros,
                data = data,
                TotalMonto = data.Sum(c => c.total)
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            ViewBag.Proveedores = new SelectList(db.Proveedores, "id_proveedor", "Nombre");
            return View(new CompraViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompraViewModel model)
        {
            if (ModelState.IsValid)
            {
                var compra = model.ToEntity();
                db.Compras_Farmacia.Add(compra);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Proveedores = new SelectList(db.Proveedores, "id_proveedor", "Nombre", model.ID_proveedor);
            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var compra = db.Compras_Farmacia.Find(id);
            if (compra == null)
                return HttpNotFound();

            var model = CompraViewModel.FromEntity(compra);
            ViewBag.Proveedores = new SelectList(db.Proveedores, "id_proveedor", "Nombre", model.ID_proveedor);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CompraViewModel model)
        {
            if (ModelState.IsValid)
            {
                var compra = db.Compras_Farmacia.Find(model.id_compra);
                if (compra == null)
                    return HttpNotFound();

                compra.ID_proveedor = model.ID_proveedor;
                compra.fecha = model.fecha;
                compra.estado = model.estado;
                compra.total = model.total;

                db.Entry(compra).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Proveedores = new SelectList(db.Proveedores, "id_proveedor", "Nombre", model.ID_proveedor);
            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var compra = db.Compras_Farmacia.Find(id);
            if (compra == null)
                return HttpNotFound();

            return View(CompraViewModel.FromEntity(compra));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var compra = db.Compras_Farmacia.Find(id);
            if (compra != null)
            {
                db.Compras_Farmacia.Remove(compra);
                db.SaveChanges();
            }
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
