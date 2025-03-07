using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class CompraViewModel
    {
        // Datos de la compra
        public int id_compra { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un proveedor")]
        [Display(Name = "Proveedor")]
        public int ID_proveedor { get; set; }

        [Display(Name = "Fecha")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime? fecha { get; set; }

        [Display(Name = "Estado")]
        public string estado { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal total { get; set; }

        // Esto es para la información adicional del proveedor
        [Display(Name = "Nombre del Proveedor")]
        public string NombreProveedor { get; set; }

        [Display(Name = "Teléfono")]
        public string TelefonoProveedor { get; set; }

        [Display(Name = "Correo")]
        public string CorreoProveedor { get; set; }

        // Esta es para la lista de detalles de la compra
        public List<DetalleCompraViewModel> Detalles { get; set; }

        public CompraViewModel()
        {
            Detalles = new List<DetalleCompraViewModel>();
            fecha = DateTime.Now;
            estado = "Pendiente";
        }

        // Método para convertir la entidad a ViewModel
        public static CompraViewModel FromEntity(Compras_Farmacia compra)
        {
            var viewModel = new CompraViewModel
            {
                id_compra = compra.id_compra,
                ID_proveedor = compra.ID_proveedor,
                fecha = compra.fecha,
                estado = compra.estado,
                total = compra.total
            };

            if (compra.Proveedores != null)
            {
                viewModel.NombreProveedor = compra.Proveedores.Nombre;
                viewModel.TelefonoProveedor = compra.Proveedores.Telefono;
                viewModel.CorreoProveedor = compra.Proveedores.Correo;
            }

            if (compra.Detalles_Compras_Farmacia != null)
            {
                viewModel.Detalles = compra.Detalles_Compras_Farmacia
                    .Select(d => DetalleCompraViewModel.FromEntity(d))
                    .ToList();
            }

            return viewModel;
        }

        // Este método sirve para convertir el ViewModel a entidad
        public Compras_Farmacia ToEntity()
        {
            var compra = new Compras_Farmacia
            {
                id_compra = this.id_compra,
                ID_proveedor = this.ID_proveedor,
                fecha = this.fecha ?? DateTime.Now,
                estado = this.estado ?? "Pendiente",
                total = this.total
            };

            return compra;
        }

        // Con esto se puede calcular el total a partir de los detalles
        public void CalcularTotal()
        {
            this.total = Detalles?.Sum(d => d.Subtotal) ?? 0;
        }
    }

    public class DetalleCompraViewModel
    {
        public int id_detalle_compra { get; set; }
        public int ID_Compra { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un medicamento")]
        [Display(Name = "Medicamento")]
        public int ID_Medicamento { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, 999999, ErrorMessage = "La cantidad debe ser mayor a 0")]
        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Subtotal")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Subtotal { get; set; }

        // Propiedades adicionales para la vista
        [Display(Name = "Nombre del Medicamento")]
        public string NombreMedicamento { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Laboratorio")]
        public string Laboratorio { get; set; }

        [Display(Name = "Precio Unitario")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PrecioUnitario
        {
            get { return Cantidad > 0 ? Math.Round(Subtotal / Cantidad, 2) : 0; }
            set { /* Este setter está solo para cumplir con la propiedad  */ }
        }

        // Este método es de convertir la entidad a ViewModel
        public static DetalleCompraViewModel FromEntity(Detalles_Compras_Farmacia detalle)
        {
            var viewModel = new DetalleCompraViewModel
            {
                id_detalle_compra = detalle.id_detalle_compra,
                ID_Compra = detalle.ID_Compra,
                ID_Medicamento = detalle.ID_Medicamento,
                Cantidad = detalle.Cantidad,
                Subtotal = detalle.Subtotal
            };

            if (detalle.Medicamentos != null)
            {
                viewModel.NombreMedicamento = detalle.Medicamentos.Nombre;
                viewModel.Categoria = detalle.Medicamentos.Categorias?.Nombre;
                viewModel.Laboratorio = detalle.Medicamentos.Laboratorios?.Nombre;
            }

            return viewModel;
        }

        // Convierte al revés que el anterior, ViewModel a entidad
        public Detalles_Compras_Farmacia ToEntity()
        {
            return new Detalles_Compras_Farmacia
            {
                id_detalle_compra = this.id_detalle_compra,
                ID_Compra = this.ID_Compra,
                ID_Medicamento = this.ID_Medicamento,
                Cantidad = this.Cantidad,
                Subtotal = this.Subtotal
            };
        }
    }
}