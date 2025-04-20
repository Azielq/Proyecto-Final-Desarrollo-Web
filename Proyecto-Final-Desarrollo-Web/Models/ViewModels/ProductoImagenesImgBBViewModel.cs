using Proyecto_Final_Desarrollo_Web.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.Models.ViewModels
{
    public class ProductoImagenesImgBBViewModel
    {
        public int ID_Producto { get; set; }

        [Display(Name = "Producto")]
        public string NombreProducto { get; set; }

        // Lista de imágenes del producto
        public List<ImagenProductoViewModel> Imagenes { get; set; }

        // Para la subida de archivos
        [Display(Name = "Subir imagen")]
        public HttpPostedFileBase ImageFile { get; set; }

        // Para agregar imágenes por URL
        [Display(Name = "URL de la imagen")]
        public string ImageUrl { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Es imagen principal")]
        public bool EsPrincipal { get; set; }

        public ProductoImagenesImgBBViewModel()
        {
            Imagenes = new List<ImagenProductoViewModel>();
        }
    }

    // ViewModel para la respuesta de ImgBB
    public class ImgBBResponseViewModel
    {
        public bool Success { get; set; }
        public ImgBBDataViewModel Data { get; set; }
        public ImgBBErrorViewModel Error { get; set; }
    }

    public class ImgBBDataViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Display_url { get; set; }
        public string Thumb { get; set; }
        public string Delete_url { get; set; }
    }

    public class ImgBBErrorViewModel
    {
        public string Message { get; set; }
        public int Code { get; set; }
    }
}