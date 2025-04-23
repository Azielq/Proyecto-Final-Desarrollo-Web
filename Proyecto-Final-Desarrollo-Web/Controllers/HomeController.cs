using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class HomeController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        // GET: De Farmacia
        public ActionResult Index2()
        {
            try
            {
                // Crea el ViewModel principal
                var viewModel = new TiendaViewModel
                {
                    TituloPagina = "FarmaU - Tu farmacia online",
                    DescripcionPagina = "Bienvenido a FarmaU, tu farmacia de confianza online. Encontrarás medicamentos, productos de cuidado personal, vitaminas y más."
                };

                // Obtiene categorías destacadas (limitadas a 6)
                viewModel.Categorias = db.Categorias
                    .Take(6)
                    .ToList()
                    .Select(c => CategoriaViewModel.FromEntity(c))
                    .ToList();

                // Obtiene productos destacados (puede ser basado en ventas, novedad, etc.)
                viewModel.ProductosDestacados = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Imagenes_Producto)
                    .Include(p => p.Lotes)
                    .Where(p => p.estado == "Activo")
                    .OrderByDescending(p => p.Detalles_Factura.Sum(df => df.cantidad))
                    .Take(8)
                    .ToList()
                    .Select(p => ProductoTiendaViewModel.FromEntity(p))
                    .ToList();

                // Simula productos en oferta (en un entorno real, podría haber una tabla de ofertas o en Json)
                var random = new Random();
                viewModel.Ofertas = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Imagenes_Producto)
                    .Include(p => p.Lotes)
                    .Where(p => p.estado == "Activo")
                    .OrderBy(p => Guid.NewGuid()) // Ordenamiento aleatorio
                    .Take(4)
                    .ToList()
                    .Select(p => ProductoTiendaViewModel.FromEntity(p, (decimal)random.Next(10, 30) / 100)) // Descuento aleatorio entre 10% y 30%
                    .ToList();

                // Obtiene productos nuevos (basado en criterios alternativos como últimos añadidos al inventario)
                viewModel.NuevosProductos = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Imagenes_Producto)
                    .Include(p => p.Lotes)
                    .Where(p => p.estado == "Activo")
                    .OrderByDescending(p => p.ID_Producto) // Usamos el ID como aproximación para los más recientes
                    .Take(8)
                    .ToList()
                    .Select(p => ProductoTiendaViewModel.FromEntity(p))
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Registra el error
                System.Diagnostics.Debug.WriteLine($"Error en Index2: {ex.Message}");
                ViewBag.ErrorMessage = "Ocurrió un error al cargar la página principal: " + ex.Message;

                // Retorna una vista con modelo vacío
                return View(new TiendaViewModel
                {
                    TituloPagina = "FarmaU - Tu farmacia online",
                    DescripcionPagina = "Bienvenido a FarmaU, tu farmacia de confianza online."
                });
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        // GET: Home/Tienda - Vista principal de la tienda con filtros
        public ActionResult Tienda(FiltroTiendaViewModel filtros = null, int pagina = 1)
        {
            try
            {
                // Si filtros es null, inicializa
                if (filtros == null)
                    filtros = new FiltroTiendaViewModel();

                // Configura la paginación
                int productosPorPagina = 12;

                // Guarda los valores de los filtros en ViewBag para usar en la vista
                ViewBag.TipoProducto = Request.QueryString.GetValues("Filtros.TipoProducto");
                ViewBag.RequiereReceta = Request.QueryString["Filtros.RequiereReceta"];
                ViewBag.GenericoDisponible = Request.QueryString["Filtros.GenericoDisponible"];

                // Consulta base de productos
                var query = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Imagenes_Producto)
                    .Include(p => p.Lotes)
                    .Where(p => p.estado == "Activo");

                // Aplica filtros
                // Por categoría
                if (filtros.CategoriaId.HasValue)
                {
                    query = query.Where(p => p.ID_Categoría == filtros.CategoriaId.Value);
                }

                // Por búsqueda
                if (!string.IsNullOrWhiteSpace(filtros.Busqueda))
                {
                    string busqueda = filtros.Busqueda.ToLower();
                    query = query.Where(p => p.Nombre.ToLower().Contains(busqueda) ||
                                            p.Detalles.ToLower().Contains(busqueda) ||
                                            p.Marca.ToLower().Contains(busqueda) ||
                                            p.Categorias.Nombre.ToLower().Contains(busqueda));
                }

                // Por precio mínimo
                if (filtros.PrecioMinimo.HasValue)
                {
                    query = query.Where(p => p.precio_venta >= filtros.PrecioMinimo.Value);
                }

                // Por precio máximo
                if (filtros.PrecioMaximo.HasValue)
                {
                    query = query.Where(p => p.precio_venta <= filtros.PrecioMaximo.Value);
                }

                // Por marca (si está especificado)
                if (filtros.Marcas != null && filtros.Marcas.Length > 0)
                {
                    query = query.Where(p => filtros.Marcas.Contains(p.Marca));
                }

                // Por tipo de producto (Si viene de checkboxes)
                var tipoProducto = Request.QueryString.GetValues("Filtros.TipoProducto");
                if (tipoProducto != null && tipoProducto.Length > 0)
                {
                    // Usa categorías para filtrar por tipo
                    query = query.Where(p => tipoProducto.Contains(p.Categorias.Nombre) ||
                                           (tipoProducto.Contains("Medicamento") && p.Categorias.Nombre.Contains("Medicamento")) ||
                                           (tipoProducto.Contains("Cuidado") && p.Categorias.Nombre.Contains("Cuidado Personal")) ||
                                           (tipoProducto.Contains("Vitamina") && p.Categorias.Nombre.Contains("Vitamina")));
                }

                // Solo en stock
                if (filtros.SoloEnStock)
                {
                    query = query.Where(p => p.Lotes.Sum(l => l.cantidad) > 0);
                }

                // Solo ofertas (simulado)
                if (filtros.SoloOfertas)
                {
                    // Aquí podrías tener una columna en la BD para indicar si es oferta
                    // Como simulación, seleccionamos productos con precios terminados en 9
                    query = query.Where(p => p.precio_venta % 10 == 9);
                }

                // Solo nuevos (simulado)
                if (filtros.SoloNuevos)
                {
                    // Simulación: productos con ID alto son los más nuevos
                    query = query.OrderByDescending(p => p.ID_Producto).Take(20);
                }

                // Receta médica
                if (Request.QueryString["Filtros.RequiereReceta"] == "true")
                {
                    // Simulación: asumir que algunos productos requieren receta
                    query = query.Where(p => p.ID_Producto % 2 == 0); // Solo como ejemplo
                }

                // Genérico disponible
                if (Request.QueryString["Filtros.GenericoDisponible"] == "true")
                {
                    // Simulación: asumir que algunos productos tienen genérico
                    query = query.Where(p => p.ID_Producto % 3 == 0); // Solo como ejemplo
                }

                // Aplicamos ordenamiento
                switch (filtros.OrdenarPor ?? "populares")
                {
                    case "precio_asc":
                        query = query.OrderBy(p => p.precio_venta);
                        break;
                    case "precio_desc":
                        query = query.OrderByDescending(p => p.precio_venta);
                        break;
                    case "nombre_asc":
                        query = query.OrderBy(p => p.Nombre);
                        break;
                    case "nombre_desc":
                        query = query.OrderByDescending(p => p.Nombre);
                        break;
                    case "nuevos":
                        query = query.OrderByDescending(p => p.ID_Producto);
                        break;
                    case "populares":
                    default:
                        query = query.OrderByDescending(p => p.Detalles_Factura.Sum(df => df.cantidad));
                        break;
                }

                // Cuenta el total de productos para la paginación
                int totalProductos = query.Count();
                int totalPaginas = (int)Math.Ceiling((double)totalProductos / productosPorPagina);

                // Asegura que la página solicitada sea válida
                if (pagina < 1) pagina = 1;
                if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

                // Aplica paginación
                var productos = query
                    .Skip((pagina - 1) * productosPorPagina)
                    .Take(productosPorPagina)
                    .ToList();

                // Crea el ViewModel
                var viewModel = new TiendaViewModel
                {
                    TituloPagina = "Tienda FarmaU",
                    DescripcionPagina = "Explora nuestra amplia selección de productos farmacéuticos y de cuidado personal.",
                    Productos = productos.Select(p => ProductoTiendaViewModel.FromEntity(p)).ToList(),
                    Filtros = filtros,
                    TotalProductos = totalProductos,
                    PaginaActual = pagina,
                    ProductosPorPagina = productosPorPagina,
                    TotalPaginas = totalPaginas
                };

                // Obtiene todas las categorías para el filtro lateral
                viewModel.Categorias = db.Categorias
                    .ToList()
                    .Select(c => CategoriaViewModel.FromEntity(c))
                    .ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Registra el error
                System.Diagnostics.Debug.WriteLine($"Error en Tienda: {ex.Message}");
                ViewBag.ErrorMessage = "Ocurrió un error al cargar la tienda: " + ex.Message;

                // Retorna vista con modelo vacío
                return View(new TiendaViewModel
                {
                    TituloPagina = "Tienda FarmaU",
                    DescripcionPagina = "Explora nuestra amplia selección de productos farmacéuticos y de cuidado personal."
                });
            }
        }

        // GET: Home/DetalleProducto
        public ActionResult DetalleProducto(int id)
        {
            try
            {
                // Obtiene el producto con todas sus relaciones
                var producto = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Imagenes_Producto)
                    .Include(p => p.Lotes)
                    .FirstOrDefault(p => p.ID_Producto == id);

                if (producto == null)
                {
                    return HttpNotFound();
                }

                // Convierte el producto a ViewModel
                var productoViewModel = ProductoTiendaViewModel.FromEntity(producto);

                // Obtiene productos relacionados (de la misma categoría)
                var productosRelacionados = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Imagenes_Producto)
                    .Include(p => p.Lotes)
                    .Where(p => p.ID_Categoría == producto.ID_Categoría && p.ID_Producto != id && p.estado == "Activo")
                    .Take(4)
                    .ToList()
                    .Select(p => ProductoTiendaViewModel.FromEntity(p))
                    .ToList();

                // Crea el ViewModel para la vista
                var viewModel = new TiendaViewModel
                {
                    Productos = new List<ProductoTiendaViewModel> { productoViewModel },
                    ProductosDestacados = productosRelacionados,
                    TituloPagina = productoViewModel.Nombre,
                    DescripcionPagina = productoViewModel.Descripcion
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Registra el error
                System.Diagnostics.Debug.WriteLine($"Error en DetalleProducto: {ex.Message}");
                ViewBag.ErrorMessage = "Ocurrió un error al cargar el detalle del producto: " + ex.Message;

                return RedirectToAction("Tienda");
            }
        }

        // GET: Home/Categorias - Vista de categorías principales
        public ActionResult Categorias()
        {
            try
            {
                // Obtiene todas las categorías
                var categorias = db.Categorias
                    .ToList()
                    .Select(c => CategoriaViewModel.FromEntity(c))
                    .ToList();

                // Crea el ViewModel
                var viewModel = new TiendaViewModel
                {
                    Categorias = categorias,
                    TituloPagina = "Categorías",
                    DescripcionPagina = "Explora todas nuestras categorías de productos"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Registra el error
                System.Diagnostics.Debug.WriteLine($"Error en Categorias: {ex.Message}");
                ViewBag.ErrorMessage = "Ocurrió un error al cargar las categorías: " + ex.Message;

                return View(new TiendaViewModel
                {
                    TituloPagina = "Categorías",
                    DescripcionPagina = "Explora todas nuestras categorías de productos"
                });
            }
        }

        // GET: Home/Ofertas - Productos en oferta
        public ActionResult Ofertas()
        {
            try
            {
                // Simula productos en oferta (en un entorno real, podría haber una tabla de ofertas)
                var random = new Random();

                // Consulta productos
                var productosQuery = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Imagenes_Producto)
                    .Include(p => p.Lotes)
                    .Where(p => p.estado == "Activo");

                // Toma una muestra aleatoria para ofertas
                var productosOferta = productosQuery
                    .OrderBy(p => Guid.NewGuid())
                    .Take(20)
                    .ToList()
                    .Select(p => ProductoTiendaViewModel.FromEntity(p, (decimal)random.Next(10, 50) / 100)) // Descuento aleatorio entre 10% y 50%
                    .ToList();

                // Crea el ViewModel
                var viewModel = new TiendaViewModel
                {
                    Productos = productosOferta,
                    TituloPagina = "Ofertas Especiales",
                    DescripcionPagina = "¡Aprovecha nuestras ofertas por tiempo limitado!",
                    TotalProductos = productosOferta.Count,
                    Categorias = db.Categorias.ToList().Select(c => CategoriaViewModel.FromEntity(c)).ToList()
                };

                return View("Tienda", viewModel);
            }
            catch (Exception ex)
            {
                // Registra el error
                System.Diagnostics.Debug.WriteLine($"Error en Ofertas: {ex.Message}");
                ViewBag.ErrorMessage = "Ocurrió un error al cargar las ofertas: " + ex.Message;

                return RedirectToAction("Tienda");
            }
        }

        // GET: Home/Nuevos - Productos nuevos
        public ActionResult Nuevos()
        {
            try
            {
                // Obtiene productos ordenados por ID (suponiendo que los más recientes tienen IDs más altos)
                var productosNuevos = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Imagenes_Producto)
                    .Include(p => p.Lotes)
                    .Where(p => p.estado == "Activo")
                    .OrderByDescending(p => p.ID_Producto)
                    .Take(20)
                    .ToList()
                    .Select(p => ProductoTiendaViewModel.FromEntity(p))
                    .ToList();

                // Marca todos como nuevos para la visualización
                foreach (var producto in productosNuevos)
                {
                    producto.EsNuevo = true;
                }

                // Crea el ViewModel
                var viewModel = new TiendaViewModel
                {
                    Productos = productosNuevos,
                    TituloPagina = "Productos Nuevos",
                    DescripcionPagina = "Descubre los productos más recientes en nuestro catálogo",
                    TotalProductos = productosNuevos.Count,
                    Categorias = db.Categorias.ToList().Select(c => CategoriaViewModel.FromEntity(c)).ToList()
                };

                return View("Tienda", viewModel);
            }
            catch (Exception ex)
            {
                // Registra el error
                System.Diagnostics.Debug.WriteLine($"Error en Nuevos: {ex.Message}");
                ViewBag.ErrorMessage = "Ocurrió un error al cargar los productos nuevos: " + ex.Message;

                return RedirectToAction("Tienda");
            }
        }

        // Método para manejar redirección de categorías desde Index2
        public ActionResult VerCategoria(int id)
        {
            // Crea un filtro preconfigurado para esta categoría
            var filtro = new FiltroTiendaViewModel
            {
                CategoriaId = id,
                OrdenarPor = "populares"
            };

            // Llama a la acción Tienda con el filtro
            return Tienda(filtro);
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