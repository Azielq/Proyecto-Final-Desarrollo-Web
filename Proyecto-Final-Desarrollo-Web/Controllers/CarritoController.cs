using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class CarritoController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Carrito
        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                TempData["Message"] = "Debes iniciar sesión para acceder a tu carrito";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Login", "Usuarios");
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);
            var carritoItems = db.Carrito
                .Include(c => c.Productos)
                .Where(c => c.ID_Usuario == idUsuario)
                .ToList();

            var viewModel = new CarritoViewModel
            {
                ID_Usuario = idUsuario,
                Items = carritoItems.Select(item => new CarritoItemViewModel
                {
                    ID_Carrito = item.ID_Carrito,
                    ID_Producto = item.ID_Producto,
                    NombreProducto = item.Productos.Nombre,
                    ImagenUrl = item.Productos.Imagenes_Producto
                                        .Where(i => i.Estado && i.EsPrincipal)
                                        .Select(i => i.URL)
                                        .FirstOrDefault()
                                    ?? "/Content/images/no-image.png",
                    PrecioUnitario = item.Productos.precio_venta,
                    Cantidad = item.Cantidad,
                    FechaAgregado = item.FechaAgregado
                }).ToList()
            };

            return View(viewModel);
        }


        // GET: Carrito/VerCarritoMini (Para mostrar un resumen del carrito en un widget de la barra de navegación)
        [ChildActionOnly]
        public ActionResult VerCarritoMini()
        {
            if (Session["UserID"] == null)
            {
                return PartialView("_CarritoMini", new CarritoViewModel());
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);

            var carritoItems = db.Carrito
                .Include(c => c.Productos)
                .Where(c => c.ID_Usuario == idUsuario)
                .ToList();

            var viewModel = new CarritoViewModel
            {
                ID_Usuario = idUsuario,
                Items = carritoItems.Select(item => new CarritoItemViewModel
                {
                    ID_Producto = item.ID_Producto,
                    NombreProducto = item.Productos.Nombre,
                    PrecioUnitario = item.Productos.precio_venta,
                    Cantidad = item.Cantidad
                }).ToList()
            };

            return PartialView("_CarritoMini", viewModel);
        }

        // POST: Carrito/Agregar
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Agregar(CarritoCrudViewModel.AgregarAlCarritoViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error al agregar producto al carrito";
                TempData["MessageType"] = "error";
                return RedirectToAction("Details", "Productos", new { id = viewModel.ID_Producto });
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);

            // Verificar si el producto ya está en el carrito
            var itemExistente = db.Carrito.FirstOrDefault(c =>
                c.ID_Usuario == idUsuario && c.ID_Producto == viewModel.ID_Producto);

            if (itemExistente != null)
            {
                // Actualizar cantidad si ya existe
                itemExistente.Cantidad += viewModel.Cantidad;
            }
            else
            {
                // Crear nuevo item en el carrito
                var nuevoItem = new Carrito
                {
                    ID_Usuario = idUsuario,
                    ID_Producto = viewModel.ID_Producto,
                    Cantidad = viewModel.Cantidad,
                    FechaAgregado = DateTime.Now
                };

                db.Carrito.Add(nuevoItem);
            }

            db.SaveChanges();

            // Obtener el nombre del producto para mostrar en el mensaje
            var producto = db.Productos.Find(viewModel.ID_Producto);
            string nombreProducto = producto != null ? producto.Nombre : "Producto";

            TempData["Message"] = $"{nombreProducto} agregado al carrito";
            TempData["MessageType"] = "success";
            TempData["UseSweet"] = true;

            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult AgregarAjax(int idProducto, int cantidad = 1)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Debe iniciar sesión para agregar productos al carrito" });
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);

            try
            {
                // Verificar si el producto existe y está activo
                var producto = db.Productos
                    .FirstOrDefault(p => p.ID_Producto == idProducto && p.estado == "Activo");

                if (producto == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado o no disponible" });
                }

                // Verificar si hay suficiente stock
                int stockDisponible = db.Lotes
                    .Where(l => l.ID_Producto == idProducto && l.cantidad > 0)
                    .Sum(l => l.cantidad);

                if (stockDisponible < cantidad)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Stock insuficiente. Solo hay {stockDisponible} unidades disponibles."
                    });
                }

                // Verificar si el producto ya está en el carrito
                var itemCarrito = db.Carrito
                    .FirstOrDefault(c => c.ID_Usuario == idUsuario && c.ID_Producto == idProducto);

                if (itemCarrito != null)
                {
                    // Si ya existe, actualizamos la cantidad
                    itemCarrito.Cantidad += cantidad;
                    itemCarrito.FechaAgregado = DateTime.Now; // Actualizamos la fecha
                    db.Entry(itemCarrito).State = EntityState.Modified;
                }
                else
                {
                    // Si no existe, creamos un nuevo item
                    var nuevoItem = new Carrito
                    {
                        ID_Usuario = idUsuario,
                        ID_Producto = idProducto,
                        Cantidad = cantidad,
                        FechaAgregado = DateTime.Now
                    };

                    db.Carrito.Add(nuevoItem);
                }

                // Guardamos los cambios
                db.SaveChanges();

                // Calculamos el total de items en el carrito
                int totalItems = db.Carrito
                    .Where(c => c.ID_Usuario == idUsuario)
                    .Sum(c => c.Cantidad);

                // Obtenemos información adicional para la respuesta
                string nombreProducto = producto.Nombre;
                decimal precioProducto = producto.precio_venta;

                return Json(new
                {
                    success = true,
                    message = $"{nombreProducto} agregado al carrito",
                    totalItems = totalItems,
                    productoNombre = nombreProducto,
                    productoPrecio = precioProducto
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al agregar al carrito",
                    error = ex.Message
                });
            }
        }

        // POST: Carrito/Actualizar
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Actualizar(CarritoCrudViewModel.ActualizarCarritoViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error al actualizar carrito";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            var itemCarrito = db.Carrito.Find(viewModel.ID_Carrito);

            if (itemCarrito == null)
            {
                TempData["Message"] = "Producto no encontrado en el carrito";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            // Verificar que el ítem pertenezca al usuario actual
            if (itemCarrito.ID_Usuario != Convert.ToInt32(Session["UserID"]))
            {
                TempData["Message"] = "No tienes permiso para modificar este item";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            itemCarrito.Cantidad = viewModel.Cantidad;
            db.SaveChanges();

            TempData["Message"] = "Carrito actualizado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // POST: Carrito/ActualizarAjax
        [HttpPost]
        public JsonResult ActualizarAjax(int idCarrito, int cantidad)
        {
            if (Session["UserID"] == null)
                return Json(new { success = false, message = "Debe iniciar sesión" });

            int idUsuario = Convert.ToInt32(Session["UserID"]);
            var item = db.Carrito.Find(idCarrito);
            if (item == null || item.ID_Usuario != idUsuario)
                return Json(new { success = false, message = "Item no encontrado o sin permiso" });

            item.Cantidad = cantidad;
            db.SaveChanges();

            decimal subtotal = item.Productos.precio_venta * cantidad;
            decimal totalCarrito = db.Carrito
                .Where(c => c.ID_Usuario == idUsuario)
                .Sum(c => c.Cantidad * c.Productos.precio_venta);
            int totalItems = db.Carrito
                .Where(c => c.ID_Usuario == idUsuario)
                .Sum(c => c.Cantidad);

            return Json(new
            {
                success = true,
                message = "Carrito actualizado",
                subtotal,
                totalCarrito,
                totalItems
            });
        }

        // POST: Carrito/Eliminar
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(CarritoCrudViewModel.EliminarDelCarritoViewModel viewModel)
        {
            var itemCarrito = db.Carrito.Find(viewModel.ID_Carrito);

            if (itemCarrito == null)
            {
                TempData["Message"] = "Producto no encontrado en el carrito";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            // Verificar que el ítem pertenezca al usuario actual
            if (itemCarrito.ID_Usuario != Convert.ToInt32(Session["UserID"]))
            {
                TempData["Message"] = "No tienes permiso para eliminar este item";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            db.Carrito.Remove(itemCarrito);
            db.SaveChanges();

            TempData["Message"] = "Producto eliminado del carrito";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // POST: Carrito/EliminarAjax
        [HttpPost]
        public JsonResult EliminarAjax(int idCarrito)
        {
            if (Session["UserID"] == null)
                return Json(new { success = false, message = "Debe iniciar sesión" });

            int idUsuario = Convert.ToInt32(Session["UserID"]);
            var item = db.Carrito.Find(idCarrito);
            if (item == null || item.ID_Usuario != idUsuario)
                return Json(new { success = false, message = "Item no encontrado o sin permiso" });

            db.Carrito.Remove(item);
            db.SaveChanges();

            decimal totalCarrito = db.Carrito
                .Where(c => c.ID_Usuario == idUsuario)
                .Select(c => c.Cantidad * c.Productos.precio_venta)
                .DefaultIfEmpty(0m)
                .Sum();

            int totalItems = db.Carrito
                .Where(c => c.ID_Usuario == idUsuario)
                .Select(c => c.Cantidad)
                .DefaultIfEmpty(0)
                .Sum();


            return Json(new
            {
                success = true,
                message = "Producto eliminado",
                totalCarrito,
                totalItems
            });
        }


        // POST: Carrito/VaciarCarrito
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult VaciarCarrito()
        {
            int idUsuario = Convert.ToInt32(Session["UserID"]);

            var itemsCarrito = db.Carrito.Where(c => c.ID_Usuario == idUsuario);
            db.Carrito.RemoveRange(itemsCarrito);
            db.SaveChanges();

            TempData["Message"] = "Carrito vaciado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Carrito/ProcederPago
        public ActionResult ProcederPago()
        {
            // Si el usuario no está autenticado, redirigir al login con mensaje
            if (Session["UserID"] == null)
            {
                TempData["Message"] = "Debes iniciar sesión para proceder al pago";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Login", "Usuarios");
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);

            var carritoItems = db.Carrito
                .Include(c => c.Productos)
                .Where(c => c.ID_Usuario == idUsuario)
                .ToList();

            if (carritoItems.Count == 0)
            {
                TempData["Message"] = "El carrito está vacío";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Index");
            }

            // Subtotal del carrito
            decimal subtotal = carritoItems.Sum(item => item.Productos.precio_venta * item.Cantidad);

            // Calcular IVA (16%)
            decimal iva = subtotal * 0.16m;

            // Calcular gastos de envío (pueden ser fijos o basados en el total)
            decimal gastosEnvio = (subtotal > 1000) ? 0 : 99;

            // Por defecto, no hay descuento
            decimal descuento = 0;

            var viewModel = new CarritoViewModel
            {
                ID_Usuario = idUsuario,
                Items = carritoItems.Select(item => new CarritoItemViewModel
                {
                    ID_Carrito = item.ID_Carrito,
                    ID_Producto = item.ID_Producto,
                    ID_Usuario = item.ID_Usuario,
                    NombreProducto = item.Productos.Nombre,
                    // No hay ImagenUrl en el modelo Productos, usamos ruta predeterminada
                    ImagenUrl = "/Content/images/no-image.png",
                    PrecioUnitario = item.Productos.precio_venta,
                    Cantidad = item.Cantidad,
                    FechaAgregado = item.FechaAgregado
                }).ToList(),
                InformacionPago = new PagoViewModel
                {
                    Subtotal = subtotal,
                    Impuesto = iva,
                    GastosEnvio = gastosEnvio,
                    Descuento = descuento
                }
            };

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

        // GET: Carrito/ObtenerTotalElementos
        [ChildActionOnly]
        public ActionResult ObtenerTotalElementos()
        {
            try
            {
                int totalItems = 0;

                if (Session["UserID"] != null)
                {
                    try
                    {
                        int idUsuario = Convert.ToInt32(Session["UserID"]);

                        using (var connection = new System.Data.SqlClient.SqlConnection(
                            System.Configuration.ConfigurationManager.ConnectionStrings["FarmaUEntities"].ConnectionString))
                        {
                            connection.Open();

                            var command = new System.Data.SqlClient.SqlCommand(
                                "SELECT SUM(Cantidad) FROM Carrito WHERE ID_Usuario = @idUsuario",
                                connection);

                            command.Parameters.AddWithValue("@idUsuario", idUsuario);

                            var result = command.ExecuteScalar();
                            if (result != DBNull.Value && result != null)
                            {
                                totalItems = Convert.ToInt32(result);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log del error
                        System.Diagnostics.Debug.WriteLine("Error al obtener total de elementos: " + ex.Message);
                        totalItems = 0;
                    }
                }

                return Content(totalItems.ToString());
            }
            catch (Exception ex)
            {
                // En caso de error, simplemente devolvemos 0
                System.Diagnostics.Debug.WriteLine("Error general en ObtenerTotalElementos: " + ex.Message);
                return Content("0");
            }
        }

        // GET: Carrito/ObtenerResumenCarrito
        [ChildActionOnly]
        public ActionResult ObtenerResumenCarrito()
        {
            if (Session["UserID"] == null)
            {
                return PartialView("_ResumenCarritoMini", new CarritoViewModel());
            }

            try
            {
                int idUsuario = Convert.ToInt32(Session["UserID"]);

                var carritoItems = db.Carrito
                    .Include(c => c.Productos)
                    .Where(c => c.ID_Usuario == idUsuario)
                    .ToList();

                // Verificar si hay items
                if (carritoItems == null || !carritoItems.Any())
                {
                    return PartialView("_ResumenCarritoMini", new CarritoViewModel { ID_Usuario = idUsuario });
                }

                // Si hay items, crear el viewmodel
                var viewModel = new CarritoViewModel
                {
                    ID_Usuario = idUsuario,
                    Items = carritoItems.Select(item => new CarritoItemViewModel
                    {
                        ID_Carrito = item.ID_Carrito,
                        ID_Producto = item.ID_Producto,
                        ID_Usuario = item.ID_Usuario,
                        NombreProducto = item.Productos.Nombre,
                        // No hay ImagenUrl en el modelo Productos, usamos una ruta predeterminada
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = item.Productos.precio_venta,
                        Cantidad = item.Cantidad,
                        FechaAgregado = item.FechaAgregado
                    }).ToList()
                };

                return PartialView("_ResumenCarritoMini", viewModel);
            }
            catch (Exception ex)
            {
                // Registrar el error para depuración
                System.Diagnostics.Debug.WriteLine("Error al obtener resumen del carrito: " + ex.Message);

                // Devolver un viewmodel vacío en caso de error
                return PartialView("_ResumenCarritoMini", new CarritoViewModel
                {
                    ID_Usuario = Convert.ToInt32(Session["UserID"])
                });
            }
        }

        // GET: Carrito/AgregarProductosDePrueba
        public ActionResult AgregarProductosDePrueba()
        {
            if (Session["UserID"] == null)
            {
                TempData["Message"] = "Debes iniciar sesión para realizar esta acción";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Login", "Usuarios");
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);
            List<string> resultados = new List<string>();

            try
            {
                // Obtener algunos productos aleatorios de la base de datos
                var productos = db.Productos.Where(p => p.estado == "Activo").Take(3).ToList();

                if (!productos.Any())
                {
                    TempData["Message"] = "No hay productos disponibles para agregar";
                    TempData["MessageType"] = "warning";
                    return RedirectToAction("Index");
                }

                foreach (var producto in productos)
                {
                    // Generar una cantidad aleatoria entre 1 y 3
                    int cantidad = new Random().Next(1, 4);

                    // Verificar si el producto ya está en el carrito
                    var itemExistente = db.Carrito.FirstOrDefault(c =>
                        c.ID_Usuario == idUsuario && c.ID_Producto == producto.ID_Producto);

                    if (itemExistente != null)
                    {
                        // Actualizar cantidad si ya existe
                        itemExistente.Cantidad += cantidad;
                        itemExistente.FechaAgregado = DateTime.Now;
                        resultados.Add($"Actualizado: {producto.Nombre} (+{cantidad})");
                    }
                    else
                    {
                        // Crear nuevo item en el carrito
                        var nuevoItem = new Carrito
                        {
                            ID_Usuario = idUsuario,
                            ID_Producto = producto.ID_Producto,
                            Cantidad = cantidad,
                            FechaAgregado = DateTime.Now
                        };

                        db.Carrito.Add(nuevoItem);
                        resultados.Add($"Agregado: {producto.Nombre} (x{cantidad})");
                    }
                }

                db.SaveChanges();

                TempData["Message"] = "Productos de prueba agregados al carrito: " + string.Join(", ", resultados);
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error al agregar productos de prueba: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("Index");
        }


        // Método para procesar el pago y generar factura
        [HttpPost]
        [Authorize] // Solo usuarios autenticados
        [ValidateAntiForgeryToken]
        public ActionResult ProcesarPago(PagoViewModel infoPago)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error en los datos de pago.";
                TempData["MessageType"] = "error";
                return RedirectToAction("ProcederPago");
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);

            // Obtener los ítems del carrito
            var carritoItems = db.Carrito
                .Include(c => c.Productos)
                .Where(c => c.ID_Usuario == idUsuario)
                .ToList();

            if (!carritoItems.Any())
            {
                TempData["Message"] = "El carrito está vacío.";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Index");
            }

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Crear la factura
                    var factura = new Facturas
                    {
                        ID_Cliente = ObtenerClienteIdPorUsuario(idUsuario), // Implementar este método
                        fecha = DateTime.Now,
                        total = carritoItems.Sum(item => item.Productos.precio_venta * item.Cantidad),
                        estado = "pagado" // Estado inicial
                    };

                    db.Facturas.Add(factura);
                    db.SaveChanges();

                    // 2. Crear los detalles de la factura
                    foreach (var item in carritoItems)
                    {
                        var detalle = new Detalles_Factura
                        {
                            id_Factura = factura.id_Factura,
                            ID_Producto = item.ID_Producto,
                            cantidad = item.Cantidad,
                            subtotal = item.Productos.precio_venta * item.Cantidad
                        };

                        db.Detalles_Factura.Add(detalle);
                    }
                    db.SaveChanges();

                    // 3. Actualizar el inventario (restar de lotes)
                    foreach (var item in carritoItems)
                    {
                        bool stockReducido = ReducirStockDeLotes(item.ID_Producto, item.Cantidad, factura.id_Factura);

                        if (!stockReducido)
                        {
                            // Si no hay suficiente stock, revertir transacción
                            dbTransaction.Rollback();
                            TempData["Message"] = "No hay suficiente stock para el producto: " + item.Productos.Nombre;
                            TempData["MessageType"] = "error";
                            return RedirectToAction("ProcederPago");
                        }
                    }

                    // 4. Vaciar el carrito
                    var itemsCarrito = db.Carrito.Where(c => c.ID_Usuario == idUsuario);
                    db.Carrito.RemoveRange(itemsCarrito);
                    db.SaveChanges();

                    // Confirmar toda la transacción
                    dbTransaction.Commit();

                    TempData["Message"] = "¡Compra realizada con éxito! Su factura ha sido generada.";
                    TempData["MessageType"] = "success";
                    TempData["FacturaId"] = factura.id_Factura;

                    return RedirectToAction("DetalleFactura", "Usuarios", new { id = factura.id_Factura });
                }
                catch (Exception ex)
                {
                    dbTransaction.Rollback();
                    TempData["Message"] = "Error al procesar el pago: " + ex.Message;
                    TempData["MessageType"] = "error";
                    return RedirectToAction("ProcederPago");
                }
            }
        }

        // Método para obtener el ID del cliente asociado al usuario
        // Método corregido para obtener el ID del cliente asociado al usuario
        // Método para obtener el ID del cliente asociado al usuario
        // Método para obtener el ID del cliente asociado al usuario
        private int ObtenerClienteIdPorUsuario(int idUsuario)
        {
            // Obtenemos el usuario
            var usuario = db.Usuarios.Find(idUsuario);

            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            // Obtenemos el ID_Persona del usuario
            int idPersona = usuario.ID_Persona;

            // Buscamos el cliente que corresponde a esa persona
            var cliente = db.Clientes.FirstOrDefault(c => c.ID_Persona == idPersona);

            if (cliente == null)
            {
                // Si no existe un cliente para esta persona, creamos uno
                cliente = new Clientes
                {
                    ID_Persona = idPersona
                };

                db.Clientes.Add(cliente);
                db.SaveChanges();
            }

            return cliente.id_cliente;
        }


        // Método para reducir el stock de los lotes y registrar movimientos
        private bool ReducirStockDeLotes(int idProducto, int cantidad, int idFactura)
        {
            // Obtener lotes disponibles ordenados por fecha de vencimiento (FIFO)
            var lotes = db.Lotes
                .Where(l => l.ID_Producto == idProducto && l.cantidad > 0)
                .OrderBy(l => l.fecha_vencimiento)
                .ToList();

            int cantidadPendiente = cantidad;

            foreach (var lote in lotes)
            {
                if (cantidadPendiente <= 0) break;

                // Determinar cuánto tomar de este lote
                int cantidadARestar = Math.Min(lote.cantidad, cantidadPendiente);

                // Actualizar el lote
                lote.cantidad -= cantidadARestar;
                db.Entry(lote).State = EntityState.Modified;

                // Registrar movimiento de inventario
                var movimiento = new Movimientos_Inventario
                {
                    id_Producto = idProducto,
                    id_Lote = lote.id_Lote,
                    tipo = "Venta",
                    cantidad = cantidadARestar, // Cantidad positiva aunque sea salida
                    fecha = DateTime.Now,
                    ID_Factura = idFactura
                };

                db.Movimientos_Inventario.Add(movimiento);

                cantidadPendiente -= cantidadARestar;
            }

            // Si aún quedan unidades pendientes, no hay suficiente stock
            return cantidadPendiente <= 0;
        }

        // POST: Carrito/ConfirmarPedido (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ConfirmarPedido(ConfirmarPedidoViewModel vm)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Formulario inválido" });

            if (Session["UserID"] == null)
                return Json(new { success = false, message = "Debe iniciar sesión" });

            int idUsuario = Convert.ToInt32(Session["UserID"]);
            var items = db.Carrito
                .Include(c => c.Productos)
                .Where(c => c.ID_Usuario == idUsuario)
                .ToList();
            if (!items.Any())
                return Json(new { success = false, message = "Carrito vacío" });

            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var factura = new Facturas
                    {
                        ID_Cliente = ObtenerClienteIdPorUsuario(idUsuario),
                        fecha      = DateTime.Now,
                        total      = items.Sum(i => i.Productos.precio_venta * i.Cantidad),
                        estado     = "pagado"
                    };
                    db.Facturas.Add(factura);
                    db.SaveChanges();

                    foreach (var it in items)
                    {
                        db.Detalles_Factura.Add(new Detalles_Factura {
                            id_Factura  = factura.id_Factura,
                            ID_Producto = it.ID_Producto,
                            cantidad    = it.Cantidad,
                            subtotal    = it.Productos.precio_venta * it.Cantidad
                        });

                        int pendiente = it.Cantidad;
                        var lotes = db.Lotes
                            .Where(l => l.ID_Producto == it.ID_Producto && l.cantidad > 0)
                            .OrderBy(l => l.fecha_vencimiento)
                            .ToList();

                        foreach (var lote in lotes)
                        {
                            if (pendiente <= 0) break;
                            int resta = Math.Min(lote.cantidad, pendiente);
                            lote.cantidad -= resta;
                            db.Movimientos_Inventario.Add(new Movimientos_Inventario {
                                id_Producto = it.ID_Producto,
                                id_Lote      = lote.id_Lote,
                                tipo         = "Venta",
                                cantidad     = resta * -1,
                                fecha        = DateTime.Now,
                                ID_Factura   = factura.id_Factura
                            });
                            pendiente -= resta;
                        }
                        if (pendiente > 0)
                            throw new Exception("Stock insuficiente para " + it.Productos.Nombre);
                    }

                    db.Carrito.RemoveRange(items);
                    db.SaveChanges();
                    tx.Commit();

                    return Json(new { success = true, facturaId = factura.id_Factura });
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return Json(new { success = false, message = ex.Message });
                }
            }
        }

        

    }
}