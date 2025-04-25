using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.Models.ViewModels;
using Proyecto_Final_Desarrollo_Web.ViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class ImgBBController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // Obtiene la API Key de ImgBB desde Web.config
        private readonly string API_KEY = ConfigurationManager.AppSettings["ImgBB_ApiKey"];
        private const string API_URL = "https://api.imgbb.com/1/upload";

        // GET: ImgBB
        public ActionResult Index()
        {
            return View();
        }

        // GET: ImgBB/ProductImages
        public ActionResult ProductImages(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            var producto = db.Productos
                .Include("Imagenes_Producto")
                .FirstOrDefault(p => p.ID_Producto == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            var viewModel = new ProductoImagenesImgBBViewModel
            {
                ID_Producto = producto.ID_Producto,
                NombreProducto = producto.Nombre,
                Imagenes = producto.Imagenes_Producto
                    .Where(i => i.Estado)
                    .Select(i => new ImagenProductoViewModel
                    {
                        ID_Imagen = i.ID_Imagen,
                        URL = i.URL,
                        Descripcion = i.Descripcion,
                        EsPrincipal = i.EsPrincipal,
                        FechaCreacion = i.FechaCreacion
                    }).ToList()
            };

            return View(viewModel);
        }

        // POST: ImgBB/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload(HttpPostedFileBase imageFile, int? productoId, string descripcion = "", bool esPrincipal = false)
        {
            if (imageFile == null || imageFile.ContentLength == 0)
            {
                TempData["Message"] = "Por favor seleccione una imagen para subir.";
                TempData["MessageType"] = "warning";
                return RedirectToAction("ProductImages", new { id = productoId });
            }

            if (productoId == null)
            {
                TempData["Message"] = "Se requiere un ID de producto válido.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Productos");
            }

            try
            {
                // Valida el tipo de archivo
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                string fileExtension = Path.GetExtension(imageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["Message"] = "Tipo de archivo no permitido. Solo se aceptan imágenes (JPG, PNG, GIF, WEBP).";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("ProductImages", new { id = productoId });
                }

                // Valida tamaño (máximo 10MB para ImgBB)
                if (imageFile.ContentLength > 10 * 1024 * 1024)
                {
                    TempData["Message"] = "La imagen no debe superar los 10MB.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("ProductImages", new { id = productoId });
                }

                // Valida que la API key esté configurada
                if (string.IsNullOrEmpty(API_KEY))
                {
                    TempData["Message"] = "Error de configuración: API Key de ImgBB no configurada en Web.config.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("ProductImages", new { id = productoId });
                }

                // Sube imagen a ImgBB
                string imgbbResponse = await UploadToImgBB(imageFile);

                // Parsea la respuesta
                dynamic responseData = JsonConvert.DeserializeObject(imgbbResponse);

                if (responseData != null && responseData.success == true)
                {
                    // Si se marca como principal, actualiza las otras imágenes
                    if (esPrincipal)
                    {
                        var imagenesActuales = db.Imagenes_Producto
                            .Where(i => i.ID_Producto == productoId && i.EsPrincipal)
                            .ToList();

                        foreach (var img in imagenesActuales)
                        {
                            img.EsPrincipal = false;
                            db.Entry(img).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    // Guarda la información de la imagen en la base de datos
                    var nuevaImagen = new Imagenes_Producto
                    {
                        ID_Producto = productoId.Value,
                        URL = responseData.data.url,
                        Descripcion = !string.IsNullOrEmpty(descripcion) ?
                            descripcion : Path.GetFileNameWithoutExtension(imageFile.FileName),
                        EsPrincipal = esPrincipal,
                        FechaCreacion = DateTime.Now,
                        Estado = true
                    };

                    db.Imagenes_Producto.Add(nuevaImagen);
                    await db.SaveChangesAsync();

                    TempData["Message"] = "Imagen subida correctamente.";
                    TempData["MessageDetail"] = $"La imagen se ha subido a ImgBB y se ha asociado al producto.";
                    TempData["MessageType"] = "success";
                    TempData["UseSweet"] = true;
                }
                else
                {
                    TempData["Message"] = "Error al subir la imagen a ImgBB: " +
                        (responseData?.error?.message ?? "Error desconocido");
                    TempData["MessageType"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error al procesar la imagen: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("ProductImages", new { id = productoId });
        }

        // POST: ImgBB/UploadUrl
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UploadUrl(string imageUrl, int? productoId, string descripcion = "", bool esPrincipal = false)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                TempData["Message"] = "Por favor ingrese una URL de imagen válida.";
                TempData["MessageType"] = "warning";
                return RedirectToAction("ProductImages", new { id = productoId });
            }

            if (productoId == null)
            {
                TempData["Message"] = "Se requiere un ID de producto válido.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Productos");
            }

            try
            {
                // Valida la URL básicamente
                Uri uriResult;
                bool validUrl = Uri.TryCreate(imageUrl, UriKind.Absolute, out uriResult) &&
                               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!validUrl)
                {
                    TempData["Message"] = "La URL de la imagen no es válida.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("ProductImages", new { id = productoId });
                }

                // Si se marca como principal, actualiza las otras imágenes
                if (esPrincipal)
                {
                    var imagenesActuales = db.Imagenes_Producto
                        .Where(i => i.ID_Producto == productoId && i.EsPrincipal)
                        .ToList();

                    foreach (var img in imagenesActuales)
                    {
                        img.EsPrincipal = false;
                        db.Entry(img).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                // Guarda la información de la imagen en la base de datos
                var nuevaImagen = new Imagenes_Producto
                {
                    ID_Producto = productoId.Value,
                    URL = imageUrl,
                    Descripcion = !string.IsNullOrEmpty(descripcion) ?
                        descripcion : "Imagen desde URL",
                    EsPrincipal = esPrincipal,
                    FechaCreacion = DateTime.Now,
                    Estado = true
                };

                db.Imagenes_Producto.Add(nuevaImagen);
                await db.SaveChangesAsync();

                TempData["Message"] = "Imagen desde URL agregada correctamente.";
                TempData["MessageDetail"] = "La URL de la imagen se ha asociado al producto.";
                TempData["MessageType"] = "success";
                TempData["UseSweet"] = true;
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error al procesar la URL de la imagen: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("ProductImages", new { id = productoId });
        }

        // POST: ImgBB/SetMainImage
        [HttpPost]
        public async Task<ActionResult> SetMainImage(int id, int productoId)
        {
            try
            {
                // Desmarca la imagen principal actual
                var imagenesActuales = db.Imagenes_Producto
                    .Where(i => i.ID_Producto == productoId && i.EsPrincipal)
                    .ToList();

                foreach (var img in imagenesActuales)
                {
                    img.EsPrincipal = false;
                    db.Entry(img).State = System.Data.Entity.EntityState.Modified;
                }

                // Marca la nueva imagen principal
                var nuevaImagenPrincipal = db.Imagenes_Producto.Find(id);
                if (nuevaImagenPrincipal == null)
                {
                    return Json(new { success = false, message = "Imagen no encontrada." });
                }

                nuevaImagenPrincipal.EsPrincipal = true;
                db.Entry(nuevaImagenPrincipal).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();

                return Json(new { success = true, message = "Imagen principal actualizada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar: " + ex.Message });
            }
        }

        // POST: ImgBB/DeleteImage
        [HttpPost]
        public async Task<ActionResult> DeleteImage(int id, int productoId)
        {
            try
            {
                var imagen = db.Imagenes_Producto.Find(id);
                if (imagen == null)
                {
                    return Json(new { success = false, message = "Imagen no encontrada." });
                }

                // En lugar de eliminar físicamente, cambiamos el estado
                imagen.Estado = false;
                db.Entry(imagen).State = System.Data.Entity.EntityState.Modified;
                await db.SaveChangesAsync();

                // Si era la imagen principal, seleccionar otra imagen como principal
                if (imagen.EsPrincipal)
                {
                    var otraImagen = db.Imagenes_Producto
                        .Where(i => i.ID_Producto == productoId && i.Estado && i.ID_Imagen != id)
                        .FirstOrDefault();

                    if (otraImagen != null)
                    {
                        otraImagen.EsPrincipal = true;
                        db.Entry(otraImagen).State = System.Data.Entity.EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, message = "Imagen eliminada correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }

        // Método privado para cargar la imagen a ImgBB
        private async Task<string> UploadToImgBB(HttpPostedFileBase imageFile)
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    // Lee el archivo a un array de bytes
                    byte[] fileBytes;
                    using (var binaryReader = new BinaryReader(imageFile.InputStream))
                    {
                        fileBytes = binaryReader.ReadBytes(imageFile.ContentLength);
                    }

                    // Convierte a Base64
                    string base64Image = Convert.ToBase64String(fileBytes);

                    // Agrega la imagen y la API key
                    content.Add(new StringContent(API_KEY), "key");
                    content.Add(new StringContent(base64Image), "image");

                    // Envía la solicitud
                    var response = await client.PostAsync(API_URL, content);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
    }
}