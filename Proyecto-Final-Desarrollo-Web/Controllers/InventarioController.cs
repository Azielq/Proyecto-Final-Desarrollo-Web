using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class InventarioController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetInventarioTable(InventarioTableRequest request)
        {
            var query = db.Inventario
                .Select(i => new InventarioTableViewModel
                {
                    id_Inventario = i.id_Inventario,
                    ID_Lote = i.ID_Lote,
                    NombreMedicamento = i.Lotes.Medicamentos.Nombre,
                    Categoria = i.Lotes.Medicamentos.Categorias.Nombre,
                    Laboratorio = i.Lotes.Medicamentos.Laboratorios.Nombre,
                    NumeroLote = i.Lotes.id_Lote.ToString(),
                    ubicacion = i.ubicacion,
                    Cantidad = i.Lotes.cantidad, // Usamos la propiedad cantidad de Lotes
                    FechaVencimiento = i.Lotes.fecha_vencimiento,
                    Estado = i.Lotes.cantidad > 0 ? (i.Lotes.fecha_vencimiento < DateTime.Now ? "Vencido" : "Activo") : "Agotado" // Usamos la propiedad cantidad de Lotes
                });

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                string search = request.SearchValue.ToLower();
                query = query.Where(i =>
                    i.NombreMedicamento.ToLower().Contains(search) ||
                    i.Categoria.ToLower().Contains(search) ||
                    i.Laboratorio.ToLower().Contains(search) ||
                    i.NumeroLote.ToLower().Contains(search) ||
                    i.ubicacion.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(request.Ubicacion))
            {
                query = query.Where(i => i.ubicacion == request.Ubicacion);
            }
            if (request.MedicamentoId.HasValue)
            {
                query = query.Where(i => i.ID_Lote == request.MedicamentoId.Value);
            }
            if (request.CategoriaId.HasValue)
            {
                query = query.Where(i => i.Categoria == db.Categorias.Find(request.CategoriaId.Value).Nombre);
            }
            if (request.SoloVencidos.HasValue && request.SoloVencidos.Value)
            {
                query = query.Where(i => i.FechaVencimiento < DateTime.Now);
            }
            if (request.SoloPorVencer.HasValue && request.SoloPorVencer.Value)
            {
                DateTime umbral = DateTime.Now.AddDays(30);
                query = query.Where(i => i.FechaVencimiento >= DateTime.Now && i.FechaVencimiento <= umbral);
            }

            switch (request.SortColumn)
            {
                case "NombreMedicamento":
                    query = request.SortDirection == "asc" ? query.OrderBy(i => i.NombreMedicamento) : query.OrderByDescending(i => i.NombreMedicamento);
                    break;
                case "Categoria":
                    query = request.SortDirection == "asc" ? query.OrderBy(i => i.Categoria) : query.OrderByDescending(i => i.Categoria);
                    break;
                case "Cantidad":
                    query = request.SortDirection == "asc" ? query.OrderBy(i => i.Cantidad) : query.OrderByDescending(i => i.Cantidad);
                    break;
                case "FechaVencimiento":
                    query = request.SortDirection == "asc" ? query.OrderBy(i => i.FechaVencimiento) : query.OrderByDescending(i => i.FechaVencimiento);
                    break;
                default:
                    query = query.OrderBy(i => i.id_Inventario);
                    break;
            }

            int totalRecords = db.Inventario.Count();
            int filteredRecords = query.Count();

            var data = query.Skip(request.Start).Take(request.Length).ToList();

            // Calcular valores totales
            decimal valorTotal = db.Inventario.Sum(i => i.Lotes.cantidad * i.Lotes.Medicamentos.precio_venta); // Usamos la propiedad cantidad de Lotes
            int totalUnidades = db.Inventario.Sum(i => i.Lotes.cantidad); // Usamos la propiedad cantidad de Lotes

            return Json(new InventarioTableResponse
            {
                draw = request.Start,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data,
                ValorTotal = valorTotal,
                TotalUnidades = totalUnidades
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(InventarioTableViewModel model)
        {
            if (ModelState.IsValid)
            {
                var inventario = new Inventario
                {
                    ID_Lote = model.ID_Lote,
                    ubicacion = model.ubicacion
                };
                db.Inventario.Add(inventario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var inventario = db.Inventario.Find(id);
            if (inventario == null)
            {
                return HttpNotFound();
            }
            var model = new InventarioTableViewModel
            {
                id_Inventario = inventario.id_Inventario,
                ID_Lote = inventario.Lotes.id_Lote,
                NombreMedicamento = inventario.Lotes.Medicamentos.Nombre,
                Categoria = inventario.Lotes.Medicamentos.Categorias.Nombre,
                Laboratorio = inventario.Lotes.Medicamentos.Laboratorios.Nombre,
                NumeroLote = inventario.Lotes.id_Lote.ToString(),
                ubicacion = inventario.ubicacion,
                Cantidad = inventario.Lotes.cantidad, // Usamos la propiedad cantidad de Lotes
                FechaVencimiento = inventario.Lotes.fecha_vencimiento,
                Estado = inventario.Lotes.cantidad > 0 ? (inventario.Lotes.fecha_vencimiento < DateTime.Now ? "Vencido" : "Activo") : "Agotado" // Usamos la propiedad cantidad de Lotes
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(InventarioTableViewModel model)
        {
            if (ModelState.IsValid)
            {
                var inventario = db.Inventario.Find(model.id_Inventario);
                if (inventario != null)
                {
                    inventario.ubicacion = model.ubicacion;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var inventario = db.Inventario.Find(id);
            if (inventario != null)
            {
                db.Inventario.Remove(inventario);
                db.SaveChanges();
                return Json(new { success = true, message = "Inventario eliminado correctamente." });
            }
            return Json(new { success = false, message = "Error al eliminar el inventario." });
        }
    }
}

