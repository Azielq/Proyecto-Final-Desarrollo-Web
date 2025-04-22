using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class ImagenProductoViewModel
    {
        public int ID_Imagen { get; set; }

        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ID_Producto { get; set; }

        [Required(ErrorMessage = "La URL de la imagen es obligatoria")]
        [Display(Name = "URL de la imagen")]
        public string URL { get; set; }

        [StringLength(100, ErrorMessage = "La descripción no puede exceder los 100 caracteres")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Es imagen principal")]
        public bool EsPrincipal { get; set; }

        [Display(Name = "Fecha de creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public System.DateTime FechaCreacion { get; set; }

        [Display(Name = "Estado")]
        public bool Estado { get; set; }

        // Propiedad para el archivo de imagen (no se almacena en la BD)
        [Display(Name = "Imagen")]
        public HttpPostedFileBase ImagenFile { get; set; }

        public string NombreProducto { get; set; }

        // Método para convertir la entidad a ViewModel
        public static ImagenProductoViewModel FromEntity(Imagenes_Producto imagen)
        {
            return new ImagenProductoViewModel
            {
                ID_Imagen = imagen.ID_Imagen,
                ID_Producto = imagen.ID_Producto,
                URL = imagen.URL,
                Descripcion = imagen.Descripcion,
                EsPrincipal = imagen.EsPrincipal,
                FechaCreacion = imagen.FechaCreacion,
                Estado = imagen.Estado,
                NombreProducto = imagen.Productos?.Nombre
            };
        }

        // Método para convertir ViewModel a entidad
        public Imagenes_Producto ToEntity()
        {
            return new Imagenes_Producto
            {
                ID_Imagen = this.ID_Imagen,
                ID_Producto = this.ID_Producto,
                URL = this.URL,
                Descripcion = this.Descripcion,
                EsPrincipal = this.EsPrincipal,
                FechaCreacion = this.ID_Imagen == 0 ? DateTime.Now : this.FechaCreacion,
                Estado = this.Estado
            };
        }
    }
}