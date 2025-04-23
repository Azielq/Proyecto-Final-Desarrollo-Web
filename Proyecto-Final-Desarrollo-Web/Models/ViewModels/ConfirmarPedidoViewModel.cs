using System.ComponentModel.DataAnnotations;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class ConfirmarPedidoViewModel
    {
        // Datos Personales
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        public string Apellidos { get; set; }

        [Required(ErrorMessage = "El email es obligatorio"), EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio"), Phone]
        public string Telefono { get; set; }

        // Dirección
        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        public string Ciudad { get; set; }

        [Required(ErrorMessage = "La provincia es obligatoria")]
        public string Provincia { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio")]
        public string CodigoPostal { get; set; }

        // Método de pago
        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public string MetodoPago { get; set; }

        // Campos de tarjeta (opcionales si se usa otro método)
        public string NumeroTarjeta { get; set; }
        public string FechaVencimiento { get; set; }
        public string CVV { get; set; }
        public string NombreTarjeta { get; set; }

        // Notas adicionales
        public string Notas { get; set; }
    }
}
