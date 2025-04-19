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
            // Si el usuario no está autenticado, redirigir al login con mensaje
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
                    ID_Usuario = item.ID_Usuario,
                    NombreProducto = item.Productos.Nombre,
                    // No hay ImagenUrl en el modelo Productos, usamos ruta predeterminada
                    ImagenUrl = "/Content/images/no-image.png",
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

        // POST: Carrito/AgregarAjax
        [HttpPost]
        [Authorize]
        public JsonResult AgregarAjax(int idProducto, int cantidad = 1)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Debe iniciar sesión para agregar productos al carrito" });
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);

            try
            {
                // Verificar si el producto existe
                var producto = db.Productos.Find(idProducto);
                if (producto == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                // Verificar si el producto ya está en el carrito
                var itemExistente = db.Carrito.FirstOrDefault(c =>
                    c.ID_Usuario == idUsuario && c.ID_Producto == idProducto);

                if (itemExistente != null)
                {
                    // Actualizar cantidad si ya existe
                    itemExistente.Cantidad += cantidad;
                }
                else
                {
                    // Crear nuevo item en el carrito
                    var nuevoItem = new Carrito
                    {
                        ID_Usuario = idUsuario,
                        ID_Producto = idProducto,
                        Cantidad = cantidad,
                        FechaAgregado = DateTime.Now
                    };

                    db.Carrito.Add(nuevoItem);
                }

                db.SaveChanges();

                // Calcular total de items en el carrito para actualizar el contador
                int totalItems = db.Carrito
                    .Where(c => c.ID_Usuario == idUsuario)
                    .Sum(c => c.Cantidad);

                return Json(new
                {
                    success = true,
                    message = $"{producto.Nombre} agregado al carrito",
                    totalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al agregar al carrito: " + ex.Message });
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
        [Authorize]
        public JsonResult ActualizarAjax(int idCarrito, int cantidad)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Debe iniciar sesión para actualizar el carrito" });
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);

            try
            {
                var itemCarrito = db.Carrito.Find(idCarrito);

                if (itemCarrito == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado en el carrito" });
                }

                // Verificar que el ítem pertenezca al usuario actual
                if (itemCarrito.ID_Usuario != idUsuario)
                {
                    return Json(new { success = false, message = "No tienes permiso para modificar este item" });
                }

                itemCarrito.Cantidad = cantidad;
                db.SaveChanges();

                // Recalcular totales
                var producto = db.Productos.Find(itemCarrito.ID_Producto);
                decimal subtotal = producto.precio_venta * cantidad;

                // Calcular el nuevo total del carrito
                decimal totalCarrito = db.Carrito
                    .Where(c => c.ID_Usuario == idUsuario)
                    .Join(db.Productos, c => c.ID_Producto, p => p.ID_Producto, (c, p) => new { c.Cantidad, p.precio_venta })
                    .Sum(x => x.Cantidad * x.precio_venta);

                // Calcular total de items
                int totalItems = db.Carrito
                    .Where(c => c.ID_Usuario == idUsuario)
                    .Sum(c => c.Cantidad);

                return Json(new
                {
                    success = true,
                    message = "Carrito actualizado",
                    subtotal = subtotal,
                    totalCarrito = totalCarrito,
                    totalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el carrito: " + ex.Message });
            }
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
        [Authorize]
        public JsonResult EliminarAjax(int idCarrito)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Debe iniciar sesión para eliminar productos del carrito" });
            }

            int idUsuario = Convert.ToInt32(Session["UserID"]);

            try
            {
                var itemCarrito = db.Carrito.Find(idCarrito);

                if (itemCarrito == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado en el carrito" });
                }

                // Verificar que el ítem pertenezca al usuario actual
                if (itemCarrito.ID_Usuario != idUsuario)
                {
                    return Json(new { success = false, message = "No tienes permiso para eliminar este item" });
                }

                db.Carrito.Remove(itemCarrito);
                db.SaveChanges();

                // Calcular el nuevo total del carrito
                decimal totalCarrito = db.Carrito
                    .Where(c => c.ID_Usuario == idUsuario)
                    .Join(db.Productos, c => c.ID_Producto, p => p.ID_Producto, (c, p) => new { c.Cantidad, p.precio_venta })
                    .Sum(x => x.Cantidad * x.precio_venta);

                // Calcular total de items
                int totalItems = db.Carrito
                    .Where(c => c.ID_Usuario == idUsuario)
                    .Sum(c => c.Cantidad);

                return Json(new
                {
                    success = true,
                    message = "Producto eliminado del carrito",
                    totalCarrito = totalCarrito,
                    totalItems = totalItems
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar del carrito: " + ex.Message });
            }
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
    }
}