using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using Proyecto_Final_Desarrollo_Web.TableViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class FacturaController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities(); 

        public ActionResult Index(FacturaTableRequest request)
        {
            // Paginación y búsqueda
            var query = db.Facturas.AsQueryable();

            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where(f => f.Clientes.Personas.Nombre.Contains(request.SearchValue));
            }

            if (request.FechaInicio.HasValue)
            {
                query = query.Where(f => f.fecha >= request.FechaInicio.Value);
            }

            if (request.FechaFin.HasValue)
            {
                query = query.Where(f => f.fecha <= request.FechaFin.Value);
            }

            if (!string.IsNullOrEmpty(request.Estado))
            {
                query = query.Where(f => f.estado == request.Estado);
            }

            if (request.ClienteId.HasValue)
            {
                query = query.Where(f => f.ID_Cliente == request.ClienteId.Value);
            }

            var totalRecords = query.Count();

            // Ordenamiento
            if (request.SortColumn == "fecha" && request.SortDirection == "asc")
            {
                query = query.OrderBy(f => f.fecha);
            }
            else if (request.SortColumn == "fecha" && request.SortDirection == "desc")
            {
                query = query.OrderByDescending(f => f.fecha);
            }

            var facturas = query.Skip(request.Start).Take(request.Length).ToList();

            var response = new FacturaTableResponse
            {
                draw = 1,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = facturas.Select(f => new FacturaTableViewModel
                {
                    id_Factura = f.id_Factura,
                    fecha = f.fecha,
                    NombreCliente = f.Clientes.Personas.Nombre,
                    DocumentoCliente = f.Clientes.Personas.numero_documento,
                    total = f.total,
                    estado = f.estado,
                    NumeroProductos = f.Detalles_Factura.Count(),
                    BadgeClass = f.estado == "Completada" ? "bg-success" : "bg-warning"
                }).ToList(),
                TotalMonto = facturas.Sum(f => f.total)
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id)
        {
            var factura = db.Facturas.Include("Detalles_Factura.Medicamentos").FirstOrDefault(f => f.id_Factura == id);
            if (factura == null)
            {
                return HttpNotFound();
            }

            var viewModel = new VentaViewModel
            {
                id_Factura = factura.id_Factura,
                ID_Cliente = factura.ID_Cliente,
                fecha = factura.fecha,
                total = factura.total,
                estado = factura.estado,
                Detalles = factura.Detalles_Factura.Select(d => new DetalleVentaViewModel
                {
                    ID_Detalle_Factura = d.ID_Detalle_Factura,
                    ID_Medicamento = d.ID_Medicamento,
                    cantidad = d.cantidad,
                    subtotal = d.subtotal,
                    NombreMedicamento = d.Medicamentos.Nombre,
                    Categoria = d.Medicamentos.Categorias.Nombre,
                    PrecioUnitario = d.subtotal / d.cantidad
                }).ToList()
            };

            return View(viewModel);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VentaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var factura = viewModel.ToEntity();

                // Añadir detalles de la factura
                foreach (var detalle in viewModel.Detalles)
                {
                    var detalleFactura = detalle.ToEntity();
                    factura.Detalles_Factura.Add(detalleFactura);
                }

                db.Facturas.Add(factura);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        public ActionResult Edit(int id)
        {
            var factura = db.Facturas.Include("Detalles_Factura").FirstOrDefault(f => f.id_Factura == id);
            if (factura == null)
            {
                return HttpNotFound();
            }

            var viewModel = VentaViewModel.FromEntity(factura);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VentaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var factura = db.Facturas.Include("Detalles_Factura").FirstOrDefault(f => f.id_Factura == viewModel.id_Factura);
                if (factura == null)
                {
                    return HttpNotFound();
                }

                factura.estado = viewModel.estado;
                factura.total = viewModel.total;

                // Actualizar detalles de la factura
                factura.Detalles_Factura.Clear();
                foreach (var detalle in viewModel.Detalles)
                {
                    var detalleFactura = detalle.ToEntity();
                    factura.Detalles_Factura.Add(detalleFactura);
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(viewModel);
        }

        public ActionResult Delete(int id)
        {
            var factura = db.Facturas.Include("Detalles_Factura").FirstOrDefault(f => f.id_Factura == id);
            if (factura == null)
            {
                return HttpNotFound();
            }

            var viewModel = VentaViewModel.FromEntity(factura);
            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var factura = db.Facturas.FirstOrDefault(f => f.id_Factura == id);
            if (factura != null)
            {
                db.Facturas.Remove(factura);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
