using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class VentaController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            var ventas = db.Facturas.Include(f => f.Clientes).ToList();
            var viewModel = ventas.Select(VentaViewModel.FromEntity).ToList();
            return View(viewModel);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var factura = db.Facturas.Include(f => f.Detalles_Factura.Select(d => d.Medicamentos))
                                     .FirstOrDefault(f => f.id_Factura == id);

            if (factura == null)
                return HttpNotFound();

            var viewModel = VentaViewModel.FromEntity(factura);
            return View(viewModel);
        }

        public ActionResult Create()
        {
            ViewBag.ID_Cliente = new SelectList(db.Clientes.Include(c => c.Personas), "ID_Cliente", "Personas.Nombre");
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre");
            return View(new VentaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VentaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var factura = model.ToEntity();
                db.Facturas.Add(factura);
                db.SaveChanges();

                foreach (var detalle in model.Detalles)
                {
                    var detalleEntity = detalle.ToEntity();
                    detalleEntity.id_Factura = factura.id_Factura;
                    db.Detalles_Factura.Add(detalleEntity);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_Cliente = new SelectList(db.Clientes.Include(c => c.Personas), "ID_Cliente", "Personas.Nombre", model.ID_Cliente);
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre");
            return View(model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var factura = db.Facturas.Include(f => f.Detalles_Factura).FirstOrDefault(f => f.id_Factura == id);

            if (factura == null)
                return HttpNotFound();

            var viewModel = VentaViewModel.FromEntity(factura);
            ViewBag.ID_Cliente = new SelectList(db.Clientes.Include(c => c.Personas), "ID_Cliente", "Personas.Nombre", viewModel.ID_Cliente);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VentaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var factura = db.Facturas.Include(f => f.Detalles_Factura).FirstOrDefault(f => f.id_Factura == model.id_Factura);
                if (factura == null)
                    return HttpNotFound();

                factura.ID_Cliente = model.ID_Cliente;
                factura.fecha = model.fecha;
                factura.total = model.total;
                factura.estado = model.estado;

                db.Entry(factura).State = EntityState.Modified;

                // Eliminar detalles antiguos y agregar los nuevos
                db.Detalles_Factura.RemoveRange(factura.Detalles_Factura);
                foreach (var detalle in model.Detalles)
                {
                    var detalleEntity = detalle.ToEntity();
                    detalleEntity.id_Factura = factura.id_Factura;
                    db.Detalles_Factura.Add(detalleEntity);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ID_Cliente = new SelectList(db.Clientes.Include(c => c.Personas), "ID_Cliente", "Personas.Nombre", model.ID_Cliente);
            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var factura = db.Facturas.Include(f => f.Detalles_Factura.Select(d => d.Medicamentos))
                                     .FirstOrDefault(f => f.id_Factura == id);

            if (factura == null)
                return HttpNotFound();

            var viewModel = VentaViewModel.FromEntity(factura);
            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var factura = db.Facturas.Include(f => f.Detalles_Factura).FirstOrDefault(f => f.id_Factura == id);
            if (factura == null)
                return HttpNotFound();

            db.Detalles_Factura.RemoveRange(factura.Detalles_Factura);
            db.Facturas.Remove(factura);
            db.SaveChanges();

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
