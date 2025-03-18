using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using System.Data.Entity;


namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class LoteController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Listar(LoteTableRequest request)
        {
            var query = db.Lotes.Select(l => new LoteTableViewModel
            {
                id_Lote = l.id_Lote,
                NombreMedicamento = l.Medicamentos.Nombre,
                Categoria = l.Medicamentos.Categorias.Nombre,
                Laboratorio = l.Medicamentos.Laboratorios.Nombre,
                cantidad = l.cantidad,
                fecha_vencimiento = l.fecha_vencimiento,
                Ubicacion = l.Inventario.FirstOrDefault().ubicacion,
                DiasParaVencer = (int)(l.fecha_vencimiento - DateTime.Now).TotalDays
            });

            // Filtros
            if (request.MedicamentoId.HasValue)
                query = query.Where(l => l.id_Lote == request.MedicamentoId.Value);

            if (request.CategoriaId.HasValue)
                query = query.Where(l => l.Categoria == request.CategoriaId.ToString());

            if (request.SoloVencidos == true)
                query = query.Where(l => l.DiasParaVencer < 0);

            if (request.SoloPorVencer == true)
                query = query.Where(l => l.DiasParaVencer > 0 && l.DiasParaVencer <= 30);

            if (request.SoloConStock == true)
                query = query.Where(l => l.cantidad > 0);

            int totalRecords = query.Count();
            var data = query.Skip(request.Start).Take(request.Length).ToList();

            var response = new LoteTableResponse
            {
                draw = request.Start,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data,
                TotalUnidades = data.Sum(l => l.cantidad),
                TotalLotes = totalRecords,
                LotesVencidos = data.Count(l => l.DiasParaVencer < 0),
                LotesPorVencer = data.Count(l => l.DiasParaVencer > 0 && l.DiasParaVencer <= 30)
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Crear()
        {
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre");
            return View(new LoteViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(LoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var lote = model.ToEntity();
                db.Lotes.Add(lote);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre", model.ID_Medicamento);
            return View(model);
        }

        public ActionResult Editar(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lote = db.Lotes.Find(id);
            if (lote == null)
                return HttpNotFound();

            var model = LoteViewModel.FromEntity(lote);
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre", model.ID_Medicamento);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(LoteViewModel model)
        {
            if (ModelState.IsValid)
            {
                var lote = db.Lotes.Find(model.id_Lote);
                if (lote == null)
                    return HttpNotFound();

                lote.ID_Medicamento = model.ID_Medicamento;
                lote.cantidad = model.cantidad;
                lote.fecha_vencimiento = model.fecha_vencimiento;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ID_Medicamento = new SelectList(db.Medicamentos, "ID_Medicamento", "Nombre", model.ID_Medicamento);
            return View(model);
        }

        public ActionResult Eliminar(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var lote = db.Lotes.Find(id);
            if (lote == null)
                return HttpNotFound();

            return View(LoteViewModel.FromEntity(lote));
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarEliminar(int id)
        {
            var lote = db.Lotes.Find(id);
            if (lote == null)
                return HttpNotFound();

            db.Lotes.Remove(lote);
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
