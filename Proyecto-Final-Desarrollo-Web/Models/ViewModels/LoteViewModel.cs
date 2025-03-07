using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class LoteViewModel
    {
        public int id_Lote { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un medicamento")]
        [Display(Name = "Medicamento")]
        public int ID_Medicamento { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 999999, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int cantidad { get; set; }

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Fecha de Vencimiento")]
        public DateTime fecha_vencimiento { get; set; }

        // Estas son propiedades útiles adicionales para la vista
        [Display(Name = "Medicamento")]
        public string NombreMedicamento { get; set; }

        [Display(Name = "Laboratorio")]
        public string NombreLaboratorio { get; set; }

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

        // El metódillo para convertir la entidad a ViewModel
        public static LoteViewModel FromEntity(Lotes lote)
        {
            var viewModel = new LoteViewModel
            {
                id_Lote = lote.id_Lote,
                ID_Medicamento = lote.ID_Medicamento,
                cantidad = lote.cantidad,
                fecha_vencimiento = lote.fecha_vencimiento,
                NombreMedicamento = lote.Medicamentos?.Nombre,
                NombreLaboratorio = lote.Medicamentos?.Laboratorios?.Nombre
            };

            // Con esto si hay inventario relacionado, se puede obtener la ubicación
            if (lote.Inventario != null && lote.Inventario.Count > 0)
            {
                viewModel.Ubicacion = lote.Inventario.FirstOrDefault()?.ubicacion;
            }

            return viewModel;
        }

        // Convierte al revés que el otro método
        public Lotes ToEntity()
        {
            return new Lotes
            {
                id_Lote = this.id_Lote,
                ID_Medicamento = this.ID_Medicamento,
                cantidad = this.cantidad,
                fecha_vencimiento = this.fecha_vencimiento
            };
        }
    }
}