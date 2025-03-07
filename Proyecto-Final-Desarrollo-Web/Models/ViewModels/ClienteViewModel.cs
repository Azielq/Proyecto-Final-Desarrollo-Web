using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class ClienteViewModel
    {
        // Datos del Cliente
        public int id_cliente { get; set; }
        public int ID_Persona { get; set; }

        // Datos de la Personas
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [Display(Name = "Primer Apellido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        public string Apellido_1 { get; set; }

        [Display(Name = "Segundo Apellido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        public string Apellido_2 { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [Display(Name = "Tipo de Documento")]
        [StringLength(20, ErrorMessage = "El tipo de documento no puede exceder los 20 caracteres")]
        public string tipo_documento { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [Display(Name = "Número de Documento")]
        public long numero_documento { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string Teléfono { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [Display(Name = "Dirección")]
        [StringLength(100, ErrorMessage = "La dirección no puede exceder los 100 caracteres")]
        public string dirección { get; set; }

        // Propiedades adicionales para la vista
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto
        {
            get
            {
                return $"{Nombre} {Apellido_1} {Apellido_2}";
            }
        }

        [Display(Name = "Total Compras")]
        public decimal TotalCompras { get; set; }

        [Display(Name = "Última Compra")]
        public DateTime? UltimaCompra { get; set; }

        // Esto es para las lista de facturas del cliente
        public List<FacturaResumenViewModel> Facturas { get; set; }

        public ClienteViewModel()
        {
            Facturas = new List<FacturaResumenViewModel>();
        }

        // Este método lo hago para convertir la entidad a ViewModel
        public static ClienteViewModel FromEntity(Clientes cliente)
        {
            var viewModel = new ClienteViewModel
            {
                id_cliente = cliente.id_cliente,
                ID_Persona = cliente.ID_Persona
            };

            if (cliente.Personas != null)
            {
                viewModel.Nombre = cliente.Personas.Nombre;
                viewModel.Apellido_1 = cliente.Personas.Apellido_1;
                viewModel.Apellido_2 = cliente.Personas.Apellido_2;
                viewModel.tipo_documento = cliente.Personas.tipo_documento;
                viewModel.numero_documento = cliente.Personas.numero_documento;
                viewModel.Correo = cliente.Personas.Correo;
                viewModel.Teléfono = cliente.Personas.Teléfono;
                viewModel.dirección = cliente.Personas.dirección;
            }

            if (cliente.Facturas != null && cliente.Facturas.Count > 0)
            {
                viewModel.TotalCompras = cliente.Facturas
                    .Where(f => f.estado == "Completada")
                    .Sum(f => f.total);

                viewModel.UltimaCompra = cliente.Facturas
                    .Where(f => f.estado == "Completada")
                    .OrderByDescending(f => f.fecha)
                    .Select(f => f.fecha)
                    .FirstOrDefault();

                viewModel.Facturas = cliente.Facturas
                    .OrderByDescending(f => f.fecha)
                    .Take(10)
                    .Select(f => new FacturaResumenViewModel
                    {
                        id_Factura = f.id_Factura,
                        fecha = f.fecha,
                        total = f.total,
                        estado = f.estado,
                        CantidadProductos = f.Detalles_Factura != null ? f.Detalles_Factura.Count : 0
                    })
                    .ToList();
            }

            return viewModel;
        }

        // Este método es para convertir el ViewModel a entidades, al revés que el anterior
        public Tuple<Personas, Clientes> ToEntities()
        {
            var persona = new Personas
            {
                ID_Persona = this.ID_Persona,
                Nombre = this.Nombre,
                Apellido_1 = this.Apellido_1,
                Apellido_2 = this.Apellido_2,
                tipo_documento = this.tipo_documento,
                numero_documento = this.numero_documento,
                Correo = this.Correo,
                Teléfono = this.Teléfono,
                dirección = this.dirección
            };

            var cliente = new Clientes
            {
                id_cliente = this.id_cliente,
                ID_Persona = this.ID_Persona
            };

            return new Tuple<Personas, Clientes>(persona, cliente);
        }
    }

    public class FacturaResumenViewModel
    {
        public int id_Factura { get; set; }
        public DateTime? fecha { get; set; }
        public decimal total { get; set; }
        public string estado { get; set; }
        public int CantidadProductos { get; set; }
    }
}