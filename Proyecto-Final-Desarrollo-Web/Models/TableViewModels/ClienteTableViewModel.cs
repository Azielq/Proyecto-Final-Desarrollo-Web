using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    public class ClienteTableViewModel
    {
        public int id_cliente { get; set; }
        public int ID_Persona { get; set; }

        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; }

        [Display(Name = "Documento")]
        public string Documento { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Correo")]
        public string Correo { get; set; }

        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Display(Name = "Total Compras")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalCompras { get; set; }

        [Display(Name = "Última Compra")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? UltimaCompra { get; set; }

        [Display(Name = "Compras Realizadas")]
        public int NumeroCompras { get; set; }
    }

    public class ClienteTableRequest
    {
        // Para la paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Para la búsqueda
        public string SearchValue { get; set; }

        // Para lo del ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
    }

    public class ClienteTableResponse
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<ClienteTableViewModel> data { get; set; }

        public ClienteTableResponse()
        {
            data = new List<ClienteTableViewModel>();
        }
    }
}