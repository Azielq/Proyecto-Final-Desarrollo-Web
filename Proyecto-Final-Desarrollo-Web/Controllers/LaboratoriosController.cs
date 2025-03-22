using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Helpers;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class LaboratoriosController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Laboratorios
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", string pais = "")
        {
            try
            {
                // Consulta base
                var query = db.Laboratorios
                    .Include(l => l.Medicamentos)
                    .AsQueryable();

                // Aplica filtros
                if (!string.IsNullOrEmpty(pais))
                {
                    query = query.Where(l => l.pais_origen == pais);
                }

                // Busca por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(l => l.Nombre.ToLower().Contains(searchValue) ||
                                           l.pais_origen.ToLower().Contains(searchValue));
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var laboratorios = query
                    .OrderBy(l => l.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Pais = pais;

                // Obtiene lista de países para el filtro
                ViewBag.Paises = new SelectList(db.Laboratorios
                    .Select(l => l.pais_origen)
                    .Distinct()
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .OrderBy(p => p)
                    .ToList());

                // Transforma a ViewModel
                var laboratoriosViewModel = laboratorios.Select(l => new LaboratorioTableViewModel
                {
                    ID_Laboratorio = l.ID_Laboratorio,
                    Nombre = l.Nombre,
                    pais_origen = l.pais_origen,
                    TotalMedicamentos = l.Medicamentos.Count(m => m.estado == "Activo")
                }).ToList();

                return View(laboratoriosViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar laboratorios: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los laboratorios: " + ex.Message;
                return View(new List<LaboratorioTableViewModel>());
            }
        }

        // GET: Laboratorios/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Laboratorios laboratorio = db.Laboratorios
                .Include(l => l.Medicamentos)
                .FirstOrDefault(l => l.ID_Laboratorio == id);

            if (laboratorio == null)
            {
                return HttpNotFound();
            }

            // Esto obtiene medicamentos activos de este laboratorio
            var medicamentos = db.Medicamentos
                .Include(m => m.Categorias)
                .Where(m => m.ID_Laboratorio == id && m.estado == "Activo")
                .ToList();

            ViewBag.Medicamentos = medicamentos;
            ViewBag.TotalMedicamentos = medicamentos.Count;

            return View(laboratorio);
        }

        // GET: Laboratorios/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Laboratorios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Laboratorio,Nombre,pais_origen")] Laboratorios laboratorio)
        {
            if (ModelState.IsValid)
            {
                db.Laboratorios.Add(laboratorio);
                db.SaveChanges();

                TempData["Message"] = "Laboratorio creado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(laboratorio);
        }

        // GET: Laboratorios/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Laboratorios laboratorio = db.Laboratorios.Find(id);
            if (laboratorio == null)
            {
                return HttpNotFound();
            }

            return View(laboratorio);
        }

        // POST: Laboratorios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Laboratorio,Nombre,pais_origen")] Laboratorios laboratorio)
        {
            if (ModelState.IsValid)
            {
                db.Entry(laboratorio).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Laboratorio actualizado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(laboratorio);
        }

        // GET: Laboratorios/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Laboratorios laboratorio = db.Laboratorios
                .Include(l => l.Medicamentos)
                .FirstOrDefault(l => l.ID_Laboratorio == id);

            if (laboratorio == null)
            {
                return HttpNotFound();
            }

            // Esto verifica si tiene medicamentos asociados
            if (laboratorio.Medicamentos.Any())
            {
                ViewBag.TieneMedicamentos = true;
                ViewBag.TotalMedicamentos = laboratorio.Medicamentos.Count;
            }
            else
            {
                ViewBag.TieneMedicamentos = false;
            }

            return View(laboratorio);
        }

        // POST: Laboratorios/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Laboratorios laboratorio = db.Laboratorios
                .Include(l => l.Medicamentos)
                .FirstOrDefault(l => l.ID_Laboratorio == id);

            // Esto verifica si tiene medicamentos asociados
            if (laboratorio.Medicamentos.Any())
            {
                TempData["Message"] = "No se puede eliminar el laboratorio porque tiene medicamentos asociados";
                TempData["MessageType"] = "error";
                return RedirectToAction("Delete", new { id = id });
            }

            db.Laboratorios.Remove(laboratorio);
            db.SaveChanges();

            TempData["Message"] = "Laboratorio eliminado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Laboratorios/GetPaises
        [HttpGet]
        public ActionResult GetPaises()
        {
            var paises = db.Laboratorios
                .Select(l => l.pais_origen)
                .Distinct()
                .Where(p => p != null)
                .OrderBy(p => p)
                .ToList();

            return Json(paises, JsonRequestBehavior.AllowGet);
        }

        // GET: Laboratorios/MedicamentosPorLaboratorio
        [HttpGet]
        public ActionResult MedicamentosPorLaboratorio()
        {
            var laboratorios = db.Laboratorios
                .Include(l => l.Medicamentos)
                .ToList()
                .Select(l => new
                {
                    Laboratorio = l.Nombre,
                    Pais = l.pais_origen,
                    TotalMedicamentos = l.Medicamentos.Count(m => m.estado == "Activo")
                })
                .OrderByDescending(l => l.TotalMedicamentos)
                .ToList();

            return Json(laboratorios, JsonRequestBehavior.AllowGet);
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