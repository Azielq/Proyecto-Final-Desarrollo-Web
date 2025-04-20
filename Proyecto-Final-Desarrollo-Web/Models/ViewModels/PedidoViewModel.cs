using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    // ViewModel para la lista de pedidos
    public class PedidoViewModel
    {
        [Key]
        [Display(Name = "ID")]
        public int ID_Pedido { get; set; }

        [Required(ErrorMessage = "El número de pedido es obligatorio")]
        [Display(Name = "Nº Pedido")]
        [StringLength(20, ErrorMessage = "El número de pedido no puede exceder los 20 caracteres")]
        public string NumeroPedido { get; set; }

        [Required(ErrorMessage = "La fecha del pedido es obligatoria")]
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime FechaPedido { get; set; }

        [Required(ErrorMessage = "El subtotal es obligatorio")]
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor a 0")]
        public decimal Subtotal { get; set; }

        [Required(ErrorMessage = "El impuesto es obligatorio")]
        [Display(Name = "Impuesto")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "El impuesto debe ser mayor o igual a 0")]
        public decimal Impuesto { get; set; }

        [Required(ErrorMessage = "Los gastos de envío son obligatorios")]
        [Display(Name = "Envío")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "Los gastos de envío deben ser mayor o igual a 0")]
        public decimal GastosEnvio { get; set; }

        [Display(Name = "Descuento")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
        public decimal Descuento { get; set; }

        [Required(ErrorMessage = "El total es obligatorio")]
        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El total debe ser mayor a 0")]
        public decimal Total { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Display(Name = "Estado")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
        public string Estado { get; set; }

        [Display(Name = "Productos")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe haber al menos un producto en el pedido")]
        public int NumeroProductos { get; set; }

        [Display(Name = "Valorado")]
        public bool Calificado { get; set; }
    }

    // ViewModel para los detalles de un pedido
    public class PedidoDetalleViewModel
    {
        [Key]
        [Display(Name = "ID")]
        public int ID_Pedido { get; set; }

        [Required(ErrorMessage = "El número de pedido es obligatorio")]
        [Display(Name = "Nº Pedido")]
        [StringLength(20, ErrorMessage = "El número de pedido no puede exceder los 20 caracteres")]
        public string NumeroPedido { get; set; }

        [Required(ErrorMessage = "La fecha del pedido es obligatoria")]
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime FechaPedido { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Display(Name = "Estado")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los 20 caracteres")]
        public string Estado { get; set; }

        [Display(Name = "Nº Seguimiento")]
        [StringLength(50, ErrorMessage = "El número de seguimiento no puede exceder los 50 caracteres")]
        public string NumeroSeguimiento { get; set; }

        // Datos del cliente
        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "La dirección de envío es obligatoria")]
        [Display(Name = "Dirección")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string DireccionEnvio { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [Display(Name = "Ciudad")]
        [StringLength(50, ErrorMessage = "La ciudad no puede exceder los 50 caracteres")]
        public string CiudadEnvio { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio")]
        [Display(Name = "Código Postal")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 dígitos")]
        public string CodigoPostal { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres")]
        public string Email { get; set; }

        // Método de pago
        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [Display(Name = "Método de Pago")]
        [StringLength(50, ErrorMessage = "El método de pago no puede exceder los 50 caracteres")]
        public string MetodoPago { get; set; }

        [Display(Name = "Últimos Dígitos")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Debe contener exactamente 4 dígitos")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Deben ser 4 dígitos")]
        public string UltimosDigitosTarjeta { get; set; }

        // Información de costos
        [Required(ErrorMessage = "El subtotal es obligatorio")]
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor a 0")]
        public decimal Subtotal { get; set; }

        [Required(ErrorMessage = "El impuesto es obligatorio")]
        [Display(Name = "Impuesto")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "El impuesto debe ser mayor o igual a 0")]
        public decimal Impuesto { get; set; }

        [Required(ErrorMessage = "Los gastos de envío son obligatorios")]
        [Display(Name = "Envío")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "Los gastos de envío deben ser mayor o igual a 0")]
        public decimal GastosEnvio { get; set; }

        [Display(Name = "Descuento")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
        public decimal Descuento { get; set; }

        [Required(ErrorMessage = "El total es obligatorio")]
        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El total debe ser mayor a 0")]
        public decimal Total { get; set; }

        // Productos del pedido
        [Display(Name = "Productos")]
        public List<PedidoItemViewModel> Productos { get; set; }

        // Historial de estados del pedido
        [Display(Name = "Historial de Estados")]
        public List<PedidoEstadoViewModel> HistorialEstados { get; set; }

        // Valoración
        [Display(Name = "Valorado")]
        public bool Calificado { get; set; }

        [Display(Name = "Valoración")]
        [Range(1, 5, ErrorMessage = "La valoración debe estar entre 1 y 5 estrellas")]
        public int? Valoracion { get; set; }

        [Display(Name = "Comentario")]
        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres")]
        public string Comentario { get; set; }
    }

    // ViewModel para los productos en un pedido
    public class PedidoItemViewModel
    {
        [Key]
        [Display(Name = "ID Producto")]
        public int ID_Producto { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        [Display(Name = "Producto")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreProducto { get; set; }

        [Display(Name = "Imagen")]
        [DataType(DataType.ImageUrl)]
        public string ImagenUrl { get; set; }

        [Required(ErrorMessage = "El precio unitario es obligatorio")]
        [Display(Name = "Precio")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioUnitario { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Display(Name = "Cantidad")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Cantidad { get; set; }

        [NotMapped]
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal => PrecioUnitario * Cantidad;
    }

    // ViewModel para los estados de un pedido
    public class PedidoEstadoViewModel
    {
        [Required(ErrorMessage = "El estado es obligatorio")]
        [Display(Name = "Estado")]
        [StringLength(50, ErrorMessage = "El estado no puede exceder los 50 caracteres")]
        public string Estado { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres")]
        public string Descripcion { get; set; }

        [Display(Name = "Icono")]
        [StringLength(30, ErrorMessage = "El icono no puede exceder los 30 caracteres")]
        public string IconoEstado { get; set; }

        [Display(Name = "Color")]
        [StringLength(20, ErrorMessage = "El color no puede exceder los 20 caracteres")]
        public string ColorEstado { get; set; }
    }

    // ViewModel para la confirmación de un pedido
    public class PedidoConfirmadoViewModel
    {
        [Key]
        [Display(Name = "ID")]
        public int ID_Pedido { get; set; }

        [Required(ErrorMessage = "El número de pedido es obligatorio")]
        [Display(Name = "Nº Pedido")]
        [StringLength(20, ErrorMessage = "El número de pedido no puede exceder los 20 caracteres")]
        public string NumeroPedido { get; set; }

        [Required(ErrorMessage = "La fecha del pedido es obligatoria")]
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        [DataType(DataType.DateTime)]
        public DateTime FechaPedido { get; set; }

        // Datos del cliente
        [Required(ErrorMessage = "El nombre del cliente es obligatorio")]
        [Display(Name = "Nombre")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string NombreCliente { get; set; }

        [Required(ErrorMessage = "La dirección de envío es obligatoria")]
        [Display(Name = "Dirección")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string DireccionEnvio { get; set; }

        [Required(ErrorMessage = "La ciudad es obligatoria")]
        [Display(Name = "Ciudad")]
        [StringLength(50, ErrorMessage = "La ciudad no puede exceder los 50 caracteres")]
        public string CiudadEnvio { get; set; }

        [Required(ErrorMessage = "El código postal es obligatorio")]
        [Display(Name = "Código Postal")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "El código postal debe tener 5 dígitos")]
        public string CodigoPostal { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "El formato del teléfono no es válido")]
        [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres")]
        public string Email { get; set; }

        // Método de pago
        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [Display(Name = "Método de Pago")]
        [StringLength(50, ErrorMessage = "El método de pago no puede exceder los 50 caracteres")]
        public string MetodoPago { get; set; }

        [Display(Name = "Últimos Dígitos")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Debe contener exactamente 4 dígitos")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Deben ser 4 dígitos")]
        public string UltimosDigitosTarjeta { get; set; }

        // Información de costos
        [Required(ErrorMessage = "El subtotal es obligatorio")]
        [Display(Name = "Subtotal")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor a 0")]
        public decimal Subtotal { get; set; }

        [Required(ErrorMessage = "El impuesto es obligatorio")]
        [Display(Name = "Impuesto")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "El impuesto debe ser mayor o igual a 0")]
        public decimal Impuesto { get; set; }

        [Required(ErrorMessage = "Los gastos de envío son obligatorios")]
        [Display(Name = "Envío")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "Los gastos de envío deben ser mayor o igual a 0")]
        public decimal GastosEnvio { get; set; }

        [Display(Name = "Descuento")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0, double.MaxValue, ErrorMessage = "El descuento debe ser mayor o igual a 0")]
        public decimal Descuento { get; set; }

        [Required(ErrorMessage = "El total es obligatorio")]
        [Display(Name = "Total")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El total debe ser mayor a 0")]
        public decimal Total { get; set; }

        // Productos del pedido
        [Display(Name = "Productos")]
        public List<PedidoItemViewModel> Productos { get; set; }
    }

    // ViewModel para valorar un pedido
    public class ValoracionPedidoViewModel
    {
        [Required(ErrorMessage = "El ID del pedido es obligatorio")]
        public int ID_Pedido { get; set; }

        [Required(ErrorMessage = "La valoración es obligatoria")]
        [Range(1, 5, ErrorMessage = "La valoración debe estar entre 1 y 5 estrellas")]
        public int Valoracion { get; set; }

        [StringLength(500, ErrorMessage = "El comentario no puede exceder los 500 caracteres")]
        public string Comentario { get; set; }
    }
}   