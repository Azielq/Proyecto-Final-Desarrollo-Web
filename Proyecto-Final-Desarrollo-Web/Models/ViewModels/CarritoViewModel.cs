using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.Models.ViewModels
{
	public class CarritoViewModel
	{
        public int ID_Carrito { get; set; }
        public int ID_Usuario { get; set; }
        public int ID_Producto { get; set; }
        public int Cantidad { get; set; }
        public Nullable<System.DateTime> FechaAgregado { get; set; }

        public virtual Productos Productos { get; set; }
        public virtual Usuarios Usuarios { get; set; }
    }
}