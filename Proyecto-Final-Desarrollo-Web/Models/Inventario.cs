//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Proyecto_Final_Desarrollo_Web.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Inventario
    {
        public int id_Inventario { get; set; }
        public int ID_Lote { get; set; }
        public string ubicacion { get; set; }
    
        public virtual Lotes Lotes { get; set; }
    }
}
