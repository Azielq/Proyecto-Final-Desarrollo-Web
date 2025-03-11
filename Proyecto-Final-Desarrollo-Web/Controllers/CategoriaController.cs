using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.Models.ViewModels;
using Proyecto_Final_Desarrollo_Web.TableViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetCategoriasTable(CategoriaTableRequest request)
        {
            var query = db.Categorias
                .Select(c => new CategoriaTableViewModel
                {
                    ID_Categoría = c.ID_Categoria,
                    Nombre = c.Nombre,
                    Descripcion = c.Descripcion,
                    TotalMedicamentos = c.Medicamentos.Count() 
                });

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                string search = request.SearchValue.ToLower();
                query = query.Where(c => c.Nombre.ToLower().Contains(search) || c.Descripcion.ToLower().Contains(search));
            }

            switch (request.SortColumn)
            {
                case "Nombre":
                    query = request.SortDirection == "asc" ? query.OrderBy(c => c.Nombre) : query.OrderByDescending(c => c.Nombre);
                    break;
                case "TotalMedicamentos":
                    query = request.SortDirection == "asc" ? query.OrderBy(c => c.TotalMedicamentos) : query.OrderByDescending(c => c.TotalMedicamentos);
                    break;
                default:
                    query = query.OrderBy(c => c.ID_Categoría);
                    break;
            }

            int totalRecords = db.Categorias.Count();
            int filteredRecords = query.Count();

            var data = query.Skip(request.Start).Take(request.Length).ToList();

            return Json(new CategoriaTableResponse
            {
                draw = request.Start,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CategoriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var categoria = new Categoria
                {
                    Nombre = model.Nombre,
                    Descripcion = model.Descripcion
                };
                db.Categorias.Add(categoria);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var categoria = db.Categorias.Find(id);
            if (categoria == null)
            {
                return HttpNotFound();
            }
            var model = new CategoriaViewModel
            {
                ID_Categoria = categoria.ID_Categoria,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(CategoriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var categoria = db.Categorias.Find(model.ID_Categoria);
                if (categoria != null)
                {
                    categoria.Nombre = model.Nombre;
                    categoria.Descripcion = model.Descripcion;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var categoria = db.Categorias.Find(id);
            if (categoria != null)
            {
                db.Categorias.Remove(categoria);
                db.SaveChanges();
                return Json(new { success = true, message = "Categoría eliminada correctamente." });
            }
            return Json(new { success = false, message = "Error al eliminar la categoría." });
        }
    }
}
