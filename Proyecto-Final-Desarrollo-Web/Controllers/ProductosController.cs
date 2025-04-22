using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using Proyecto_Final_Desarrollo_Web.Helpers;


namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class ProductosController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Productos
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", int? categoriaId = null, string estado = "Activo")
        {
            try
            {
                // Carga listas para filtros
                ViewBag.Categorias = new SelectList(db.Categorias.OrderBy(c => c.Nombre), "ID_Categoría", "Nombre");

                // Consulta base
                var query = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Lotes)
                    .Include(p => p.Imagenes_Producto)
                    .AsQueryable();

                // Aplica filtros
                if (!string.IsNullOrEmpty(estado))
                {
                    query = query.Where(p => p.estado == estado);
                }

                if (categoriaId.HasValue)
                {
                    query = query.Where(p => p.ID_Categoría == categoriaId.Value);
                }

                // Busca por término en varios campos
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchValue = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.Nombre.ToLower().Contains(searchValue) ||
                        p.Marca.ToLower().Contains(searchValue) ||
                        p.Categorias.Nombre.ToLower().Contains(searchValue)
                    );
                }

                // Total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var productos = query
                    .OrderBy(p => p.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configuración de paginación
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los parámetros de búsqueda
                ViewBag.SearchTerm = searchTerm;
                ViewBag.CategoriaId = categoriaId;
                ViewBag.Estado = estado;

                // Transforma a ViewModel
                var productosViewModel = productos.Select(p =>
                {
                    var viewModel = ProductoViewModel.FromEntity(p);

                    // Agrega URL de imagen principal si existe
                    var imagenPrincipal = p.Imagenes_Producto
                        .FirstOrDefault(i => i.Estado && i.EsPrincipal);

                    if (imagenPrincipal != null)
                    {
                        viewModel.UrlImagenPrincipal = imagenPrincipal.URL;
                    }

                    return viewModel;
                }).ToList();

                return View(productosViewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar productos: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los productos: " + ex.Message;
                return View(new List<ProductoViewModel>());
            }
        }

        // POST: Productos/GetProductos para DataTable AJAX
        [HttpPost]
        public ActionResult GetProductos(ProductoTableRequest request)
        {
            try
            {
                var response = new ProductoTableResponse();

                // Consulta base
                var query = db.Productos
                    .Include(p => p.Categorias)
                    .Include(p => p.Lotes)
                    .Include(p => p.Imagenes_Producto)
                    .AsQueryable();

                // Aplica filtros
                if (request.CategoriaId.HasValue)
                {
                    query = query.Where(p => p.ID_Categoría == request.CategoriaId.Value);
                }

                if (!string.IsNullOrEmpty(request.Estado))
                {
                    query = query.Where(p => p.estado == request.Estado);
                }

                if (request.StockBajo.HasValue && request.StockBajo.Value)
                {
                    query = query.Where(p => p.Lotes.Sum(l => l.cantidad) < 10);
                }

                // Busca término en varios campos
                if (!string.IsNullOrWhiteSpace(request.SearchValue))
                {
                    var searchValue = request.SearchValue.ToLower();
                    query = query.Where(p =>
                        p.Nombre.ToLower().Contains(searchValue) ||
                        p.Marca.ToLower().Contains(searchValue) ||
                        p.Categorias.Nombre.ToLower().Contains(searchValue)
                    );
                }

                // Total sin filtrar
                response.recordsTotal = db.Productos.Count();

                // Total filtrado
                response.recordsFiltered = query.Count();

                // Aplica ordenación
                if (!string.IsNullOrEmpty(request.SortColumn))
                {
                    switch (request.SortColumn.ToLower())
                    {
                        case "nombre":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(p => p.Nombre)
                                : query.OrderByDescending(p => p.Nombre);
                            break;
                        case "categoria":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(p => p.Categorias.Nombre)
                                : query.OrderByDescending(p => p.Categorias.Nombre);
                            break;
                        case "precio_compra":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(p => p.precio_compra)
                                : query.OrderByDescending(p => p.precio_compra);
                            break;
                        case "precio_venta":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(p => p.precio_venta)
                                : query.OrderByDescending(p => p.precio_venta);
                            break;
                        case "estado":
                            query = request.SortDirection == "asc"
                                ? query.OrderBy(p => p.estado)
                                : query.OrderByDescending(p => p.estado);
                            break;
                        default:
                            query = query.OrderBy(p => p.Nombre);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(p => p.Nombre);
                }

                // Aplica paginación
                var productosData = query
                    .Skip(request.Start)
                    .Take(request.Length)
                    .ToList();

                // Transforma a ProductoTableViewModel
                response.data = productosData.Select(p =>
                {
                    // Calcula el stock total disponible usando cantidad 
                    var stockTotal = p.Lotes != null
                        ? p.Lotes.Sum(l => l.cantidad)
                        : 0;

                    // Obtiene la URL de la imagen principal
                    var imagenPrincipal = p.Imagenes_Producto
                        .FirstOrDefault(i => i.Estado && i.EsPrincipal);

                    // Crea el viewmodel
                    var viewModel = new ProductoTableViewModel
                    {
                        ID_Producto = p.ID_Producto,
                        Nombre = p.Nombre,
                        NombreCategoria = p.Categorias?.Nombre,
                        precio_compra = p.precio_compra,
                        precio_venta = p.precio_venta,
                        StockTotal = stockTotal,
                        estado = p.estado,
                        UrlImagenPrincipal = imagenPrincipal?.URL,
                        CantidadImagenes = p.Imagenes_Producto != null ? p.Imagenes_Producto.Count(i => i.Estado) : 0
                    };

                    return viewModel;
                }).ToList();

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: Productos/ToggleEstado
        [HttpPost]
        public ActionResult ToggleEstado(int id)
        {
            try
            {
                var producto = db.Productos
                    .FirstOrDefault(p => p.ID_Producto == id);

                if (producto == null)
                {
                    return Json(new { success = false, message = "Producto no encontrado" });
                }

                // Cambia el estado (Activo/Inactivo)
                string oldStatus = producto.estado;
                producto.estado = producto.estado == "Activo" ? "Inactivo" : "Activo";
                string newStatus = producto.estado;

                db.Entry(producto).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    estado = producto.estado,
                    message = $"El producto {producto.Nombre} ha cambiado de estado {oldStatus} a {newStatus}",
                    productName = producto.Nombre
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cambiar estado: {ex.Message}");
                return Json(new { success = false, message = "Error al cambiar el estado: " + ex.Message });
            }
        }

        // GET: Productos/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Productos producto = db.Productos
                .Include(p => p.Categorias)
                .Include(p => p.Lotes)
                .Include(p => p.Imagenes_Producto)
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }
            // Continuación del método Details en ProductosController
            var viewModel = ProductoViewModel.FromEntity(producto);

            // Obtiene todas las imágenes del producto
            viewModel.Imagenes = producto.Imagenes_Producto
                .Where(i => i.Estado)
                .Select(i => ImagenProductoViewModel.FromEntity(i))
                .ToList();

            return View(viewModel);
        }

        // GET: Productos/Create
        public ActionResult Create()
        {
            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre");

            // Inicializa un nuevo producto con propiedades vacías para TinyMCE
            var viewModel = new ProductoViewModel
            {
                Indicaciones = "",
                Detalles = "",
                Ingredientes = ""
            };

            return View(viewModel);
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Permite HTML para TinyMCE
        public async Task<ActionResult> Create(ProductoViewModel viewModel, HttpPostedFileBase[] imagenes, string[] imagenesUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Convierte ViewModel a Entidad
                    var producto = viewModel.ToEntity();

                    // Agrega el producto a la base de datos
                    db.Productos.Add(producto);
                    await db.SaveChangesAsync();

                    // Procesa las imágenes si se han subido
                    if (imagenes != null && imagenes.Length > 0)
                    {
                        await ProcesarImagenesProducto(producto.ID_Producto, imagenes, true);
                    }

                    // Procesa las URLs de imágenes si se han proporcionado
                    if (imagenesUrl != null && imagenesUrl.Length > 0)
                    {
                        await ProcesarUrlsImagenesProducto(producto.ID_Producto, imagenesUrl, imagenes == null || imagenes.Length == 0);
                    }

                    TempData["Message"] = "Producto creado correctamente";
                    TempData["MessageType"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el producto: " + ex.Message);
                }
            }

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", viewModel.ID_Categoría);
            return View(viewModel);
        }


        // GET: Productos/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Productos producto = db.Productos
                .Include(p => p.Imagenes_Producto)
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            var viewModel = ProductoViewModel.FromEntity(producto);

            // Carga las imágenes del producto
            viewModel.Imagenes = producto.Imagenes_Producto
                .Where(i => i.Estado)
                .Select(i => ImagenProductoViewModel.FromEntity(i))
                .ToList();

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", producto.ID_Categoría);

            return View(viewModel);
        }

        // POST: Productos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)] // Permite HTML para TinyMCE
        public async Task<ActionResult> Edit(ProductoViewModel viewModel, HttpPostedFileBase[] imagenes, string[] imagenesUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Obtiene el producto existente para mantener las referencias
                    var productoExistente = db.Productos.Find(viewModel.ID_Producto);

                    if (productoExistente == null)
                    {
                        return HttpNotFound();
                    }

                    // Actualiza propiedades
                    productoExistente.ID_Categoría = viewModel.ID_Categoría;
                    productoExistente.Nombre = viewModel.Nombre;
                    productoExistente.precio_compra = viewModel.precio_compra;
                    productoExistente.precio_venta = viewModel.precio_venta;
                    productoExistente.estado = viewModel.estado;
                    productoExistente.Indicaciones = viewModel.Indicaciones;
                    productoExistente.Detalles = viewModel.Detalles;
                    productoExistente.Ingredientes = viewModel.Ingredientes;
                    productoExistente.UnidadDeVenta = viewModel.UnidadDeVenta;
                    productoExistente.ContenidoNeto = viewModel.ContenidoNeto;
                    productoExistente.Marca = viewModel.Marca;

                    // Actualiza en la base de datos
                    db.Entry(productoExistente).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    // Procesa imágenes nuevas si existen
                    if (imagenes != null && imagenes.Length > 0)
                    {
                        // El segundo parámetro "false" indica que no queremos hacer estas imágenes principales automáticamente
                        await ProcesarImagenesProducto(viewModel.ID_Producto, imagenes, false);
                    }

                    // Procesa las URLs de imágenes si se han proporcionado
                    if (imagenesUrl != null && imagenesUrl.Length > 0)
                    {
                        await ProcesarUrlsImagenesProducto(viewModel.ID_Producto, imagenesUrl, false);
                    }

                    TempData["Message"] = "Producto actualizado correctamente";
                    TempData["MessageType"] = "success";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el producto: " + ex.Message);
                }
            }

            // Si hay errores, recarga las imágenes existentes
            var producto = db.Productos
                .Include(p => p.Imagenes_Producto)
                .FirstOrDefault(p => p.ID_Producto == viewModel.ID_Producto);

            if (producto != null)
            {
                viewModel.Imagenes = producto.Imagenes_Producto
                    .Where(i => i.Estado)
                    .Select(i => ImagenProductoViewModel.FromEntity(i))
                    .ToList();
            }

            ViewBag.ID_Categoría = new SelectList(db.Categorias, "ID_Categoría", "Nombre", viewModel.ID_Categoría);
            return View(viewModel);
        }

        // Método auxiliar para procesar URLs de imágenes
        private async Task<bool> ProcesarUrlsImagenesProducto(int productoId, string[] urls, bool hacerPrincipalSiEsPrimera)
        {
            bool primeraImagen = !db.Imagenes_Producto.Any(i => i.ID_Producto == productoId && i.Estado);
            bool resultado = false;

            try
            {
                foreach (var url in urls.Where(u => !string.IsNullOrEmpty(u)))
                {
                    // Crea la entidad de imagen
                    var nuevaImagen = new Imagenes_Producto
                    {
                        ID_Producto = productoId,
                        URL = url,
                        Descripcion = "Imagen por URL",
                        EsPrincipal = (primeraImagen && hacerPrincipalSiEsPrimera) ||
                                      (!primeraImagen && url == urls.First() && hacerPrincipalSiEsPrimera),
                        FechaCreacion = DateTime.Now,
                        Estado = true
                    };

                    // Si esta es principal, actualiza otras imágenes
                    if (nuevaImagen.EsPrincipal)
                    {
                        var imagenesExistentes = db.Imagenes_Producto
                            .Where(i => i.ID_Producto == productoId && i.EsPrincipal)
                            .ToList();

                        foreach (var img in imagenesExistentes)
                        {
                            img.EsPrincipal = false;
                            db.Entry(img).State = EntityState.Modified;
                        }
                    }

                    db.Imagenes_Producto.Add(nuevaImagen);
                    await db.SaveChangesAsync();

                    primeraImagen = false;
                    resultado = true;
                }

                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al procesar URLs de imágenes: {ex.Message}");
                return false;
            }
        }

        // GET: Productos/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Productos producto = db.Productos
                .Include(p => p.Categorias)
                .Include(p => p.Lotes)
                .Include(p => p.Imagenes_Producto)
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            // Verifica si tiene lotes asociados
            if (producto.Lotes.Any())
            {
                ViewBag.TieneLotes = true;
                ViewBag.TotalLotes = producto.Lotes.Count;
            }
            else
            {
                ViewBag.TieneLotes = false;
            }

            var viewModel = ProductoViewModel.FromEntity(producto);

            // Carga las imágenes del producto
            viewModel.Imagenes = producto.Imagenes_Producto
                .Where(i => i.Estado)
                .Select(i => ImagenProductoViewModel.FromEntity(i))
                .ToList();

            return View(viewModel);
        }

        // POST: Productos/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Productos producto = db.Productos
                .Include(p => p.Lotes)
                .Include(p => p.Imagenes_Producto)
                .FirstOrDefault(p => p.ID_Producto == id);

            // En lugar de eliminar físicamente, cambia el estado
            producto.estado = "Inactivo";

            // También marca como inactivas todas las imágenes
            foreach (var imagen in producto.Imagenes_Producto)
            {
                imagen.Estado = false;
                db.Entry(imagen).State = EntityState.Modified;
            }

            db.Entry(producto).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["Message"] = "Producto eliminado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Productos/Inventario
        public ActionResult Inventario(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var producto = db.Productos
                .Include(p => p.Categorias)
                .Include(p => p.Lotes)
                .Include(p => p.Imagenes_Producto)
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            var viewModel = ProductoViewModel.FromEntity(producto);

            // Obtiene los lotes del producto
            var lotes = db.Lotes
                .Where(l => l.ID_Producto == id)
                .ToList()
                .Select(l => LoteViewModel.FromEntity(l))
                .ToList();

            ViewBag.Lotes = lotes;

            return View(viewModel);
        }

        // GET: Productos/GestionarImagenes
        public ActionResult GestionarImagenes(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var producto = db.Productos
                .Include(p => p.Imagenes_Producto)
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductoNombre = producto.Nombre;
            ViewBag.ProductoID = producto.ID_Producto;

            var imagenes = producto.Imagenes_Producto
                .Where(i => i.Estado)
                .Select(i => ImagenProductoViewModel.FromEntity(i))
                .ToList();

            return View(imagenes);
        }

        // POST: Productos/SubirImagen
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubirImagen(int productoId, HttpPostedFileBase[] imagenes, bool esPrincipal = false)
        {
            if (productoId <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Verifica que el producto existe
            var producto = db.Productos.Find(productoId);
            if (producto == null)
            {
                return HttpNotFound();
            }

            if (imagenes != null && imagenes.Length > 0)
            {
                await ProcesarImagenesProducto(productoId, imagenes, esPrincipal);

                TempData["Message"] = "Imágenes subidas correctamente";
                TempData["MessageType"] = "success";
            }
            else
            {
                TempData["Message"] = "No se seleccionaron imágenes para subir";
                TempData["MessageType"] = "warning";
            }

            return RedirectToAction("GestionarImagenes", new { id = productoId });
        }

        // POST: Productos/AgregarImagenUrl
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AgregarImagenUrl(int productoId, string imageUrl, string descripcion, bool esPrincipal = false)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                TempData["Message"] = "Debe proporcionar una URL válida";
                TempData["MessageType"] = "error";
                return RedirectToAction("GestionarImagenes", new { id = productoId });
            }

            try
            {
                // Verifica que el producto existe
                var producto = db.Productos.Find(productoId);
                if (producto == null)
                {
                    return HttpNotFound();
                }

                // Si se marca como principal, asegurarse de que no haya otra principal
                if (esPrincipal)
                {
                    var imagenesActuales = db.Imagenes_Producto
                        .Where(i => i.ID_Producto == productoId && i.EsPrincipal)
                        .ToList();

                    foreach (var img in imagenesActuales)
                    {
                        img.EsPrincipal = false;
                        db.Entry(img).State = EntityState.Modified;
                    }
                }

                // Crea la nueva entidad de imagen
                var nuevaImagen = new Imagenes_Producto
                {
                    ID_Producto = productoId,
                    URL = imageUrl,
                    Descripcion = descripcion ?? "Imagen sin descripción",
                    EsPrincipal = esPrincipal,
                    FechaCreacion = DateTime.Now,
                    Estado = true
                };

                db.Imagenes_Producto.Add(nuevaImagen);
                await db.SaveChangesAsync();

                TempData["Message"] = "Imagen agregada correctamente por URL";
                TempData["MessageType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error al agregar la imagen: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("GestionarImagenes", new { id = productoId });
        }

        // Método auxiliar para procesar imágenes
        private async Task<bool> ProcesarImagenesProducto(int productoId, HttpPostedFileBase[] archivosImagen, bool hacerPrincipalSiEsPrimera)
        {
            bool primeraImagen = !db.Imagenes_Producto.Any(i => i.ID_Producto == productoId && i.Estado);
            bool resultado = false;

            try
            {
                foreach (var archivo in archivosImagen.Where(a => a != null && a.ContentLength > 0))
                {
                    // Aquí se implementaría la lógica real para subir a la nube

                    string fileName = Path.GetFileName(archivo.FileName);
                    string extension = Path.GetExtension(fileName);
                    string uniqueFileName = $"{Guid.NewGuid()}{extension}";

                    // Simula la URL que vendría de tu servicio en la nube
                    string cloudUrl = $"https://tu-storage-en-la-nube.com/{uniqueFileName}";

                    // Crea la entidad de imagen
                    var nuevaImagen = new Imagenes_Producto
                    {
                        ID_Producto = productoId,
                        URL = cloudUrl,
                        Descripcion = fileName,
                        EsPrincipal = (primeraImagen && hacerPrincipalSiEsPrimera) ||
                                      (!primeraImagen && archivo == archivosImagen.First() && hacerPrincipalSiEsPrimera),
                        FechaCreacion = DateTime.Now,
                        Estado = true
                    };

                    // Si esta es principal, actualizar otras imágenes
                    if (nuevaImagen.EsPrincipal)
                    {
                        var imagenesExistentes = db.Imagenes_Producto
                            .Where(i => i.ID_Producto == productoId && i.EsPrincipal)
                            .ToList();

                        foreach (var img in imagenesExistentes)
                        {
                            img.EsPrincipal = false;
                            db.Entry(img).State = EntityState.Modified;
                        }
                    }

                    db.Imagenes_Producto.Add(nuevaImagen);
                    await db.SaveChangesAsync();

                    primeraImagen = false;
                    resultado = true;
                }

                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al procesar imágenes: {ex.Message}");
                return false;
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