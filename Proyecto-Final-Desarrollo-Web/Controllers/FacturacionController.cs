using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Helpers;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using Proyecto_Final_Desarrollo_Web.ViewModels.Facturacion;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    //[Authorize(Roles = "Administrador, Supervisor")] hay que configurar bien los roles!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    public class FacturacionController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Facturacion
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", string estado = "")
        {
            try
            {
                // Configurar ViewBags para filtros
                ViewBag.Estados = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Todos" },
                    new SelectListItem { Value = "Pendiente", Text = "Pendiente" },
                    new SelectListItem { Value = "Completada", Text = "Completada" },
                    new SelectListItem { Value = "Cancelada", Text = "Cancelada" }
                };

                // Consulta base
                var query = db.Facturas
                    .Include(f => f.Clientes.Personas)
                    .Include(f => f.Detalles_Factura)
                    .AsQueryable();

                // Aplicar filtros
                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(f => f.estado == estado);
                }

                // Búsqueda por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(f =>
                        f.Clientes.Personas.Nombre.ToLower().Contains(searchValue) ||
                        f.Clientes.Personas.Apellido_1.ToLower().Contains(searchValue) ||
                        f.Clientes.Personas.numero_documento.ToString().Contains(searchValue) ||
                        f.id_Factura.ToString().Contains(searchValue)
                    );
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplicar ordenamiento y paginación
                var facturas = query
                    .OrderByDescending(f => f.fecha)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guardar los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;
                ViewBag.Estado = estado;

                // Transformar a ViewModel
                var facturasViewModel = facturas.Select(f =>
                {
                    return new MiFacturaResumenViewModel
                    {
                        id_Factura = f.id_Factura,
                        fecha = f.fecha,
                        NombreCliente = $"{f.Clientes.Personas.Nombre} {f.Clientes.Personas.Apellido_1}".Trim(),
                        total = f.total,
                        estado = f.estado,
                        CantidadProductos = f.Detalles_Factura.Count
                    };
                }).ToList();

                return View(facturasViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar facturas: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar las facturas: " + ex.Message;
                return View(new List<MiFacturaResumenViewModel>());
            }
        }

        // POST: Facturacion/GetFacturas - Para DataTable AJAX
        [HttpPost]
        public ActionResult GetFacturas(FacturaTableRequest request)
        {
            try
            {
                var response = new FacturaTableResponse();

                // Consulta base
                var query = db.Facturas
                    .Include(f => f.Clientes.Personas)
                    .Include(f => f.Detalles_Factura)
                    .AsQueryable();

                // Aplicar filtros
                if (request.ClienteId.HasValue)
                {
                    query = query.Where(f => f.ID_Cliente == request.ClienteId.Value);
                }

                if (!string.IsNullOrEmpty(request.Estado))
                {
                    query = query.Where(f => f.estado == request.Estado);
                }

                if (request.FechaInicio.HasValue)
                {
                    query = query.Where(f => f.fecha >= request.FechaInicio.Value);
                }

                if (request.FechaFin.HasValue)
                {
                    var fechaFinConHora = request.FechaFin.Value.AddDays(1).AddSeconds(-1);
                    query = query.Where(f => f.fecha <= fechaFinConHora);
                }

                // Búsqueda por término
                if (!string.IsNullOrWhiteSpace(request.SearchValue))
                {
                    var searchValue = request.SearchValue.ToLower();
                    query = query.Where(f =>
                        f.Clientes.Personas.Nombre.ToLower().Contains(searchValue) ||
                        f.Clientes.Personas.Apellido_1.ToLower().Contains(searchValue) ||
                        f.Clientes.Personas.numero_documento.ToString().Contains(searchValue) ||
                        f.id_Factura.ToString().Contains(searchValue)
                    );
                }

                // Total sin filtrar
                response.recordsTotal = db.Facturas.Count();

                // Total filtrado
                response.recordsFiltered = query.Count();

                // Calcular monto total de facturas filtradas
                response.TotalMonto = query.Sum(f => f.total);

                // Aplicar ordenación
                if (!string.IsNullOrEmpty(request.SortColumn))
                {
                    switch (request.SortColumn.ToLower())
                    {
                        case "id_factura":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(f => f.id_Factura)
                                : query.OrderByDescending(f => f.id_Factura);
                            break;
                        case "fecha":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(f => f.fecha)
                                : query.OrderByDescending(f => f.fecha);
                            break;
                        case "cliente":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(f => f.Clientes.Personas.Nombre)
                                : query.OrderByDescending(f => f.Clientes.Personas.Nombre);
                            break;
                        case "total":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(f => f.total)
                                : query.OrderByDescending(f => f.total);
                            break;
                        case "estado":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(f => f.estado)
                                : query.OrderByDescending(f => f.estado);
                            break;
                        default:
                            query = query.OrderByDescending(f => f.fecha);
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(f => f.fecha);
                }

                // Aplicar paginación
                var facturasData = query
                    .Skip(request.Start)
                    .Take(request.Length)
                    .ToList();

                // Transformar a TableViewModel
                response.data = facturasData.Select(f =>
                {
                    return new FacturaTableViewModel
                    {
                        id_Factura = f.id_Factura,
                        fecha = f.fecha,
                        NombreCliente = $"{f.Clientes.Personas.Nombre} {f.Clientes.Personas.Apellido_1}".Trim(),
                        DocumentoCliente = $"{f.Clientes.Personas.tipo_documento}: {f.Clientes.Personas.numero_documento}",
                        total = f.total,
                        estado = f.estado,
                        NumeroProductos = f.Detalles_Factura.Count
                    };
                }).ToList();

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Facturacion/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Facturas factura = db.Facturas
                .Include(f => f.Clientes.Personas)
                .Include(f => f.Detalles_Factura.Select(d => d.Productos.Categorias))
                .Include(f => f.Detalles_Factura.Select(d => d.Productos.Imagenes_Producto))
                .FirstOrDefault(f => f.id_Factura == id);

            if (factura == null)
            {
                return HttpNotFound();
            }

            var viewModel = FacturaViewModel.FromEntity(factura);
            return View(viewModel);
        }

        // GET: Facturacion/POS
        public ActionResult POS()
        {
            // Preparar el ViewModel para el punto de venta
            var viewModel = new POSViewModel();

            // Cargar lista de productos activos para buscar/seleccionar
            viewModel.ProductosDisponibles = db.Productos
                .Where(p => p.estado == "Activo")
                .OrderBy(p => p.Nombre)
                .Select(p => new ProductoViewModel
                {
                    ID_Producto = p.ID_Producto,
                    Nombre = p.Nombre,
                    precio_venta = p.precio_venta,
                    Marca = p.Marca,
                    NombreCategoria = p.Categorias.Nombre,
                    StockDisponible = p.Lotes.Sum(l => l.cantidad),
                    UrlImagenPrincipal = p.Imagenes_Producto
                        .Where(i => i.Estado && i.EsPrincipal)
                        .Select(i => i.URL)
                        .FirstOrDefault()
                })
                .Take(20) // Limitar para no sobrecargar la vista inicial
                .ToList();

            // Cargar lista de clientes para dropdown
            ViewBag.ID_Cliente = new SelectList(
                db.Clientes.Include(c => c.Personas)
                .Select(c => new
                {
                    ID_Cliente = c.id_cliente,
                    NombreCompleto = c.Personas.Nombre + " " + c.Personas.Apellido_1 + " " + c.Personas.Apellido_2 + " (" + c.Personas.numero_documento + ")"
                }).OrderBy(c => c.NombreCompleto),
                "ID_Cliente", "NombreCompleto");

            return View(viewModel);
        }

        // POST: Facturacion/BuscarProducto
        [HttpPost]
        public ActionResult BuscarProducto(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new List<object>());

            var productos = db.Productos
                .Where(p => p.estado == "Activo" &&
                       (p.Nombre.Contains(term) ||
                        p.Marca.Contains(term) ||
                        p.Categorias.Nombre.Contains(term)))
                .Select(p => new
                {
                    id = p.ID_Producto,
                    label = p.Nombre + " (" + p.Marca + ")",
                    value = p.Nombre,
                    precio = p.precio_venta,
                    imagen = p.Imagenes_Producto
                        .Where(i => i.Estado && i.EsPrincipal)
                        .Select(i => i.URL)
                        .FirstOrDefault(),
                    stock = p.Lotes.Sum(l => l.cantidad),
                    categoria = p.Categorias.Nombre
                })
                .Take(10)
                .ToList();

            return Json(productos, JsonRequestBehavior.AllowGet);
        }

        // POST: Facturacion/Procesar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Procesar(POSViewModel model)
        {
            // Verificar que haya un cliente seleccionado
            if (model.ID_Cliente <= 0)
            {
                ModelState.AddModelError("ID_Cliente", "Debe seleccionar un cliente");
                return Json(new { success = false, message = "Debe seleccionar un cliente" });
            }

            // Verificar que haya productos seleccionados
            if (model.ProductosSeleccionados == null || model.ProductosSeleccionados.Count == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto");
                return Json(new { success = false, message = "Debe agregar al menos un producto" });
            }

            try
            {
                // Crear la factura
                var factura = new Facturas
                {
                    fecha = DateTime.Now,
                    ID_Cliente = model.ID_Cliente,
                    total = model.Total,
                    estado = "Completada"
                };

                db.Facturas.Add(factura);
                await db.SaveChangesAsync();

                // Crear los detalles de la factura y reducir inventario
                foreach (var item in model.ProductosSeleccionados)
                {
                    var producto = await db.Productos.FindAsync(item.ID_Producto);

                    if (producto == null)
                    {
                        throw new Exception($"Producto con ID {item.ID_Producto} no encontrado");
                    }

                    // Crear detalle de factura
                    var detalle = new Detalles_Factura
                    {
                        id_Factura = factura.id_Factura,
                        ID_Producto = item.ID_Producto,
                        cantidad = item.Cantidad,
                        subtotal = item.Cantidad * item.PrecioUnitario
                    };

                    db.Detalles_Factura.Add(detalle);

                    // Reducir el stock
                    int cantidadPendiente = item.Cantidad;
                    var lotes = db.Lotes
                        .Where(l => l.ID_Producto == item.ID_Producto && l.cantidad > 0)
                        .OrderBy(l => l.cantidad) // Usar primero los lotes con menos unidades
                        .ToList();

                    foreach (var lote in lotes)
                    {
                        if (cantidadPendiente <= 0) break;

                        if (lote.cantidad >= cantidadPendiente)
                        {
                            lote.cantidad -= cantidadPendiente;
                            cantidadPendiente = 0;
                        }
                        else
                        {
                            cantidadPendiente -= lote.cantidad;
                            lote.cantidad = 0;
                        }

                        db.Entry(lote).State = EntityState.Modified;
                    }

                    // Registrar movimiento de inventario (adaptado a tu modelo real)
                    var movimiento = new Movimientos_Inventario
                    {
                        id_Producto = item.ID_Producto,
                        id_Lote = lotes.First().id_Lote, // ¡CORRECCIÓN! Usar id_Lote en lugar de id_lote
                        tipo = "Venta",
                        cantidad = item.Cantidad * -1, // Negativo porque es salida
                        fecha = DateTime.Now,
                        ID_Factura = factura.id_Factura
                    };

                    db.Movimientos_Inventario.Add(movimiento);
                }

                // Guardar cambios en la base de datos
                await db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Factura #{factura.id_Factura} creada correctamente",
                    facturaId = factura.id_Factura
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al crear la factura: " + ex.Message
                });
            }
        }

        // POST: Facturacion/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancel(int id)
        {
            try
            {
                var factura = db.Facturas
                    .Include(f => f.Detalles_Factura)
                    .Include(f => f.Detalles_Factura.Select(d => d.Productos.Lotes))
                    .FirstOrDefault(f => f.id_Factura == id);

                if (factura == null)
                {
                    return HttpNotFound();
                }

                // Solo se pueden cancelar facturas completadas
                if (factura.estado != "Completada")
                {
                    TempData["Message"] = "Solo se pueden cancelar facturas en estado Completada";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("Details", new { id = id });
                }

                // Cambiar estado de la factura
                factura.estado = "Cancelada";
                db.Entry(factura).State = EntityState.Modified;

                // Devolver los productos al inventario
                foreach (var detalle in factura.Detalles_Factura)
                {
                    // Buscar el lote más reciente para devolver el producto
                    var lote = db.Lotes
                        .Where(l => l.ID_Producto == detalle.ID_Producto)
                        .OrderByDescending(l => l.cantidad) // Usar el lote con más unidades
                        .FirstOrDefault();

                    if (lote != null)
                    {
                        // Si existe un lote, aumentar la cantidad
                        lote.cantidad += detalle.cantidad;
                        db.Entry(lote).State = EntityState.Modified;

                        // Registrar movimiento de inventario (adaptado a tu modelo real)
                        var movimiento = new Movimientos_Inventario
                        {
                            id_Producto = detalle.ID_Producto,
                            id_Lote = lote.id_Lote, // ¡CORRECCIÓN! Usar id_Lote en lugar de id_lote
                            tipo = "Devolución",
                            cantidad = detalle.cantidad, // Positivo porque es entrada (devolución)
                            fecha = DateTime.Now,
                            ID_Factura = factura.id_Factura
                        };

                        db.Movimientos_Inventario.Add(movimiento);
                    }
                    else
                    {
                        // Si no existe, crear un nuevo lote
                        var nuevoLote = new Lotes
                        {
                            ID_Producto = detalle.ID_Producto,
                            cantidad = detalle.cantidad
                        };

                        db.Lotes.Add(nuevoLote);

                        // Registrar movimiento para el nuevo lote
                        var movimiento = new Movimientos_Inventario
                        {
                            id_Producto = detalle.ID_Producto,
                            id_Lote = 0, // Se actualizará después de guardar el lote
                            tipo = "Devolución",
                            cantidad = detalle.cantidad,
                            fecha = DateTime.Now,
                            ID_Factura = factura.id_Factura
                        };

                        db.Movimientos_Inventario.Add(movimiento);
                    }
                }

                await db.SaveChangesAsync();

                TempData["Message"] = "Factura cancelada correctamente. Los productos han sido devueltos al inventario.";
                TempData["MessageType"] = "success";
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error al cancelar la factura: " + ex.Message;
                TempData["MessageType"] = "error";
                return RedirectToAction("Details", new { id = id });
            }
        }

        // GET: Facturacion/Print/5
        public ActionResult Print(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Facturas factura = db.Facturas
                .Include(f => f.Clientes.Personas)
                .Include(f => f.Detalles_Factura.Select(d => d.Productos.Categorias))
                .FirstOrDefault(f => f.id_Factura == id);

            if (factura == null)
            {
                return HttpNotFound();
            }

            var viewModel = FacturaViewModel.FromEntity(factura);

            // Vista específica para impresión
            return View(viewModel);
        }

        // GET: Facturacion/Reports
        public ActionResult Reports()
        {
            // Vista para mostrar diferentes tipos de reportes
            return View();
        }

        // POST: Facturacion/SalesReport
        [HttpPost]
        public ActionResult SalesReport(DateTime fechaInicio, DateTime fechaFin, string tipoReporte = "diario")
        {
            try
            {
                // Asegurar que fechaFin incluya todo el día
                fechaFin = fechaFin.AddDays(1).AddSeconds(-1);

                // Consulta base de ventas en el período
                var query = db.Facturas
                    .Where(f => f.estado == "Completada" && f.fecha >= fechaInicio && f.fecha <= fechaFin)
                    .AsQueryable();

                // Resultados según el tipo de reporte
                object result = null;

                switch (tipoReporte)
                {
                    case "diario":
                        // Ventas agrupadas por día
                        result = query
                            .GroupBy(f => DbFunctions.TruncateTime(f.fecha))
                            .Select(g => new
                            {
                                Fecha = g.Key,
                                TotalVentas = g.Count(),
                                MontoTotal = g.Sum(f => f.total)
                            })
                            .OrderBy(g => g.Fecha)
                            .ToList();
                        break;

                    case "categorias":
                        // Ventas agrupadas por categoría de producto
                        result = db.Detalles_Factura
                            .Where(d => d.Facturas.fecha >= fechaInicio &&
                                     d.Facturas.fecha <= fechaFin &&
                                     d.Facturas.estado == "Completada")
                            .GroupBy(d => d.Productos.Categorias.Nombre)
                            .Select(g => new
                            {
                                Categoria = g.Key,
                                TotalProductos = g.Sum(d => d.cantidad),
                                MontoTotal = g.Sum(d => d.subtotal)
                            })
                            .OrderByDescending(g => g.MontoTotal)
                            .ToList();
                        break;

                    case "productos":
                        // Productos más vendidos
                        result = db.Detalles_Factura
                            .Where(d => d.Facturas.fecha >= fechaInicio &&
                                     d.Facturas.fecha <= fechaFin &&
                                     d.Facturas.estado == "Completada")
                            .GroupBy(d => new { d.ID_Producto, d.Productos.Nombre })
                            .Select(g => new
                            {
                                ProductoId = g.Key.ID_Producto,
                                ProductoNombre = g.Key.Nombre,
                                TotalVendido = g.Sum(d => d.cantidad),
                                MontoTotal = g.Sum(d => d.subtotal)
                            })
                            .OrderByDescending(g => g.TotalVendido)
                            .Take(10)
                            .ToList();
                        break;
                }

                // Información de resumen
                var resumen = new
                {
                    PeriodoInicio = fechaInicio.ToString("dd/MM/yyyy"),
                    PeriodoFin = fechaFin.ToString("dd/MM/yyyy"),
                    TotalFacturas = query.Count(),
                    MontoTotal = query.Sum(f => f.total),
                    TipoReporte = tipoReporte,
                    Datos = result
                };

                return Json(resumen, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
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