using Proyecto_Final_Desarrollo_Web.Models;
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

        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Categorias()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult Medicamentos()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        public ActionResult CuidadoPersonal()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult Vitaminas()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        public ActionResult Bebes()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

    }
}