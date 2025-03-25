using System.Web;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.Filters
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        private readonly string[] _roles;

        public AuthorizeRolesAttribute(params string[] roles)
        {
            _roles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // Verifica si el usuario está autenticado
            if (httpContext.Session["UserID"] == null)
            {
                return false;
            }

            // Obtiene el rol del usuario de la sesión
            string userRole = httpContext.Session["UserRole"]?.ToString();

            // Verifica si el usuario tiene alguno de los roles permitidos
            if (!string.IsNullOrEmpty(userRole))
            {
                foreach (var role in _roles)
                {
                    if (userRole == role)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Redirige a la página de acceso denegado, inicio de sesión o cualquier otra página que nosotros queramos
            if (filterContext.HttpContext.Session["UserID"] == null)
            {
                // Usuario no autenticado, redirige al Home para la pagina principal
                filterContext.Result = new RedirectResult("~/Home/Index");
            }
            else
            {
                // Usuario autenticado pero sin permiso, redirige a acceso denegado
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/AccesoDenegado.cshtml"
                };
            }
        }
    }
}