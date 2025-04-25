using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    [Authorize] // Requiere autenticación para todas las acciones
    public class PedidosController : Controller
    {
        private readonly FarmaUEntities _db;

        // Constructor con inyección de dependencia
        public PedidosController(FarmaUEntities db)
        {
            _db = db;
        }

        // Constructor sin parámetros para cuando no se usa DI
        public PedidosController()
        {
            _db = new FarmaUEntities();
        }

        // GET: Pedidos
        public ActionResult Index()
        {
            int idUsuario = ObtenerIdUsuario();

            // En una implementación real, aquí se obtendrían los pedidos de la base de datos
            // Por ahora, generamos datos de ejemplo
            var pedidos = GenerarPedidosEjemplo(idUsuario);

            return View(pedidos);
        }

        // GET: Pedidos/Detalles/5
        public ActionResult Detalles(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int idUsuario = ObtenerIdUsuario();

            // En una implementación real, aquí se obtendría el pedido específico de la base de datos
            // Por ahora, generamos datos de ejemplo
            var pedidos = GenerarPedidosEjemplo(idUsuario);
            var pedido = pedidos.FirstOrDefault(p => p.ID_Pedido == id);

            if (pedido == null)
            {
                return HttpNotFound();
            }

            // Verificar que el pedido pertenezca al usuario actual
            // (En implementación real se verificaría con la base de datos)

            // Obtenemos los detalles completos del pedido
            var detallesPedido = ObtenerDetallesPedido(pedido);

            return View(detallesPedido);
        }

        // POST: Pedidos/Cancelar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancelar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int idUsuario = ObtenerIdUsuario();

            // En una implementación real, aquí verificaríamos si el pedido existe
            // y si pertenece al usuario actual
            var pedidos = GenerarPedidosEjemplo(idUsuario);
            var pedido = pedidos.FirstOrDefault(p => p.ID_Pedido == id);

            if (pedido == null)
            {
                return HttpNotFound();
            }

            // Verificar si el pedido está en un estado que permite cancelación
            if (pedido.Estado != "Procesando")
            {
                TempData["Message"] = "Este pedido no puede ser cancelado debido a su estado actual";
                TempData["MessageType"] = "error";
                TempData["UseSweet"] = true;
                return RedirectToAction("Detalles", new { id = id });
            }

            // En una implementación real, aquí actualizaríamos el estado del pedido en la base de datos

            TempData["Message"] = "El pedido ha sido cancelado correctamente";
            TempData["MessageType"] = "success";
            TempData["UseSweet"] = true;

            return RedirectToAction("Detalles", new { id = id });
        }

        // POST: Pedidos/Valorar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Valorar(ValoracionPedidoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Ha ocurrido un error al procesar la valoración";
                TempData["MessageType"] = "error";
                return RedirectToAction("Detalles", new { id = model.ID_Pedido });
            }

            int idUsuario = ObtenerIdUsuario();

            // En una implementación real, aquí verificaríamos si el pedido existe
            // y si pertenece al usuario actual
            var pedidos = GenerarPedidosEjemplo(idUsuario);
            var pedido = pedidos.FirstOrDefault(p => p.ID_Pedido == model.ID_Pedido);

            if (pedido == null)
            {
                return HttpNotFound();
            }

            // Verificar si el pedido está en un estado que permite valoración
            if (pedido.Estado != "Entregado" || pedido.Calificado)
            {
                TempData["Message"] = "Este pedido no puede ser valorado o ya ha sido valorado anteriormente";
                TempData["MessageType"] = "error";
                TempData["UseSweet"] = true;
                return RedirectToAction("Detalles", new { id = model.ID_Pedido });
            }

            // En una implementación real, aquí guardaríamos la valoración en la base de datos

            TempData["Message"] = "Gracias por tu valoración";
            TempData["MessageType"] = "success";
            TempData["UseSweet"] = true;

            return RedirectToAction("Detalles", new { id = model.ID_Pedido });
        }

        // POST: Pedidos/DescargarFactura
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DescargarFactura(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            int idUsuario = ObtenerIdUsuario();

            // En una implementación real, aquí verificaríamos si el pedido existe
            // y si pertenece al usuario actual
            var pedidos = GenerarPedidosEjemplo(idUsuario);
            var pedido = pedidos.FirstOrDefault(p => p.ID_Pedido == id);

            if (pedido == null)
            {
                return HttpNotFound();
            }

            // En una implementación real, aquí generaríamos un PDF con la factura
            // y lo devolveríamos como un FileResult

            // Por ahora, simplemente redirigimos con un mensaje
            TempData["Message"] = "La factura ha sido generada y descargada correctamente";
            TempData["MessageType"] = "success";
            TempData["UseSweet"] = true;

            return RedirectToAction("Detalles", new { id = id });
        }

        #region Métodos Privados

        // Método para obtener el ID del usuario actual
        private int ObtenerIdUsuario()
        {
            if (Session["UserID"] == null)
            {
                throw new HttpException(401, "Usuario no autenticado");
            }

            return Convert.ToInt32(Session["UserID"]);
        }

        // Método para generar datos de ejemplo para los pedidos
        private List<PedidoViewModel> GenerarPedidosEjemplo(int idUsuario)
        {
            // Ejemplo de pedidos para mostrar en la UI
            var pedidos = new List<PedidoViewModel>
            {
                new PedidoViewModel
                {
                    ID_Pedido = 1,
                    NumeroPedido = "PED-" + (DateTime.Now.Ticks % 1000000).ToString("D6"),
                    FechaPedido = DateTime.Now.AddDays(-1),
                    Subtotal = 850.00m,
                    Impuesto = 136.00m,
                    GastosEnvio = 0.00m,
                    Descuento = 0.00m,
                    Total = 986.00m,
                    Estado = "Procesando",
                    NumeroProductos = 3,
                    Calificado = false
                },
                new PedidoViewModel
                {
                    ID_Pedido = 2,
                    NumeroPedido = "PED-" + (DateTime.Now.Ticks % 1000000 - 10000).ToString("D6"),
                    FechaPedido = DateTime.Now.AddDays(-5),
                    Subtotal = 1250.00m,
                    Impuesto = 200.00m,
                    GastosEnvio = 0.00m,
                    Descuento = 150.00m,
                    Total = 1300.00m,
                    Estado = "Enviado",
                    NumeroProductos = 4,
                    Calificado = false
                },
                new PedidoViewModel
                {
                    ID_Pedido = 3,
                    NumeroPedido = "PED-" + (DateTime.Now.Ticks % 1000000 - 20000).ToString("D6"),
                    FechaPedido = DateTime.Now.AddDays(-10),
                    Subtotal = 450.00m,
                    Impuesto = 72.00m,
                    GastosEnvio = 99.00m,
                    Descuento = 0.00m,
                    Total = 621.00m,
                    Estado = "Entregado",
                    NumeroProductos = 2,
                    Calificado = true
                },
                new PedidoViewModel
                {
                    ID_Pedido = 4,
                    NumeroPedido = "PED-" + (DateTime.Now.Ticks % 1000000 - 30000).ToString("D6"),
                    FechaPedido = DateTime.Now.AddDays(-15),
                    Subtotal = 320.00m,
                    Impuesto = 51.20m,
                    GastosEnvio = 99.00m,
                    Descuento = 0.00m,
                    Total = 470.20m,
                    Estado = "Cancelado",
                    NumeroProductos = 1,
                    Calificado = false
                }
            };

            return pedidos;
        }

        // Método para generar detalles de un pedido específico
        private PedidoDetalleViewModel ObtenerDetallesPedido(PedidoViewModel pedido)
        {
            // Crear el ViewModel de detalles
            var detallesPedido = new PedidoDetalleViewModel
            {
                ID_Pedido = pedido.ID_Pedido,
                NumeroPedido = pedido.NumeroPedido,
                FechaPedido = pedido.FechaPedido,
                Estado = pedido.Estado,
                NumeroSeguimiento = pedido.Estado == "Enviado" || pedido.Estado == "Entregado" ?
                                    "TRACK-" + (DateTime.Now.Ticks % 1000000).ToString("D10") :
                                    null,

                // Información del cliente (aquí usaríamos datos reales del usuario)
                NombreCliente = Session["UserFullName"]?.ToString() ?? "Cliente de Ejemplo",
                DireccionEnvio = "Calle Principal #123",
                CiudadEnvio = "Ciudad Ejemplo",
                CodigoPostal = "12345",
                Telefono = "123-456-7890",
                Email = "cliente@ejemplo.com",

                // Método de pago
                MetodoPago = "Tarjeta de Crédito/Débito",
                UltimosDigitosTarjeta = "4567",

                // Información de costos
                Subtotal = pedido.Subtotal,
                Impuesto = pedido.Impuesto,
                GastosEnvio = pedido.GastosEnvio,
                Descuento = pedido.Descuento,
                Total = pedido.Total,

                // Valoración
                Calificado = pedido.Calificado,
                Valoracion = pedido.Calificado ? 5 : (int?)null,
                Comentario = pedido.Calificado ? "Excelente servicio y productos de alta calidad." : null,

                // Productos (ejemplos)
                Productos = ObtenerProductosPedido(pedido.ID_Pedido),

                // Historial de estados
                HistorialEstados = ObtenerHistorialEstados(pedido)
            };

            return detallesPedido;
        }

        // Método para obtener los productos de un pedido
        private List<PedidoItemViewModel> ObtenerProductosPedido(int idPedido)
        {
            // En una implementación real, aquí obtendríamos los productos del pedido desde la base de datos
            // Por ahora, generamos datos de ejemplo según el ID del pedido
            var productos = new List<PedidoItemViewModel>();

            switch (idPedido)
            {
                case 1:
                    productos.Add(new PedidoItemViewModel
                    {
                        ID_Producto = 101,
                        NombreProducto = "Paracetamol 500mg",
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = 150.00m,
                        Cantidad = 2
                    });
                    productos.Add(new PedidoItemViewModel
                    {
                        ID_Producto = 102,
                        NombreProducto = "Vitamina C 1000mg",
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = 250.00m,
                        Cantidad = 1
                    });
                    productos.Add(new PedidoItemViewModel
                    {
                        ID_Producto = 103,
                        NombreProducto = "Ibuprofeno 400mg",
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = 300.00m,
                        Cantidad = 1
                    });
                    break;
                case 2:
                    productos.Add(new PedidoItemViewModel
                    {
                        ID_Producto = 201,
                        NombreProducto = "Suplemento Omega 3",
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = 350.00m,
                        Cantidad = 2
                    });
                    productos.Add(new PedidoItemViewModel
                    {
                        ID_Producto = 202,
                        NombreProducto = "Proteína en Polvo",
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = 550.00m,
                        Cantidad = 1
                    });
                    break;
                case 3:
                    productos.Add(new PedidoItemViewModel
                    {
                        ID_Producto = 301,
                        NombreProducto = "Crema Hidratante",
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = 250.00m,
                        Cantidad = 1
                    });
                    productos.Add(new PedidoItemViewModel
                    {
                        ID_Producto = 302,
                        NombreProducto = "Champú Anticaspa",
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = 200.00m,
                        Cantidad = 1
                    });
                    break;
                case 4:
                    productos.Add(new PedidoItemViewModel
                    {
                        ID_Producto = 401,
                        NombreProducto = "Termómetro Digital",
                        ImagenUrl = "/Content/images/no-image.png",
                        PrecioUnitario = 320.00m,
                        Cantidad = 1
                    });
                    break;
            }

            return productos;
        }

        // Método para generar el historial de estados de un pedido
        private List<PedidoEstadoViewModel> ObtenerHistorialEstados(PedidoViewModel pedido)
        {
            var historial = new List<PedidoEstadoViewModel>();

            // Siempre añadimos el estado "Pedido recibido" como primero
            historial.Add(new PedidoEstadoViewModel
            {
                Estado = "Pedido recibido",
                Fecha = pedido.FechaPedido,
                Descripcion = "Tu pedido ha sido recibido y está siendo procesado",
                IconoEstado = "check-lg",
                ColorEstado = "success"
            });

            // Dependiendo del estado actual, añadimos más estados
            switch (pedido.Estado)
            {
                case "Procesando":
                    historial.Add(new PedidoEstadoViewModel
                    {
                        Estado = "En preparación",
                        Fecha = pedido.FechaPedido.AddHours(2),
                        Descripcion = "Estamos preparando tu pedido",
                        IconoEstado = "box",
                        ColorEstado = "primary"
                    });
                    break;

                case "Enviado":
                    historial.Add(new PedidoEstadoViewModel
                    {
                        Estado = "En preparación",
                        Fecha = pedido.FechaPedido.AddHours(3),
                        Descripcion = "Tu pedido ha sido preparado",
                        IconoEstado = "box",
                        ColorEstado = "info"
                    });
                    historial.Add(new PedidoEstadoViewModel
                    {
                        Estado = "Enviado",
                        Fecha = pedido.FechaPedido.AddDays(1),
                        Descripcion = "Tu pedido ha sido enviado",
                        IconoEstado = "truck",
                        ColorEstado = "primary"
                    });
                    break;

                case "Entregado":
                    historial.Add(new PedidoEstadoViewModel
                    {
                        Estado = "En preparación",
                        Fecha = pedido.FechaPedido.AddHours(2),
                        Descripcion = "Tu pedido ha sido preparado",
                        IconoEstado = "box",
                        ColorEstado = "info"
                    });
                    historial.Add(new PedidoEstadoViewModel
                    {
                        Estado = "Enviado",
                        Fecha = pedido.FechaPedido.AddDays(1),
                        Descripcion = "Tu pedido ha sido enviado",
                        IconoEstado = "truck",
                        ColorEstado = "primary"
                    });
                    historial.Add(new PedidoEstadoViewModel
                    {
                        Estado = "Entregado",
                        Fecha = pedido.FechaPedido.AddDays(3),
                        Descripcion = "Tu pedido ha sido entregado",
                        IconoEstado = "house-door",
                        ColorEstado = "success"
                    });
                    break;

                case "Cancelado":
                    historial.Add(new PedidoEstadoViewModel
                    {
                        Estado = "Cancelado",
                        Fecha = pedido.FechaPedido.AddHours(3),
                        Descripcion = "El pedido ha sido cancelado",
                        IconoEstado = "x-circle",
                        ColorEstado = "danger"
                    });
                    break;
            }

            return historial;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}