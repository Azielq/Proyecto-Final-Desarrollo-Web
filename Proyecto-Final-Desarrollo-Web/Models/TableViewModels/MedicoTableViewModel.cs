using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class MedicoTableViewModel
    {
        public int id_Medico { get; set; }
        public int ID_Persona { get; set; }

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Display(Name = "Especialidad")]
        public string Especialidad { get; set; }

        [Display(Name = "Registro Médico")]
        public string registro_medico { get; set; }

        [Display(Name = "Documento")]
        public string Documento { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Display(Name = "Dirección")]
        public string Direccion { get; set; }
    }

    public class MedicoTableRequest
    {
        // Para la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Para la búsqueda
        public string SearchValue { get; set; }

        // Para el ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        //  Estos son unos filtros adicionales 
        public string Especialidad { get; set; }
    }

    public class MedicoTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<MedicoTableViewModel> data { get; set; }

        public MedicoTableResponse()
        {
            data = new List<MedicoTableViewModel>();
        }
    }
}