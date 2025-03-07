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
        [HttpPost]
        // GET: Home/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Home/Login
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            using (var context = new FarmaUEntities())
            {
                // Buscar el usuario por nombre de usuario
                var usuario = context.Usuarios.FirstOrDefault(u => u.usuario == username);

                if (usuario != null && VerifyPassword(password, usuario.hash_clave))
                {
                    // Actualiza la fecha de último login
                    usuario.ultimo_login = DateTime.Now;
                    context.SaveChanges();

                    // Aquí puedes implementar la lógica para iniciar la sesión (por ejemplo, FormsAuthentication o Identity)
                    // Ejemplo con FormsAuthentication:
                    // FormsAuthentication.SetAuthCookie(usuario.usuario, false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "Usuario o contraseña incorrectos";
                    return View();
                }
            }
        }

        // Método de ejemplo para validar la contraseña, suponiendo que el valor almacenado es un hash
        private bool VerifyPassword(string plainTextPassword, string storedHash)
        {
            // Implementa aquí la comparación segura del hash.
            // Podrías utilizar algoritmos como PBKDF2, bcrypt, etc.
            // Este es un ejemplo simplificado, debes ajustarlo a tu método de hash.
            return plainTextPassword.GetHashCode().ToString() == storedHash;
        }



        // Acción para mostrar la vista de registro
        public ActionResult Register()
        {
            return View();
        }
    }
}