using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Filters;
using Proyecto_Final_Desarrollo_Web.Helpers;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    [AuthorizeRoles("Administrador", "Supervisor")]
    public class RolesController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Roles
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            try
            {
                // Consulta base
                var query = db.Roles
                    .Include(r => r.Usuarios)
                    .AsQueryable();

                // Aplica filtro de búsqueda si está presente
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(r => r.Nombre.ToLower().Contains(searchTerm));
                }

                // Obtiene el total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var roles = query
                    .OrderBy(r => r.Nombre)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Configura la paginación usando el helper
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda el término de búsqueda en ViewBag para mantenerlo en la paginación
                ViewBag.SearchTerm = searchTerm;

                return View(roles);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar roles: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los roles: " + ex.Message;
                return View(new List<Roles>());
            }
        }

        // GET: Roles/GetUsuariosByRol
        [HttpGet]
        public ActionResult GetUsuariosByRol(int id)
        {
            try
            {
                var usuarios = db.Usuarios
                    .Include(u => u.Personas)
                    .Where(u => u.ID_Rol == id)
                    .Select(u => new {
                        ID_Usuario = u.ID_Usuario,
                        usuario = u.usuario,
                        nombreCompleto = u.Personas.Nombre + " " + u.Personas.Apellido_1,
                        correo = u.Personas.Correo,
                        estado = u.estado
                    })
                    .ToList();

                return Json(usuarios, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener usuarios por rol: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Roles/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                Roles rol = db.Roles
                    .Include(r => r.Usuarios)
                    .FirstOrDefault(r => r.ID_Rol == id);

                if (rol == null)
                {
                    return HttpNotFound();
                }

                // Obtiene los usuarios con este rol
                var usuarios = db.Usuarios
                    .Include(u => u.Personas)
                    .Where(u => u.ID_Rol == id)
                    .ToList();

                ViewBag.Usuarios = usuarios;
                ViewBag.TotalUsuarios = usuarios.Count;

                return View(rol);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar detalles del rol: {ex.Message}");
                TempData["Message"] = "Error al cargar detalles del rol: " + ex.Message;
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Rol,Nombre")] Roles rol)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica si ya existe un rol con el mismo nombre
                    if (db.Roles.Any(r => r.Nombre.ToLower() == rol.Nombre.ToLower()))
                    {
                        return Json(new { success = false, message = "Ya existe un rol con este nombre" });
                    }

                    db.Roles.Add(rol);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Rol creado correctamente" });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al crear rol: {ex.Message}");
                    return Json(new { success = false, message = "Error al crear el rol: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Error de validación: " + string.Join(", ", errors) });
        }

        // GET: Roles/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                Roles rol = db.Roles.Find(id);
                if (rol == null)
                {
                    return HttpNotFound();
                }

                return View(rol);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar rol para editar: {ex.Message}");
                TempData["Message"] = "Error al cargar el rol para editar: " + ex.Message;
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Rol,Nombre")] Roles rol)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica si ya existe otro rol con el mismo nombre
                    if (db.Roles.Any(r => r.Nombre.ToLower() == rol.Nombre.ToLower() && r.ID_Rol != rol.ID_Rol))
                    {
                        return Json(new { success = false, message = "Ya existe otro rol con este nombre" });
                    }

                    db.Entry(rol).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { success = true, message = "Rol actualizado correctamente" });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al actualizar rol: {ex.Message}");
                    return Json(new { success = false, message = "Error al actualizar el rol: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return Json(new { success = false, message = "Error de validación: " + string.Join(", ", errors) });
        }

        // GET: Roles/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                Roles rol = db.Roles
                    .Include(r => r.Usuarios)
                    .FirstOrDefault(r => r.ID_Rol == id);

                if (rol == null)
                {
                    return HttpNotFound();
                }

                // Verifica si tiene usuarios asociados
                ViewBag.TieneUsuarios = rol.Usuarios.Any();
                ViewBag.TotalUsuarios = rol.Usuarios.Count;

                return View(rol);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar rol para eliminar: {ex.Message}");
                TempData["Message"] = "Error al cargar el rol para eliminar: " + ex.Message;
                TempData["MessageType"] = "danger";
                return RedirectToAction("Index");
            }
        }

        // POST: Roles/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                Roles rol = db.Roles
                    .Include(r => r.Usuarios)
                    .FirstOrDefault(r => r.ID_Rol == id);

                if (rol == null)
                {
                    return Json(new { success = false, message = "El rol no existe" });
                }

                // Verifica si tiene usuarios asociados
                if (rol.Usuarios.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se puede eliminar el rol porque tiene usuarios asociados",
                        usuarios = rol.Usuarios.Count
                    });
                }

                db.Roles.Remove(rol);
                db.SaveChanges();

                return Json(new { success = true, message = "Rol eliminado correctamente" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al eliminar rol: {ex.Message}");
                return Json(new { success = false, message = "Error al eliminar el rol: " + ex.Message });
            }
        }

        // GET: Roles/GetRoles - Para uso en AJAX
        [HttpGet]
        public ActionResult GetRoles()
        {
            try
            {
                var roles = db.Roles
                    .Select(r => new { r.ID_Rol, r.Nombre })
                    .OrderBy(r => r.Nombre)
                    .ToList();

                return Json(roles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener roles: {ex.Message}");
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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