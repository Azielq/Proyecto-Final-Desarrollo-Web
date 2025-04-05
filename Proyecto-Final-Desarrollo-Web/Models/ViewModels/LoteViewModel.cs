using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class LoteViewModel
    {
        public int id_Lote { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un producto")]
        [Display(Name = "Producto")]
        public int ID_Producto { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 999999, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime fecha_vencimiento { get; set; }

        // Propiedades adicionales para la vista

        [Display(Name = "Producto")]
        public string NombreProducto { get; set; }

        [Display(Name = "Ubicación")]
        public string Ubicacion { get; set; }

        [Display(Name = "Días para Vencer")]
        public int DiasParaVencer
        {
            get
            {
                return (int)(fecha_vencimiento - DateTime.Now).TotalDays;
            }
        }

        [Display(Name = "Estado")]
        public string EstadoLote
        {
            get
            {
                if (DateTime.Now > fecha_vencimiento)
                    return "Vencido";
                else if (DiasParaVencer <= 30)
                    return "Por Vencer";
                else
                    return "Vigente";
            }
        }

        // Método para convertir la entidad a ViewModel
        public static LoteViewModel FromEntity(Lotes lote)
        {
            var viewModel = new LoteViewModel
            {
                id_Lote = lote.id_Lote,
                ID_Producto = lote.ID_Producto, // Se asume que la entidad Lotes se actualizó para usar ID_Producto
                cantidad = lote.cantidad,
                fecha_vencimiento = lote.fecha_vencimiento,
                // Se asume que la propiedad de navegación se renombró de Medicamentos a Productos
                NombreProducto = lote.Productos?.Nombre
            };

            if (lote.Inventario != null && lote.Inventario.Any())
            {
                viewModel.Ubicacion = lote.Inventario.FirstOrDefault()?.ubicacion;
            }

            return viewModel;
        }

        // Método para convertir del ViewModel a la entidad
        public Lotes ToEntity()
        {
            return new Lotes
            {
                id_Lote = this.id_Lote,
                ID_Producto = this.ID_Producto,
                cantidad = this.cantidad,
                fecha_vencimiento = this.fecha_vencimiento
            };
        }
    }
}
