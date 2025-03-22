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
    public class MedicamentosController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Medicamentos
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", int? categoriaId = null, int? laboratorioId = null, string estado = "Activo")
        {
            try
            {
                // Carga listas para filtros
                ViewBag.Categorias = new SelectList(db.Categorias.OrderBy(c => c.Nombre), "ID_Categoría", "Nombre");
                ViewBag.Laboratorios = new SelectList(db.Laboratorios.OrderBy(l => l.Nombre), "ID_Laboratorio", "Nombre");

                // Consulta base
                var query = db.Medicamentos
                    .Include(m => m.Categorias)
                    .Include(m => m.Laboratorios)
                    .Include(m => m.Lotes)
                    .AsQueryable();

                // Aplica filtros
                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(m => m.estado == estado);
                }

                if (categoriaId.HasValue)
                {
                    query = query.Where(m => m.ID_Categoría == categoriaId.Value);
                }

                if (laboratorioId.HasValue)
                {
                    query = query.Where(m => m.ID_Laboratorio == laboratorioId.Value);
                }

                // Busca por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(m => m.Nombre.ToLower().Contains(searchValue) ||
                                             m.principio_activo.ToLower().Contains(searchValue) ||
                                             m.Categorias.Nombre.ToLower().Contains(searchValue) ||
                                             m.Laboratorios.Nombre.ToLower().Contains(searchValue));
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var medicamentos = query
                    .OrderBy(m => m.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CategoriaId = categoriaId;
                ViewBag.LaboratorioId = laboratorioId;
                ViewBag.Estado = estado;

                // Transforma a ViewModel
                var medicamentosViewModel = medicamentos.Select(m => MedicamentoViewModel.FromEntity(m)).ToList();

                return View(medicamentosViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar medicamentos: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los medicamentos: " + ex.Message;
                return View(new List<MedicamentoViewModel>());
            }
        }

        // GET: Medicamentos/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Medicamentos medicamento = db.Medicamentos
                .Include(m => m.Categorias)
                .Include(m => m.Laboratorios)
                .Include(m => m.Lotes)
                .FirstOrDefault(m => m.ID_Medicamento == id);

            if (medicamento == null)
            {
                return HttpNotFound();
            }

            var viewModel = MedicamentoViewModel.FromEntity(medicamento);

            return View(viewModel);
        }

        // GET: Medicamentos/Create
        public ActionResult Create()
        {
            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre");
            ViewBag.ID_Laboratorio = new SelectList(db.Laboratorios, "ID_Laboratorio", "Nombre");
            return View(new MedicamentoViewModel());
        }

        // POST: Medicamentos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MedicamentoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var medicamento = viewModel.ToEntity();
                medicamento.estado = "Activo";

                db.Medicamentos.Add(medicamento);
                db.SaveChanges();

                TempData["Message"] = "Medicamento creado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", viewModel.ID_Categoría);
            ViewBag.ID_Laboratorio = new SelectList(db.Laboratorios, "ID_Laboratorio", "Nombre", viewModel.ID_Laboratorio);
            return View(viewModel);
        }

        // GET: Medicamentos/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Medicamentos medicamento = db.Medicamentos.Find(id);
            if (medicamento == null)
            {
                return HttpNotFound();
            }

            var viewModel = MedicamentoViewModel.FromEntity(medicamento);

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", medicamento.ID_Categoría);
            ViewBag.ID_Laboratorio = new SelectList(db.Laboratorios, "ID_Laboratorio", "Nombre", medicamento.ID_Laboratorio);

            return View(viewModel);
        }

        // POST: Medicamentos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MedicamentoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var medicamento = viewModel.ToEntity();

                db.Entry(medicamento).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Medicamento actualizado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", viewModel.ID_Categoría);
            ViewBag.ID_Laboratorio = new SelectList(db.Laboratorios, "ID_Laboratorio", "Nombre", viewModel.ID_Laboratorio);

            return View(viewModel);
        }

        // GET: Medicamentos/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Medicamentos medicamento = db.Medicamentos
                .Include(m => m.Categorias)
                .Include(m => m.Laboratorios)
                .FirstOrDefault(m => m.ID_Medicamento == id);

            if (medicamento == null)
            {
                return HttpNotFound();
            }

            // Verifica si tiene lotes asociados
            if (medicamento.Lotes.Any())
            {
                ViewBag.TieneLotes = true;
                ViewBag.TotalLotes = medicamento.Lotes.Count;
            }
            else
            {
                ViewBag.TieneLotes = false;
            }

            var viewModel = MedicamentoViewModel.FromEntity(medicamento);
            return View(viewModel);
        }

        // POST: Medicamentos/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Medicamentos medicamento = db.Medicamentos
                .Include(m => m.Lotes)
                .FirstOrDefault(m => m.ID_Medicamento == id);

            // En lugar de eliminar físicamente, cambia el estado
            medicamento.estado = "Inactivo";
            db.Entry(medicamento).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Message"] = "Medicamento eliminado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Medicamentos/Inventario
        public ActionResult Inventario(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var medicamento = db.Medicamentos
                .Include(m => m.Categorias)
                .Include(m => m.Laboratorios)
                .Include(m => m.Lotes)
                .FirstOrDefault(m => m.ID_Medicamento == id);

            if (medicamento == null)
            {
                return HttpNotFound();
            }

            var viewModel = MedicamentoViewModel.FromEntity(medicamento);

            // Obtiene los lotes del medicamento
            var lotes = db.Lotes
                .Where(l => l.ID_Medicamento == id)
                .ToList()
                .Select(l => LoteViewModel.FromEntity(l))
                .ToList();

            ViewBag.Lotes = lotes;

            return View(viewModel);
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