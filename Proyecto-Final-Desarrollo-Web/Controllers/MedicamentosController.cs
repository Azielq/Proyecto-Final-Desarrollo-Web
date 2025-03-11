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

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class MedicamentosController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Medicamentos
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetMedicamentos(MedicamentoTableRequest request)
        {
            var response = new MedicamentoTableResponse
            {
                draw = request.Start
            };

            // Consulta base
            var query = db.Medicamentos
                .Include(m => m.Categorias)
                .Include(m => m.Laboratorios)
                .AsQueryable();

            // Esto filtra activos o por estado específico
            if (!string.IsNullOrEmpty(request.Estado))
            {
                query = query.Where(m => m.estado == request.Estado);
            }
            else
            {
                query = query.Where(m => m.estado == "Activo");
            }

            // Esto filtra adicionales
            if (request.CategoriaId.HasValue)
            {
                query = query.Where(m => m.ID_Categoría == request.CategoriaId.Value);
            }

            if (request.LaboratorioId.HasValue)
            {
                query = query.Where(m => m.ID_Laboratorio == request.LaboratorioId.Value);
            }

            // Esto busca por término
            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var searchValue = request.SearchValue.ToLower();
                query = query.Where(m => m.Nombre.ToLower().Contains(searchValue) ||
                                         m.principio_activo.ToLower().Contains(searchValue) ||
                                         m.Categorias.Nombre.ToLower().Contains(searchValue) ||
                                         m.Laboratorios.Nombre.ToLower().Contains(searchValue));
            }

            // Total sin filtrar
            response.recordsTotal = query.Count();

            // Total después de filtrar
            response.recordsFiltered = query.Count();

            // Ordena
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                if (request.SortDirection.ToLower() == "asc")
                {
                    switch (request.SortColumn)
                    {
                        case "Nombre":
                            query = query.OrderBy(m => m.Nombre);
                            break;
                        case "principio_activo":
                            query = query.OrderBy(m => m.principio_activo);
                            break;
                        case "NombreCategoria":
                            query = query.OrderBy(m => m.Categorias.Nombre);
                            break;
                        case "NombreLaboratorio":
                            query = query.OrderBy(m => m.Laboratorios.Nombre);
                            break;
                        case "precio_compra":
                            query = query.OrderBy(m => m.precio_compra);
                            break;
                        case "precio_venta":
                            query = query.OrderBy(m => m.precio_venta);
                            break;
                        default:
                            query = query.OrderBy(m => m.Nombre);
                            break;
                    }
                }
                else
                {
                    switch (request.SortColumn)
                    {
                        case "Nombre":
                            query = query.OrderByDescending(m => m.Nombre);
                            break;
                        case "principio_activo":
                            query = query.OrderByDescending(m => m.principio_activo);
                            break;
                        case "NombreCategoria":
                            query = query.OrderByDescending(m => m.Categorias.Nombre);
                            break;
                        case "NombreLaboratorio":
                            query = query.OrderByDescending(m => m.Laboratorios.Nombre);
                            break;
                        case "precio_compra":
                            query = query.OrderByDescending(m => m.precio_compra);
                            break;
                        case "precio_venta":
                            query = query.OrderByDescending(m => m.precio_venta);
                            break;
                        default:
                            query = query.OrderByDescending(m => m.Nombre);
                            break;
                    }
                }
            }
            else
            {
                query = query.OrderBy(m => m.Nombre);
            }

            // Paginación
            query = query.Skip(request.Start).Take(request.Length);

            // Convierte y asigna a la respuesta
            response.data = query.ToList().Select(m => new MedicamentoTableViewModel
            {
                ID_Medicamento = m.ID_Medicamento,
                Nombre = m.Nombre,
                principio_activo = m.principio_activo,
                NombreCategoria = m.Categorias.Nombre,
                NombreLaboratorio = m.Laboratorios.Nombre,
                precio_compra = m.precio_compra,
                precio_venta = m.precio_venta,
                estado = m.estado,
                StockTotal = m.Lotes.Sum(l => l.cantidad),
                TieneLotesProximosAVencer = m.Lotes.Any(l => l.fecha_vencimiento > DateTime.Now &&
                                                         l.fecha_vencimiento <= DateTime.Now.AddDays(30) &&
                                                         l.cantidad > 0)
            }).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
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

            var viewModel = MedicamentoViewModel.FromEntity(medicamento);

            return View(viewModel);
        }

        // POST: Medicamentos/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Medicamentos medicamento = db.Medicamentos.Find(id);

            // En lugar de eliminar, cambia el estado
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