using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.Models.TableViewModels;
using Proyecto_Final_Desarrollo_Web.Models.ViewModels;
using System.Data.Entity;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class ClienteController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            List<ClienteTableViewModel> listaClientes;
            using (var db = new FarmaUEntities())
            {
                listaClientes = db.Clientes
                    .Select(a => new ClienteTableViewModel
                    {
                        ID_Cliente = a.ID_Cliente,
                        Nombre = a.Nombre,
                        Apellido = a.Apellido,
                        Direccion = a.Direccion,
                        Telefono = a.Telefono,
                        Correo = a.Correo,
                        estado = a.estado
                    })
                    .ToList();
            }
            return View(listaClientes);
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
                using (var db = new FarmaUEntities())
                {
                    var cliente = model.ToEntity();
                    db.Clientes.Add(cliente);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            ClienteViewModel model;
            using (var db = new FarmaUEntities())
            {
                var cliente = db.Clientes.Find(id);
                if (cliente == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                model = new ClienteViewModel(cliente);
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ClienteViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new FarmaUEntities())
                {
                    var cliente = model.ToEntity();
                    db.Entry(cliente).State = EntityState.Modified;
                    db.SaveChanges();
                }
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