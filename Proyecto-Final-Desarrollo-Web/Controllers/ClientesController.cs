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
using Proyecto_Final_Desarrollo_Web.Filters;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    [AuthorizeRoles("Administrador", "Supervisor")]

    public class ClientesController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Clientes
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            try
            {
                // Consulta base
                var query = db.Clientes
                    .Include(c => c.Personas)
                    .Include(c => c.Facturas)
                    .AsQueryable();

                // Busca por término
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(c => c.Personas.Nombre.ToLower().Contains(searchValue) ||
                                              c.Personas.Apellido_1.ToLower().Contains(searchValue) ||
                                              c.Personas.Apellido_2.ToLower().Contains(searchValue) ||
                                              c.Personas.numero_documento.ToString().Contains(searchValue) ||
                                              c.Personas.Teléfono.Contains(searchValue) ||
                                              c.Personas.Correo.ToLower().Contains(searchValue));
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var clientes = query
                    .OrderBy(c => c.Personas.Nombre)
                    .ThenBy(c => c.Personas.Apellido_1)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;

                // Transforma a ViewModel
                var clientesViewModel = clientes.Select(c => new ClienteTableViewModel
                {
                    id_cliente = c.id_cliente,
                    ID_Persona = c.ID_Persona,
                    NombreCompleto = $"{c.Personas.Nombre} {c.Personas.Apellido_1} {c.Personas.Apellido_2}",
                    Documento = $"{c.Personas.tipo_documento} {c.Personas.numero_documento}",
                    Telefono = c.Personas.Teléfono,
                    Correo = c.Personas.Correo,
                    Direccion = c.Personas.dirección,
                    TotalCompras = c.Facturas.Where(f => f.estado == "Completada").Sum(f => f.total),
                    UltimaCompra = c.Facturas.Where(f => f.estado == "Completada").OrderByDescending(f => f.fecha).Select(f => f.fecha).FirstOrDefault(),
                    NumeroCompras = c.Facturas.Count(f => f.estado == "Completada")
                }).ToList();

                return View(clientesViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar clientes: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los clientes: " + ex.Message;
                return View(new List<ClienteTableViewModel>());
            }
        }

        // GET: Clientes/Create
        public ActionResult Create()
        {
            return View(new ClienteViewModel());
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ClienteViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica si ya existe un cliente con el mismo número de documento
                    if (db.Personas.Any(p => p.numero_documento == viewModel.numero_documento))
                    {
                        ModelState.AddModelError("numero_documento", "Ya existe una persona con este número de documento");
                        return View(viewModel);
                    }

                    // Verifica si ya existe una persona con el mismo correo electrónico
                    if (db.Personas.Any(p => p.Correo == viewModel.Correo))
                    {
                        ModelState.AddModelError("Correo", "El correo electrónico ya está en uso");
                        return View(viewModel);
                    }

                    // Verifica si ya existe una persona con el mismo número de teléfono
                    if (db.Personas.Any(p => p.Teléfono == viewModel.Teléfono))
                    {
                        ModelState.AddModelError("Teléfono", "El número de teléfono ya está en uso");
                        return View(viewModel);
                    }

                    var entities = viewModel.ToEntities();
                    var persona = entities.Item1;
                    var cliente = entities.Item2;

                    // Primero crea la persona
                    db.Personas.Add(persona);
                    db.SaveChanges();

                    // Asigna el ID de persona al cliente
                    cliente.ID_Persona = persona.ID_Persona;
                    db.Clientes.Add(cliente);
                    db.SaveChanges();

                    // Reemplazar TempData con SweetAlert
                    SweetAlertHelper.ShowSweetAlert(this, "¡Éxito!", "Cliente creado correctamente", SweetAlertMessageType.Success);
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al crear cliente: {ex.Message}");
                    ModelState.AddModelError("", "Error al crear el cliente: " + ex.Message);
                    SweetAlertHelper.ShowSweetAlert(this, "Error", "Ha ocurrido un error al crear el cliente", SweetAlertMessageType.Error);
                }
            }

            return View(viewModel);
        }

        // GET: Clientes/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Clientes cliente = db.Clientes
                .Include(c => c.Personas)
                .FirstOrDefault(c => c.id_cliente == id);

            if (cliente == null)
            {
                return HttpNotFound();
            }

            var viewModel = ClienteViewModel.FromEntity(cliente);

            return View(viewModel);
        }

        // POST: Clientes/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ClienteViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica si ya existe otro cliente con el mismo número de documento
                    if (db.Personas.Any(p => p.numero_documento == viewModel.numero_documento && p.ID_Persona != viewModel.ID_Persona))
                    {
                        ModelState.AddModelError("numero_documento", "Ya existe otra persona con este número de documento");
                        return View(viewModel);
                    }

                    // Verifica si ya existe otra persona con el mismo correo electrónico
                    if (db.Personas.Any(p => p.Correo == viewModel.Correo && p.ID_Persona != viewModel.ID_Persona))
                    {
                        ModelState.AddModelError("Correo", "El correo electrónico ya está en uso por otra persona");
                        return View(viewModel);
                    }

                    // Verifica si ya existe otra persona con el mismo número de teléfono
                    if (db.Personas.Any(p => p.Teléfono == viewModel.Teléfono && p.ID_Persona != viewModel.ID_Persona))
                    {
                        ModelState.AddModelError("Teléfono", "El número de teléfono ya está en uso por otra persona");
                        return View(viewModel);
                    }

                    var entities = viewModel.ToEntities();
                    var persona = entities.Item1;

                    // Actualiza la persona
                    db.Entry(persona).State = EntityState.Modified;
                    db.SaveChanges();

                    // Reemplaza TempData con SweetAlert
                    SweetAlertHelper.ShowSweetAlert(this, "¡Éxito!", "Cliente actualizado correctamente", SweetAlertMessageType.Success);
                    return RedirectToAction("Details", new { id = viewModel.id_cliente });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al actualizar cliente: {ex.Message}");
                    ModelState.AddModelError("", "Error al actualizar el cliente: " + ex.Message);
                    SweetAlertHelper.ShowSweetAlert(this, "Error", "Ha ocurrido un error al actualizar el cliente", SweetAlertMessageType.Error);
                }
            }

            return View(viewModel);
        }

        // GET: Clientes/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Clientes cliente = db.Clientes
                .Include(c => c.Personas)
                .Include(c => c.Facturas)
                .FirstOrDefault(c => c.id_cliente == id);

            if (cliente == null)
            {
                return HttpNotFound();
            }

            // Verifica si tiene facturas asociadas
            if (cliente.Facturas.Any())
            {
                ViewBag.TieneFacturas = true;
                ViewBag.TotalFacturas = cliente.Facturas.Count;
            }
            else
            {
                ViewBag.TieneFacturas = false;
            }

            var viewModel = ClienteViewModel.FromEntity(cliente);
            return View(viewModel);
        }

        // POST: Clientes/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                Clientes cliente = db.Clientes
                    .Include(c => c.Facturas)
                    .FirstOrDefault(c => c.id_cliente == id);

                if (cliente == null)
                {
                    SweetAlertHelper.ShowSweetAlert(this, "Error", "El cliente no existe", SweetAlertMessageType.Error);
                    return RedirectToAction("Index");
                }

                // Verifica si tiene facturas asociadas
                if (cliente.Facturas.Any())
                {
                    SweetAlertHelper.ShowSweetAlert(this, "Acción no permitida", "No se puede eliminar el cliente porque tiene facturas asociadas", SweetAlertMessageType.Warning);
                    return RedirectToAction("Delete", new { id = id });
                }

                // Primero obtiene la persona asociada
                var persona = db.Personas.Find(cliente.ID_Persona);

                // Elimina el cliente
                db.Clientes.Remove(cliente);

                // Luego elimina la persona si no tiene otras relaciones activas
                if (persona != null)
                {
                    // Verifica que la persona no tenga otras relaciones activas (médico, usuario)
                    if (!db.Medicos.Any(m => m.ID_Persona == persona.ID_Persona) &&
                        !db.Usuarios.Any(u => u.ID_Persona == persona.ID_Persona))
                    {
                        db.Personas.Remove(persona);
                    }
                }

                db.SaveChanges();

                // Mensaje específico para la eliminación correcta del cliente
                SweetAlertHelper.ShowSweetAlert(this, "¡Éxito!", "El cliente ha sido eliminado correctamente", SweetAlertMessageType.Success);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al eliminar cliente: {ex.Message}");
                SweetAlertHelper.ShowSweetAlert(this, "Error", "Error al eliminar el cliente: " + ex.Message, SweetAlertMessageType.Error);
                return RedirectToAction("Index");
            }
        }

        // GET: Clientes/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Clientes cliente = db.Clientes
                .Include(c => c.Personas)
                .Include(c => c.Facturas)
                .FirstOrDefault(c => c.id_cliente == id);

            if (cliente == null)
            {
                return HttpNotFound();
            }

            var viewModel = ClienteViewModel.FromEntity(cliente);

            return View(viewModel);
        }

        // GET: Clientes/GetFacturasByCliente
        [HttpGet]
        public ActionResult GetFacturasByCliente(int id)
        {
            try
            {
                var facturas = db.Facturas
                    .Include(f => f.Detalles_Factura)
                    .Where(f => f.ID_Cliente == id)
                    .OrderByDescending(f => f.fecha)
                    .Take(10)
                    .Select(f => new {
                        id_Factura = f.id_Factura,
                        fecha = f.fecha,
                        total = f.total,
                        estado = f.estado,
                        cantidadProductos = f.Detalles_Factura.Count
                    })
                    .ToList();

                return Json(facturas, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener facturas por cliente: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Clientes/GetClienteById
        [HttpGet]
        public ActionResult GetClienteById(int id)
        {
            try
            {
                var cliente = db.Clientes
                    .Include(c => c.Personas)
                    .Where(c => c.id_cliente == id)
                    .Select(c => new {
                        id_cliente = c.id_cliente,
                        ID_Persona = c.ID_Persona,
                        Nombre = c.Personas.Nombre,
                        Apellido_1 = c.Personas.Apellido_1,
                        Apellido_2 = c.Personas.Apellido_2,
                        tipo_documento = c.Personas.tipo_documento,
                        numero_documento = c.Personas.numero_documento,
                        Correo = c.Personas.Correo,
                        Teléfono = c.Personas.Teléfono,
                        dirección = c.Personas.dirección
                    })
                    .FirstOrDefault();

                if (cliente == null)
                {
                    return HttpNotFound();
                }

                return Json(cliente, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener cliente: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Clientes/ComprasCliente
        public ActionResult ComprasCliente(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                // Obtien el cliente con sus datos personales y facturas
                Clientes cliente = db.Clientes
                    .Include(c => c.Personas)
                    .Include(c => c.Facturas)
                    .FirstOrDefault(c => c.id_cliente == id);

                if (cliente == null)
                {
                    return HttpNotFound();
                }

                // Convierte a ViewModel
                var viewModel = ClienteViewModel.FromEntity(cliente);

                // Obtiene las facturas del cliente ordenadas por fecha descendente
                var facturas = db.Facturas
                    .Include(f => f.Detalles_Factura)
                    .Where(f => f.ID_Cliente == id)
                    .OrderByDescending(f => f.fecha)
                    .ToList()
                    .Select(f => new FacturaResumenViewModel
                    {
                        id_Factura = f.id_Factura,
                        fecha = f.fecha,
                        total = f.total,
                        estado = f.estado,
                        CantidadProductos = f.Detalles_Factura.Count
                    })
                    .ToList();

                ViewBag.Facturas = facturas;

                // Calcula estadísticas de compras
                ViewBag.TotalCompras = facturas.Where(f => f.estado == "Completada").Sum(f => f.total);
                ViewBag.NumeroCompras = facturas.Count(f => f.estado == "Completada");
                ViewBag.PromedioCompra = ViewBag.NumeroCompras > 0 ? ViewBag.TotalCompras / ViewBag.NumeroCompras : 0;
                ViewBag.UltimaCompra = facturas.Where(f => f.estado == "Completada").OrderByDescending(f => f.fecha).Select(f => f.fecha).FirstOrDefault();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener compras del cliente: {ex.Message}");
                SweetAlertHelper.ShowSweetAlert(this, "Error", "Error al cargar las compras del cliente: " + ex.Message, SweetAlertMessageType.Error);
                return RedirectToAction("Details", new { id = id });
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