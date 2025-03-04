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
    public class LoteController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            List<LoteTableViewModel> listaLotes;
            using (var db = new FarmaUEntities())
            {
                listaLotes = db.Lotes
                    .Select(a => new LoteTableViewModel
                    {
                        id_Lote = a.id_Lote,
                        NombreMedicamento = a.Medicamentos.Nombre,
                        Categoria = a.Medicamentos.Categorias.Nombre,
                        Laboratorio = a.Medicamentos.Laboratorios.Nombre,
                        cantidad = a.cantidad,
                        fecha_vencimiento = a.fecha_vencimiento,
                        Ubicacion = a.Inventario.FirstOrDefault().Ubicacion,
                        DiasParaVencer = (int)(a.fecha_vencimiento - DateTime.Now).TotalDays
                    })
                    .ToList();
            }
            return View(listaLotes);
        }

        public ActionResult Create()
        {
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre");
            return View();
        }

        [HttpPost]
        public ActionResult Create(LoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new FarmaUEntities())
                {
                    var lote = model.ToEntity();
                    db.Lotes.Add(lote);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre", model.ID_Medicamento);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            LoteViewModel model;
            using (var db = new FarmaUEntities())
            {
                var lote = db.Lotes.Find(id);
                if (lote == null)
                {
                    return HttpNotFound();
                }
                model = lote.ToViewModel();
            }
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre", model.ID_Medicamento);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(LoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new FarmaUEntities())
                {
                    var lote = model.ToEntity();
                    db.Entry(lote).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
        }
    }
}
