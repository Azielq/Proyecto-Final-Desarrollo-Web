using Proyecto_Final_Desarrollo_Web.TableViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using Proyecto_Final_Desarrollo_Web.Models;
namespace Proyecto_Final_Desarrollo_Web.Controllers
{
        public class ProveedoresController : Controller
        {
            // Lista ficticia de proveedores para pruebas
            private static List<ProveedorTableViewModel> proveedores = new List<ProveedorTableViewModel>
        {
            new ProveedorTableViewModel { Pk_Proveedor = 1, Nombre = "Proveedor A", Correo = "proveedorA@email.com", Telefono = "123456789", direccion = "Calle 123", activo = true, TotalCompras = 5000, UltimaCompra = DateTime.Now.AddDays(-10), NumeroCompras = 15 },
            new ProveedorTableViewModel { Pk_Proveedor = 2, Nombre = "Proveedor B", Correo = "proveedorB@email.com", Telefono = "987654321", direccion = "Avenida 456", activo = false, TotalCompras = 3000, UltimaCompra = DateTime.Now.AddDays(-30), NumeroCompras = 8 }
        };

        public ActionResult Proveedores()
        {
            return View("~/Views/Proveedor/Proveedores.cshtml", proveedores);
        }

            public ActionResult Detalles(int id)
            {
                var proveedor = proveedores.FirstOrDefault(p => p.Pk_Proveedor == id);
                if (proveedor == null)
                {
                return HttpNotFound();
            }
                return View("~/Views/Proveedor/Detalles.cshtml",proveedor);
            }
        }
    }


