using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            var roles = db.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "ID_Rol", "Nombre");
            return View();
        }

        public JsonResult GetUsuarios(UsuarioTableRequest request)
        {
            var query = from u in db.Usuarios
                        join p in db.Personas on u.ID_Persona equals p.ID_Persona
                        join r in db.Roles on u.ID_Rol equals r.ID_Rol
                        select new UsuarioTableViewModel
                        {
                            ID_Usuario = u.ID_Usuario,
                            ID_Persona = u.ID_Persona,
                            usuario = u.usuario,
                            NombreCompleto = p.Nombre + " " + p.Apellido_1 + " " + p.Apellido_2,
                            NombreRol = r.Nombre,
                            Correo = p.Correo,
                            Telefono = p.Teléfono,
                            estado = u.estado,
                            ultimo_login = u.ultimo_login,
                            fecha_creacion = u.fecha_creacion
                        };

            // Filtrado por nombre de usuario o correo
            if (!string.IsNullOrEmpty(request.SearchValue))
            {
                query = query.Where(u => u.usuario.Contains(request.SearchValue) || u.Correo.Contains(request.SearchValue));
            }

            // Filtrado por rol
            if (request.RolId.HasValue)
            {
                query = query.Where(u => u.NombreRol == db.Roles.FirstOrDefault(r => r.ID_Rol == request.RolId)?.Nombre);
            }

            // Filtrado por estado
            if (request.Estado.HasValue)
            {
                query = query.Where(u => u.estado == request.Estado.Value);
            }

            // Ordenamiento
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                switch (request.SortColumn)
                {
                    case "usuario":
                        query = request.SortDirection == "asc" ? query.OrderBy(u => u.usuario) : query.OrderByDescending(u => u.usuario);
                        break;
                    case "NombreCompleto":
                        query = request.SortDirection == "asc" ? query.OrderBy(u => u.NombreCompleto) : query.OrderByDescending(u => u.NombreCompleto);
                        break;
                }
            }

            // Paginación
            var totalRecords = query.Count();
            var data = query.Skip(request.Start).Take(request.Length).ToList();

            // Respuesta
            var response = new UsuarioTableResponse
            {
                draw = request.Start,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id)
        {
            var usuario = db.Usuarios
                            .Where(u => u.ID_Usuario == id)
                            .FirstOrDefault();

            if (usuario == null)
            {
                return HttpNotFound();
            }

            var persona = db.Personas.Where(p => p.ID_Persona == usuario.ID_Persona).FirstOrDefault();
            var rol = db.Roles.Where(r => r.ID_Rol == usuario.ID_Rol).FirstOrDefault();

            var viewModel = new UsuarioTableViewModel
            {
                ID_Usuario = usuario.ID_Usuario,
                ID_Persona = persona.ID_Persona,
                usuario = usuario.usuario,
                NombreCompleto = persona.Nombre + " " + persona.Apellido_1 + " " + persona.Apellido_2,
                NombreRol = rol?.Nombre,
                Correo = persona.Correo,
                Telefono = persona.Teléfono,
                estado = usuario.estado,
                ultimo_login = usuario.ultimo_login,
                fecha_creacion = usuario.fecha_creacion
            };

            return View(viewModel);
        }

        public ActionResult Create()
        {
            var roles = db.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "ID_Rol", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Usuarios usuario)
        {
            if (ModelState.IsValid)
            {
                db.Usuarios.Add(usuario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var roles = db.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "ID_Rol", "Nombre", usuario.ID_Rol);
            return View(usuario);
        }

        public ActionResult Edit(int id)
        {
            var usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            var roles = db.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "ID_Rol", "Nombre", usuario.ID_Rol);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Usuarios usuario)
        {
            if (ModelState.IsValid)
            {
                db.Entry(usuario).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var roles = db.Roles.ToList();
            ViewBag.Roles = new SelectList(roles, "ID_Rol", "Nombre", usuario.ID_Rol);
            return View(usuario);
        }

        public ActionResult Delete(int id)
        {
            var usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var usuario = db.Usuarios.Find(id);
            db.Usuarios.Remove(usuario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
