using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        // Acción para mostrar la vista de inicio de sesión
        public ActionResult Login()
        {
            return View();
        }

        // Acción para mostrar la vista de registro
        public ActionResult Register()
        {
            return View();
        }
    }
}