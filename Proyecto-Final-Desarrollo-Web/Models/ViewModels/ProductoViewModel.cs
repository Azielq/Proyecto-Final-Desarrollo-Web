using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class ProductoViewModel
    {
        public int ID_Producto { get; set; }

        [Required(ErrorMessage = "El campo Categoría es obligatorio")]
        [Display(Name = "Categoría")]
        public int ID_Categoría { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El precio de compra es obligatorio")]
        [Range(0.01, 9999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio de Compra")]
        public decimal precio_compra { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [Range(0.01, 9999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
        [Display(Name = "Precio de Venta")]
        public decimal precio_venta { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        [AllowHtml]
        [Display(Name = "Indicaciones")]
        public string Indicaciones { get; set; }

        [AllowHtml]
        [Display(Name = "Detalles")]
        public string Detalles { get; set; }

        [AllowHtml]
        [Display(Name = "Ingredientes")]
        public string Ingredientes { get; set; }

        [StringLength(50, ErrorMessage = "La unidad de venta no puede exceder los 50 caracteres")]
        [Display(Name = "Unidad de Venta")]
        public string UnidadDeVenta { get; set; }

        [StringLength(50, ErrorMessage = "El contenido neto no puede exceder los 50 caracteres")]
        [Display(Name = "Contenido Neto")]
        public string ContenidoNeto { get; set; }

        [StringLength(100, ErrorMessage = "La marca no puede exceder los 100 caracteres")]
        [Display(Name = "Marca")]
        public string Marca { get; set; }

        // Propiedades adicionales para la vista
        [Display(Name = "Categoría")]
        public string NombreCategoria { get; set; }

        [Display(Name = "Stock Disponible")]
        public int StockDisponible { get; set; }

        [Display(Name = "Margen de Ganancia")]
        public decimal MargenGanancia
        {
            get
            {
                if (precio_compra == 0) return 0;
                return Math.Round(((precio_venta - precio_compra) / precio_compra) * 100, 2);
            }
        }

        // Nuevas propiedades para imágenes
        [Display(Name = "Imagen Principal")]
        public string UrlImagenPrincipal { get; set; }

        [Display(Name = "Imágenes")]
        public List<ImagenProductoViewModel> Imagenes { get; set; }

        // Propiedad para subir nuevas imágenes
        [Display(Name = "Subir Imágenes")]
        public HttpPostedFileBase[] NuevasImagenes { get; set; }

        public ProductoViewModel()
        {
            Imagenes = new List<ImagenProductoViewModel>();
        }

        // Método para convertir la entidad a ViewModel
        public static ProductoViewModel FromEntity(Proyecto_Final_Desarrollo_Web.Models.Productos producto)
        {
            var viewModel = new ProductoViewModel
            {
                ID_Producto = producto.ID_Producto,
                ID_Categoría = producto.ID_Categoría,
                Nombre = producto.Nombre,
                precio_compra = producto.precio_compra,
                precio_venta = producto.precio_venta,
                estado = producto.estado,
                Indicaciones = producto.Indicaciones,
                Detalles = producto.Detalles,
                Ingredientes = producto.Ingredientes,
                UnidadDeVenta = producto.UnidadDeVenta,
                ContenidoNeto = producto.ContenidoNeto,
                Marca = producto.Marca,
                NombreCategoria = producto.Categorias?.Nombre,
                StockDisponible = producto.Lotes != null ? producto.Lotes.Sum(l => l.cantidad) : 0
            };

            // Carga las imágenes si existen
            if (producto.Imagenes_Producto != null && producto.Imagenes_Producto.Any())
            {
                viewModel.Imagenes = producto.Imagenes_Producto
                    .Where(i => i.Estado)
                    .Select(i => ImagenProductoViewModel.FromEntity(i))
                    .ToList();

                // Asigna la URL de la imagen principal
                var imagenPrincipal = producto.Imagenes_Producto
                    .FirstOrDefault(i => i.Estado && i.EsPrincipal);

                viewModel.UrlImagenPrincipal = imagenPrincipal?.URL;
            }

            return viewModel;
        }

        public Proyecto_Final_Desarrollo_Web.Models.Productos ToEntity()
        {
            return new Proyecto_Final_Desarrollo_Web.Models.Productos
            {
                ID_Producto = this.ID_Producto,
                ID_Categoría = this.ID_Categoría,
                Nombre = this.Nombre,
                precio_compra = this.precio_compra,
                precio_venta = this.precio_venta,
                estado = this.estado ?? "Activo",
                Indicaciones = this.Indicaciones,
                Detalles = this.Detalles,
                Ingredientes = this.Ingredientes,
                UnidadDeVenta = this.UnidadDeVenta,
                ContenidoNeto = this.ContenidoNeto,
                Marca = this.Marca
            };
        }
    }
}