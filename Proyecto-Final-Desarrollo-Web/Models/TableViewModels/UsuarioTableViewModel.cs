using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class UsuarioTableViewModel
    {
        public int ID_Usuario { get; set; }
        public int ID_Persona { get; set; }

        [Display(Name = "Usuario")]
        public string usuario { get; set; }

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Display(Name = "Rol")]
        public string NombreRol { get; set; }

        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Estado")]
        public int estado { get; set; }

        [Display(Name = "Último Acceso")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ultimo_login { get; set; }

        [Display(Name = "Fecha Creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? fecha_creacion { get; set; }

        // Propiedades calculadas
        public string EstadoTexto
        {
            get
            {
                switch (estado)
                {
                    case 1:
                        return "Activo";
                    case 0:
                        return "Inactivo";
                    case 2:
                        return "Bloqueado";
                    default:
                        return "Desconocido";
                }
            }
        }

        public string EstadoClass
        {
            get
            {
                switch (estado)
                {
                    case 1:
                        return "badge bg-success";
                    case 0:
                        return "badge bg-danger";
                    case 2:
                        return "badge bg-warning";
                    default:
                        return "badge bg-secondary";
                }
            }
        }
    }

    public class UsuarioTableRequest
    {
        // Para la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Para la búsqueda
        public string SearchValue { get; set; }

        // Para el ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        // Estos son algunos filtros adicionales  
        public int? RolId { get; set; }
        public int? Estado { get; set; }
    }

    public class UsuarioTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<UsuarioTableViewModel> data { get; set; }

        public UsuarioTableResponse()
        {
            data = new List<UsuarioTableViewModel>();
        }
    }
}