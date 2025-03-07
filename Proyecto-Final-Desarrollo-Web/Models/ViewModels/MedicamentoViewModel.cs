using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class MedicamentoViewModel
    {
        public int ID_Medicamento { get; set; }

        [Required(ErrorMessage = "El campo Categoría es obligatorio")]
        [Display(Name = "Categoría")]
        public int ID_Categoría { get; set; }

        [Required(ErrorMessage = "El campo Laboratorio es obligatorio")]
        [Display(Name = "Laboratorio")]
        public int ID_Laboratorio { get; set; }

        [Required(ErrorMessage = "El nombre del medicamento es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El principio activo es obligatorio")]
        [StringLength(100, ErrorMessage = "El principio activo no puede exceder los 100 caracteres")]
        [Display(Name = "Principio Activo")]
        public string principio_activo { get; set; }

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

        // Estas son adicionales para la vista
        [Display(Name = "Categoría")]
        public string NombreCategoria { get; set; }

        [Display(Name = "Laboratorio")]
        public string NombreLaboratorio { get; set; }

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

        // El metódillo para convertir la entidad a ViewModel
        public static MedicamentoViewModel FromEntity(Medicamentos medicamento)
        {
            return new MedicamentoViewModel
            {
                ID_Medicamento = medicamento.ID_Medicamento,
                ID_Categoría = medicamento.ID_Categoría,
                ID_Laboratorio = medicamento.ID_Laboratorio,
                Nombre = medicamento.Nombre,
                principio_activo = medicamento.principio_activo,
                precio_compra = medicamento.precio_compra,
                precio_venta = medicamento.precio_venta,
                estado = medicamento.estado,
                NombreCategoria = medicamento.Categorias?.Nombre,
                NombreLaboratorio = medicamento.Laboratorios?.Nombre,
                StockDisponible = medicamento.Lotes != null ? medicamento.Lotes.Sum(l => l.cantidad) : 0
            };
        }

        // Al revés en ViewModel a entidad
        public Medicamentos ToEntity()
        {
            return new Medicamentos
            {
                ID_Medicamento = this.ID_Medicamento,
                ID_Categoría = this.ID_Categoría,
                ID_Laboratorio = this.ID_Laboratorio,
                Nombre = this.Nombre,
                principio_activo = this.principio_activo,
                precio_compra = this.precio_compra,
                precio_venta = this.precio_venta,
                estado = this.estado ?? "Activo"
            };
        }
    }
}