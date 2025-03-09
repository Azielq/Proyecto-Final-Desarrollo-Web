using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class LoginController : Controller
    {
        private PBKDF2PasswordHasher _passwordHasher = new PBKDF2PasswordHasher();

        [HttpGet]
        public ActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            using (var db = new FarmaUEntities())
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.usuario == model.Usuario);

                if (usuario == null || _passwordHasher.VerifyHashedPassword(usuario.hash_clave, model.Password) != Microsoft.AspNet.Identity.PasswordVerificationResult.Success)
                {
                    ViewBag.ErrorMessage = "Usuario o contraseña incorrectos";
                    return View("Index"); // Asegura que regresa a la vista correcta
                }

                if (usuario.estado == 0)
                {
                    ViewBag.ErrorMessage = "Esta cuenta está inactiva. Contacte al administrador.";
                    return RedirectToAction("Index", "Home"); // Si el login es exitoso
                }

                // Actualiza la fecha de último acceso
                usuario.ultimo_login = DateTime.Now;
                db.SaveChanges();

                // Establece la sesión
                Session["Usuario"] = usuario.usuario;
                Session["ID_Usuario"] = usuario.ID_Usuario;
                Session["Rol"] = usuario.ID_Rol;

                // Mensaje de éxito usando TempData
                TempData["SuccessMessage"] = "Inicio de sesión exitoso. ¡Bienvenido " + usuario.usuario + "!";

                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }
    }
}
