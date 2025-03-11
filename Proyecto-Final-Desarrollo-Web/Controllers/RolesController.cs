using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class RolesController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Roles
        public ActionResult Index()
        {
            var roles = db.Roles.ToList();
            return View(roles);
        }

        // GET: Roles/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

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

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID_Rol,Nombre")] Roles rol)
        {
            if (ModelState.IsValid)
            {
                db.Roles.Add(rol);
                db.SaveChanges();

                TempData["Message"] = "Rol creado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(rol);
        }

        // GET: Roles/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Roles rol = db.Roles.Find(id);
            if (rol == null)
            {
                return HttpNotFound();
            }

            return View(rol);
        }

        // POST: Roles/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID_Rol,Nombre")] Roles rol)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rol).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Rol actualizado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            return View(rol);
        }

        // GET: Roles/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Roles rol = db.Roles
                .Include(r => r.Usuarios)
                .FirstOrDefault(r => r.ID_Rol == id);

            if (rol == null)
            {
                return HttpNotFound();
            }

            // Verifica si tiene usuarios asociados
            if (rol.Usuarios.Any())
            {
                ViewBag.TieneUsuarios = true;
                ViewBag.TotalUsuarios = rol.Usuarios.Count;
            }
            else
            {
                ViewBag.TieneUsuarios = false;
            }

            return View(rol);
        }

        // POST: Roles/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Roles rol = db.Roles
                .Include(r => r.Usuarios)
                .FirstOrDefault(r => r.ID_Rol == id);

            // Verifica si tiene usuarios asociados
            if (rol.Usuarios.Any())
            {
                TempData["Message"] = "No se puede eliminar el rol porque tiene usuarios asociados";
                TempData["MessageType"] = "error";
                return RedirectToAction("Delete", new { id = id });
            }

            db.Roles.Remove(rol);
            db.SaveChanges();

            TempData["Message"] = "Rol eliminado correctamente";
            TempData["MessageType"] = "success";

            return RedirectToAction("Index");
        }

        // GET: Roles/GetRoles
        [HttpGet]
        public ActionResult GetRoles()
        {
            var roles = db.Roles
                .Select(r => new { r.ID_Rol, r.Nombre })
                .OrderBy(r => r.Nombre)
                .ToList();

            return Json(roles, JsonRequestBehavior.AllowGet);
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