using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using System.Data.Entity;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class ClienteController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(ClienteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entities = model.ToEntities();
                db.Personas.Add(entities.Item1);
                db.Clientes.Add(entities.Item2);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var cliente = db.Clientes.Find(id);
            if (cliente == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var model = ClienteViewModel.FromEntity(cliente);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ClienteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var entities = model.ToEntities();
                db.Entry(entities.Item1).State = EntityState.Modified;
                db.Entry(entities.Item2).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var cliente = db.Clientes.Include(c => c.Personas).FirstOrDefault(c => c.id_cliente == id);
            if (cliente == null)
                return HttpNotFound();

            return View(ClienteViewModel.FromEntity(cliente));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarDelete(int id)
        {
            var cliente = db.Clientes.Include(c => c.Personas).FirstOrDefault(c => c.id_cliente == id);
            if (cliente != null)
            {
                db.Personas.Remove(cliente.Personas);
                db.Clientes.Remove(cliente);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
