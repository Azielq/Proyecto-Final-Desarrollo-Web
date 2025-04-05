using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class ProductoViewModel
    {
        public int ID_Producto { get; set; }

        [Required(ErrorMessage = "El campo Categoría es obligatorio")]
        [Display(Name = "Categoría")]
        public int ID_Categoría { get; set; }

        // Se eliminó la propiedad ID_Laboratorio

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        // Se eliminó el campo 'principio_activo'

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

        // Propiedades adicionales para la vista

        [Display(Name = "Categoría")]
        public string NombreCategoria { get; set; }

        // Se eliminó 'NombreLaboratorio' ya que la entidad Laboratorios fue removida

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

        // Método para convertir la entidad a ViewModel
        public static ProductoViewModel FromEntity(Proyecto_Final_Desarrollo_Web.Models.Productos producto)
        {
            return new ProductoViewModel
            {
                ID_Producto = producto.ID_Producto,
                ID_Categoría = producto.ID_Categoría,
                Nombre = producto.Nombre,
                precio_compra = producto.precio_compra,
                precio_venta = producto.precio_venta,
                estado = producto.estado,
                NombreCategoria = producto.Categorias?.Nombre,
                StockDisponible = producto.Lotes != null ? producto.Lotes.Sum(l => l.cantidad) : 0
            };
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
                estado = this.estado ?? "Activo"
            };
        }
    }
}
