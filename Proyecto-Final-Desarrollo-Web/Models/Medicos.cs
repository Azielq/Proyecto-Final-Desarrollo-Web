//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Proyecto_Final_Desarrollo_Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Medicos
    {
        public int id_Medico { get; set; }
        public int ID_Persona { get; set; }
        public string Especialidad { get; set; }
        public string registro_medico { get; set; }
    
        public virtual Personas Personas { get; set; }
    }
}
