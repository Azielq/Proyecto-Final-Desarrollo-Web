using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Proyecto_Final_Desarrollo_Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Aquí puedes agregar propiedades adicionales si lo requieres

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // El tipo de autenticación debe coincidir con el definido en CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Puedes agregar aquí claims personalizados si es necesario
            return userIdentity;
        }
    }
}
