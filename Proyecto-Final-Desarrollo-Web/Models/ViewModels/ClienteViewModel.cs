using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class ClienteViewModel : IValidatableObject
    {
        // Datos del Cliente
        public int id_cliente { get; set; }
        public int ID_Persona { get; set; }

        // Datos de la Personas
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [Display(Name = "Primer Apellido")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
        public string Apellido_1 { get; set; }

        [Display(Name = "Segundo Apellido")]
        [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]*$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
        public string Apellido_2 { get; set; }

        [Required(ErrorMessage = "El tipo de documento es obligatorio")]
        [Display(Name = "Tipo de Documento")]
        public string tipo_documento { get; set; }

        [Required(ErrorMessage = "El número de documento es obligatorio")]
        [Display(Name = "Número de Documento")]
        [RegularExpression(@"^[0-9]{6,12}$", ErrorMessage = "El número de documento debe tener entre 6 y 12 dígitos y solo puede contener números")]
        public long numero_documento { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        [RegularExpression(@"^\+?[0-9]{8,15}$",
            ErrorMessage = "El teléfono debe tener entre 8 y 15 dígitos y puede incluir el signo '+' al inicio")]
        public string Teléfono { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [Display(Name = "Dirección")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "La dirección debe tener entre 5 y 100 caracteres")]
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
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalCompras { get; set; }

        [Display(Name = "Última Compra")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? UltimaCompra { get; set; }

        // Esto es para las lista de facturas del cliente
        public List<FacturaResumenViewModel> Facturas { get; set; }

        public ClienteViewModel()
        {
            Facturas = new List<FacturaResumenViewModel>();
        }

        // Validaciones personalizadas adicionales
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validación avanzada del correo electrónico
            if (!string.IsNullOrEmpty(Correo))
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                if (!emailRegex.IsMatch(Correo))
                {
                    yield return new ValidationResult(
                        "El formato del correo electrónico no es válido. Debe seguir el formato: usuario@dominio.com",
                        new[] { nameof(Correo) });
                }
            }

            // Validación avanzada del teléfono
            if (!string.IsNullOrEmpty(Teléfono))
            {
                // Limpia el teléfono de espacios, guiones, etc.
                var phoneClean = Regex.Replace(Teléfono, @"[\s\-\(\)]", "");

                // Verifica que el teléfono tenga solo números y posiblemente un '+' al inicio
                var phoneRegex = new Regex(@"^\+?[0-9]{8,15}$");
                if (!phoneRegex.IsMatch(phoneClean))
                {
                    yield return new ValidationResult(
                        "El formato del teléfono no es válido. Debe contener entre 8 y 15 dígitos, puede incluir el prefijo internacional con '+' al inicio.",
                        new[] { nameof(Teléfono) });
                }
            }

            // Validaciones adicionales según necesidades del negocio pueden agregarse aquí
        }

        // Métodos de conversión - mantener los existentes
        public static ClienteViewModel FromEntity(Clientes cliente)
        {
            // El código existente se mantiene igual
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

        public Tuple<Personas, Clientes> ToEntities()
        {
            // El código existente se mantiene igual
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
