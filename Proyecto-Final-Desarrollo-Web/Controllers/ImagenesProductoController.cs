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
using Proyecto_Final_Desarrollo_Web.Helpers;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class ImagenesProductoController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: ImagenesProducto
        public ActionResult Index(int? productoId)
        {
            if (productoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var producto = db.Productos.Find(productoId);
            if (producto == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductoNombre = producto.Nombre;
            ViewBag.ProductoId = producto.ID_Producto;

            var imagenes = db.Imagenes_Producto
                .Where(i => i.ID_Producto == productoId)
                .ToList()
                .Select(i => ImagenProductoViewModel.FromEntity(i))
                .ToList();

            return View(imagenes);
        }

        // GET: ImagenesProducto/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Busca la imagen en la base de datos
            Imagenes_Producto imagen = db.Imagenes_Producto.Find(id);
            if (imagen == null)
            {
                return HttpNotFound();
            }

            // Convierte la entidad a ViewModel
            var viewModel = ImagenProductoViewModel.FromEntity(imagen);

            // Pasa el nombre del producto al ViewBag para la navegación
            var producto = db.Productos.Find(imagen.ID_Producto);
            if (producto != null)
            {
                viewModel.NombreProducto = producto.Nombre;
            }

            return View(viewModel);
        }


        // GET: ImagenesProducto/Create
        public ActionResult Create(int? productoId)
        {
            if (productoId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var producto = db.Productos.Find(productoId);
            if (producto == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ImagenProductoViewModel
            {
                ID_Producto = producto.ID_Producto,
                NombreProducto = producto.Nombre,
                Estado = true,
                FechaCreacion = DateTime.Now
            };

            return View(viewModel);
        }

        // POST: ImagenesProducto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ImagenProductoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Caso 1: Se está subiendo un archivo
                    if (viewModel.ImagenFile != null && viewModel.ImagenFile.ContentLength > 0)
                    {
                        // Genera nombre único para el archivo
                        string fileName = Path.GetFileName(viewModel.ImagenFile.FileName);
                        string extension = Path.GetExtension(fileName);
                        string uniqueFileName = $"{Guid.NewGuid()}{extension}";

                        // En un escenario real, aquí subiría el archivo a mi proveedor en la nube
                        // y obtendría la URL resultante. Por ahora, simulamos este proceso.
                        string cloudUrl = $"https://tu-storage-en-la-nube.com/{uniqueFileName}";

                        // Asigna la URL obtenida
                        viewModel.URL = cloudUrl;
                    }
                    // Caso 2: Se está proporcionando directamente una URL
                    else if (string.IsNullOrEmpty(viewModel.URL))
                    {
                        ModelState.AddModelError("", "Debe proporcionar una imagen o una URL válida");
                        return View(viewModel);
                    }

                    // Si es la primera imagen del producto, marcarla como principal
                    if (!db.Imagenes_Producto.Any(i => i.ID_Producto == viewModel.ID_Producto))
                    {
                        viewModel.EsPrincipal = true;
                    }

                    // Si se marca como principal, asegurarse de que no haya otra principal
                    if (viewModel.EsPrincipal)
                    {
                        var imagenesActuales = db.Imagenes_Producto
                            .Where(i => i.ID_Producto == viewModel.ID_Producto && i.EsPrincipal)
                            .ToList();

                        foreach (var img in imagenesActuales)
                        {
                            img.EsPrincipal = false;
                            db.Entry(img).State = EntityState.Modified;
                        }
                    }

                    // Convierte a entidad y guardar
                    var entidad = viewModel.ToEntity();
                    db.Imagenes_Producto.Add(entidad);
                    await db.SaveChangesAsync();

                    TempData["Message"] = "Imagen agregada correctamente al producto";
                    TempData["MessageType"] = "success";

                    return RedirectToAction("Index", new { productoId = viewModel.ID_Producto });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar la imagen: " + ex.Message);
                }
            }

            // Si llegamos aquí, algo salió mal, mostramos nuevamente el formulario
            viewModel.NombreProducto = db.Productos.Find(viewModel.ID_Producto)?.Nombre;
            return View(viewModel);
        }

        // GET: ImagenesProducto/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Imagenes_Producto imagen = db.Imagenes_Producto.Find(id);
            if (imagen == null)
            {
                return HttpNotFound();
            }

            var viewModel = ImagenProductoViewModel.FromEntity(imagen);
            return View(viewModel);
        }

        // POST: ImagenesProducto/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ImagenProductoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Si se marca como principal, asegurarse de que no haya otra principal
                    if (viewModel.EsPrincipal)
                    {
                        var imagenesActuales = db.Imagenes_Producto
                            .Where(i => i.ID_Producto == viewModel.ID_Producto && i.ID_Imagen != viewModel.ID_Imagen && i.EsPrincipal)
                            .ToList();

                        foreach (var img in imagenesActuales)
                        {
                            img.EsPrincipal = false;
                            db.Entry(img).State = EntityState.Modified;
                        }
                    }

                    // Si se está subiendo un nuevo archivo
                    if (viewModel.ImagenFile != null && viewModel.ImagenFile.ContentLength > 0)
                    {
                        // Aquí implementaría la lógica para subir la imagen a la nube
                        // y obtiene su URL. Por ahora, simularemos este proceso.

                        string fileName = Path.GetFileName(viewModel.ImagenFile.FileName);
                        string extension = Path.GetExtension(fileName);
                        string uniqueFileName = $"{Guid.NewGuid()}{extension}";

                        // Simula la URL que vendría de tu servicio en la nube
                        string cloudUrl = $"https://tu-storage-en-la-nube.com/{uniqueFileName}";

                        // Asigna la nueva URL
                        viewModel.URL = cloudUrl;
                    }

                    // Actualiza la entidad
                    var entidad = viewModel.ToEntity();
                    db.Entry(entidad).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    TempData["Message"] = "Imagen actualizada correctamente";
                    TempData["MessageType"] = "success";

                    return RedirectToAction("Index", new { productoId = viewModel.ID_Producto });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar la imagen: " + ex.Message);
                }
            }

            // Si llegamos aquí, algo salió mal, mostramos nuevamente el formulario
            viewModel.NombreProducto = db.Productos.Find(viewModel.ID_Producto)?.Nombre;
            return View(viewModel);
        }

        // GET: ImagenesProducto/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Imagenes_Producto imagen = db.Imagenes_Producto.Find(id);
            if (imagen == null)
            {
                return HttpNotFound();
            }

            var viewModel = ImagenProductoViewModel.FromEntity(imagen);
            return View(viewModel);
        }

        // POST: ImagenesProducto/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Imagenes_Producto imagen = db.Imagenes_Producto.Find(id);
            int productoId = imagen.ID_Producto;
            bool esPrincipal = imagen.EsPrincipal;

            db.Imagenes_Producto.Remove(imagen);
            await db.SaveChangesAsync();

            // Si era la imagen principal, asigna otra como principal
            if (esPrincipal)
            {
                var otraImagen = db.Imagenes_Producto
                    .Where(i => i.ID_Producto == productoId)
                    .FirstOrDefault();

                if (otraImagen != null)
                {
                    otraImagen.EsPrincipal = true;
                    db.Entry(otraImagen).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }

            TempData["Message"] = "Imagen eliminada correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", new { productoId = productoId });
        }

        // POST: ImagenesProducto/SetMainImage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetMainImage(int id)
        {
            Imagenes_Producto imagen = db.Imagenes_Producto.Find(id);
            if (imagen == null)
            {
                return HttpNotFound();
            }

            // Quita el estado principal de todas las imágenes del producto
            var imagenesProducto = db.Imagenes_Producto
                .Where(i => i.ID_Producto == imagen.ID_Producto)
                .ToList();

            foreach (var img in imagenesProducto)
            {
                img.EsPrincipal = false;
                db.Entry(img).State = EntityState.Modified;
            }

            // Establece esta imagen como principal
            imagen.EsPrincipal = true;
            db.Entry(imagen).State = EntityState.Modified;
            await db.SaveChangesAsync();

            TempData["Message"] = "Imagen principal actualizada correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index", new { productoId = imagen.ID_Producto });
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