using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.Models.ViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class LaboratoriosController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            var laboratorios = db.Laboratorios.Select(l => new LaboratorioViewModel
            {
                ID_Laboratorio = l.ID_Laboratorio,
                Nombre = l.Nombre,
                Descripcion = l.Descripcion
            }).ToList();

            return View(laboratorios);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(LaboratorioViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Laboratorios.Any(l => l.Nombre == model.Nombre))
                {
                    ModelState.AddModelError("Nombre", "Ya existe un laboratorio con este nombre.");
                    return View(model);
                }

                var laboratorio = new Laboratorio
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion
                };

                db.Laboratorios.Add(laboratorio);

                try
                {
                    db.SaveChanges();
                    TempData["Success"] = "El laboratorio se ha creado exitosamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ocurrió un error al guardar los cambios. Inténtalo de nuevo.");
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var laboratorio = db.Laboratorios.Find(id);
            if (laboratorio == null)
            {
                return HttpNotFound();
            }

            var model = new LaboratorioViewModel
            {
                ID_Laboratorio = laboratorio.ID_Laboratorio,
                Nombre = laboratorio.Nombre,
                Descripcion = laboratorio.Descripcion
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(LaboratorioViewModel model)
        {
            if (ModelState.IsValid)
            {
                var laboratorio = db.Laboratorios.Find(model.ID_Laboratorio);
                if (laboratorio == null)
                {
                    return HttpNotFound();
                }

                if (db.Laboratorios.Any(l => l.Nombre == model.Nombre && l.ID_Laboratorio != model.ID_Laboratorio))
                {
                    ModelState.AddModelError("Nombre", "Ya existe otro laboratorio con este nombre.");
                    return View(model);
                }

                laboratorio.Nombre = model.Nombre;
                laboratorio.Descripcion = model.Descripcion;

                try
                {
                    db.SaveChanges();
                    TempData["Success"] = "El laboratorio se ha actualizado exitosamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ocurrió un error al guardar los cambios. Inténtalo de nuevo.");
                    return View(model);
                }
            }
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var laboratorio = db.Laboratorios.Find(id);
            if (laboratorio == null)
            {
                return HttpNotFound();
            }

            if (db.Medicamentos.Any(m => m.ID_Laboratorio == id))
            {
                TempData["Error"] = "No se puede eliminar el laboratorio porque tiene medicamentos asociados.";
                return RedirectToAction("Index");
            }

            db.Laboratorios.Remove(laboratorio);

            try
            {
                db.SaveChanges();
                TempData["Success"] = "El laboratorio se ha eliminado exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurrió un error al eliminar el laboratorio.";
            }

            return RedirectToAction("Index");
        }
    }
}
