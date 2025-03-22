using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario o correo electrónico es obligatorio")]
        [DisplayName("Usuario o Correo")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Display(Name = "Recordarme")]
        public bool RememberMe { get; set; }
    }

    public class UsuarioViewModel : IValidatableObject
    {
        public int ID_Usuario { get; set; }
        public int ID_Persona { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Display(Name = "Rol")]
        public int ID_Rol { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Usuario")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "El usuario debe tener entre 4 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El usuario solo puede contener letras, números, puntos, guiones y guiones bajos")]
        public string usuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
        ErrorMessage = "La contraseña debe contener al menos una letra minúscula, una letra mayúscula, un número y un carácter especial.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Estado")]
        public int estado { get; set; }

        // Datos de persona
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        [Display(Name = "Primer Apellido")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
        public string Apellido_1 { get; set; }

        [Display(Name = "Segundo Apellido")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]*$", ErrorMessage = "El apellido solo puede contener letras y espacios")]
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
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Display(Name = "Teléfono")]
        [RegularExpression(@"^\+?[0-9]{8,15}$",
            ErrorMessage = "El teléfono debe tener entre 8 y 15 dígitos y puede incluir el signo '+' al inicio")]
        public string Teléfono { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        [Display(Name = "Dirección")]
        public string dirección { get; set; }

        // Información útil adicional
        [Display(Name = "Nombre del Rol")]
        public string NombreRol { get; set; }

        [Display(Name = "Último Acceso")]
        public System.DateTime? ultimo_login { get; set; }

        [Display(Name = "Fecha de Creación")]
        public System.DateTime? fecha_creacion { get; set; }

        // Validaciones adicionales
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validación de correo electrónico
            if (!string.IsNullOrEmpty(Correo))
            {
                // Esto me verifica que el correo tenga un formato válido más estricto
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                if (!emailRegex.IsMatch(Correo))
                {
                    yield return new ValidationResult(
                        "El formato del correo electrónico no es válido. Debe seguir el formato: usuario@dominio.com",
                        new[] { nameof(Correo) });
                }
            }

            // Validación de teléfono
            if (!string.IsNullOrEmpty(Teléfono))
            {
                // Limpia el teléfono de espacios, guiones, etc.
                var phoneClean = Regex.Replace(Teléfono, @"[\s\-\(\)]", "");

                // Esto me verifica que el teléfono tenga solo números y posiblemente un '+' al inicio
                var phoneRegex = new Regex(@"^\+?[0-9]{8,15}$");
                if (!phoneRegex.IsMatch(phoneClean))
                {
                    yield return new ValidationResult(
                        "El formato del teléfono no es válido. Debe contener entre 8 y 15 dígitos, puede incluir el prefijo internacional con '+' al inicio.",
                        new[] { nameof(Teléfono) });
                }
            }

            // Validación de número de documento
            var docRegex = new Regex(@"^[0-9]{6,12}$");
            if (!docRegex.IsMatch(numero_documento.ToString()))
            {
                yield return new ValidationResult(
                    "El número de documento debe tener entre 6 y 12 dígitos y solo puede contener números.",
                    new[] { nameof(numero_documento) });
            }
        }
    }

    public class CambiarPasswordViewModel
    {
        public int ID_Usuario { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string usuario { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Apellido")]
        public string Apellido_1 { get; set; }

        [Required(ErrorMessage = "La contraseña actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña actual")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "La nueva contraseña es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva contraseña")]
        [StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} caracteres.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
            ErrorMessage = "La contraseña debe contener al menos una letra minúscula, una letra mayúscula, un número y un carácter especial.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nueva contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }
    }
}