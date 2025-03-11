using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;
using Proyecto_Final_Desarrollo_Web.TableViewModels;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class UsuariosController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();

        // GET: Usuarios
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetUsuarios(UsuarioTableRequest request)
        {
            var response = new UsuarioTableResponse
            {
                draw = request.Start
            };

            // Consulta base
            var query = db.Usuarios
                .Include(u => u.Personas)
                .Include(u => u.Roles)
                .AsQueryable();

            // Filtros
            if (request.RolId.HasValue)
            {
                query = query.Where(u => u.ID_Rol == request.RolId.Value);
            }

            if (request.Estado.HasValue)
            {
                query = query.Where(u => u.estado == request.Estado.Value);
            }

            // Busca por término
            if (!string.IsNullOrWhiteSpace(request.SearchValue))
            {
                var searchValue = request.SearchValue.ToLower();
                query = query.Where(u => u.usuario.ToLower().Contains(searchValue) ||
                                       u.Personas.Nombre.ToLower().Contains(searchValue) ||
                                       u.Personas.Apellido_1.ToLower().Contains(searchValue) ||
                                       u.Personas.Correo.ToLower().Contains(searchValue) ||
                                       u.Roles.Nombre.ToLower().Contains(searchValue));
            }

            // Total de registros sin filtrar
            response.recordsTotal = db.Usuarios.Count();

            // Total después de filtrar
            response.recordsFiltered = query.Count();

            // Ordena
            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                if (request.SortDirection.ToLower() == "asc")
                {
                    switch (request.SortColumn)
                    {
                        case "usuario":
                            query = query.OrderBy(u => u.usuario);
                            break;
                        case "NombreCompleto":
                            query = query.OrderBy(u => u.Personas.Nombre).ThenBy(u => u.Personas.Apellido_1);
                            break;
                        case "NombreRol":
                            query = query.OrderBy(u => u.Roles.Nombre);
                            break;
                        case "Correo":
                            query = query.OrderBy(u => u.Personas.Correo);
                            break;
                        case "estado":
                            query = query.OrderBy(u => u.estado);
                            break;
                        case "ultimo_login":
                            query = query.OrderBy(u => u.ultimo_login);
                            break;
                        default:
                            query = query.OrderBy(u => u.usuario);
                            break;
                    }
                }
                else
                {
                    switch (request.SortColumn)
                    {
                        case "usuario":
                            query = query.OrderByDescending(u => u.usuario);
                            break;
                        case "NombreCompleto":
                            query = query.OrderByDescending(u => u.Personas.Nombre).ThenByDescending(u => u.Personas.Apellido_1);
                            break;
                        case "NombreRol":
                            query = query.OrderByDescending(u => u.Roles.Nombre);
                            break;
                        case "Correo":
                            query = query.OrderByDescending(u => u.Personas.Correo);
                            break;
                        case "estado":
                            query = query.OrderByDescending(u => u.estado);
                            break;
                        case "ultimo_login":
                            query = query.OrderByDescending(u => u.ultimo_login);
                            break;
                        default:
                            query = query.OrderByDescending(u => u.usuario);
                            break;
                    }
                }
            }
            else
            {
                query = query.OrderBy(u => u.usuario);
            }

            // Paginación
            query = query.Skip(request.Start).Take(request.Length);

            // Convierte y asigna a la respuesta
            response.data = query.ToList().Select(u => new UsuarioTableViewModel
            {
                ID_Usuario = u.ID_Usuario,
                ID_Persona = u.ID_Persona,
                usuario = u.usuario,
                NombreCompleto = $"{u.Personas.Nombre} {u.Personas.Apellido_1}",
                NombreRol = u.Roles.Nombre,
                Correo = u.Personas.Correo,
                Telefono = u.Personas.Teléfono,
                estado = u.estado,
                ultimo_login = u.ultimo_login,
                fecha_creacion = u.fecha_creacion
            }).ToList();

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // GET: Usuarios/Details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Usuarios usuario = db.Usuarios
                .Include(u => u.Personas)
                .Include(u => u.Roles)
                .FirstOrDefault(u => u.ID_Usuario == id);

            if (usuario == null)
            {
                return HttpNotFound();
            }

            var viewModel = new UsuarioViewModel
            {
                ID_Usuario = usuario.ID_Usuario,
                ID_Persona = usuario.ID_Persona,
                ID_Rol = usuario.ID_Rol,
                usuario = usuario.usuario,
                estado = usuario.estado,
                ultimo_login = usuario.ultimo_login,
                fecha_creacion = usuario.fecha_creacion,
                NombreRol = usuario.Roles.Nombre,

                // Datos de persona
                Nombre = usuario.Personas.Nombre,
                Apellido_1 = usuario.Personas.Apellido_1,
                Apellido_2 = usuario.Personas.Apellido_2,
                tipo_documento = usuario.Personas.tipo_documento,
                numero_documento = usuario.Personas.numero_documento,
                Correo = usuario.Personas.Correo,
                Teléfono = usuario.Personas.Teléfono,
                dirección = usuario.Personas.dirección
            };

            return View(viewModel);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre");
            return View(new UsuarioViewModel());
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Verifica si ya existe un usuario con el mismo nombre de usuario
                if (db.Usuarios.Any(u => u.usuario == viewModel.usuario))
                {
                    ModelState.AddModelError("usuario", "El nombre de usuario ya está en uso");
                    ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                    return View(viewModel);
                }

                // Verifica si ya existe una persona con el mismo número de documento
                if (db.Personas.Any(p => p.numero_documento == viewModel.numero_documento))
                {
                    ModelState.AddModelError("numero_documento", "Ya existe una persona con este número de documento");
                    ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                    return View(viewModel);
                }

                // Crea la persona
                var persona = new Personas
                {
                    Nombre = viewModel.Nombre,
                    Apellido_1 = viewModel.Apellido_1,
                    Apellido_2 = viewModel.Apellido_2,
                    tipo_documento = viewModel.tipo_documento,
                    numero_documento = viewModel.numero_documento,
                    Correo = viewModel.Correo,
                    Teléfono = viewModel.Teléfono,
                    dirección = viewModel.dirección
                };

                db.Personas.Add(persona);
                db.SaveChanges();

                // Crea el usuario
                var usuario = new Usuarios
                {
                    ID_Persona = persona.ID_Persona,
                    ID_Rol = viewModel.ID_Rol,
                    usuario = viewModel.usuario,
                    hash_clave = GetSHA256(viewModel.Password),
                    estado = 1, // Activo
                    fecha_creacion = DateTime.Now
                };

                db.Usuarios.Add(usuario);
                db.SaveChanges();

                TempData["Message"] = "Usuario creado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
            return View(viewModel);
        }

        // GET: Usuarios/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Usuarios usuario = db.Usuarios
                .Include(u => u.Personas)
                .FirstOrDefault(u => u.ID_Usuario == id);

            if (usuario == null)
            {
                return HttpNotFound();
            }

            var viewModel = new UsuarioViewModel
            {
                ID_Usuario = usuario.ID_Usuario,
                ID_Persona = usuario.ID_Persona,
                ID_Rol = usuario.ID_Rol,
                usuario = usuario.usuario,
                estado = usuario.estado,

                // Datos de persona
                Nombre = usuario.Personas.Nombre,
                Apellido_1 = usuario.Personas.Apellido_1,
                Apellido_2 = usuario.Personas.Apellido_2,
                tipo_documento = usuario.Personas.tipo_documento,
                numero_documento = usuario.Personas.numero_documento,
                Correo = usuario.Personas.Correo,
                Teléfono = usuario.Personas.Teléfono,
                dirección = usuario.Personas.dirección
            };

            ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", usuario.ID_Rol);
            return View(viewModel);
        }

        // POST: Usuarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UsuarioViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Verifica si ya existe otro usuario con el mismo nombre de usuario
                if (db.Usuarios.Any(u => u.usuario == viewModel.usuario && u.ID_Usuario != viewModel.ID_Usuario))
                {
                    ModelState.AddModelError("usuario", "El nombre de usuario ya está en uso");
                    ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                    return View(viewModel);
                }

                // Verifica si ya existe otra persona con el mismo número de documento
                if (db.Personas.Any(p => p.numero_documento == viewModel.numero_documento && p.ID_Persona != viewModel.ID_Persona))
                {
                    ModelState.AddModelError("numero_documento", "Ya existe otra persona con este número de documento");
                    ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                    return View(viewModel);
                }

                // Actualiza la persona
                var persona = db.Personas.Find(viewModel.ID_Persona);
                persona.Nombre = viewModel.Nombre;
                persona.Apellido_1 = viewModel.Apellido_1;
                persona.Apellido_2 = viewModel.Apellido_2;
                persona.tipo_documento = viewModel.tipo_documento;
                persona.numero_documento = viewModel.numero_documento;
                persona.Correo = viewModel.Correo;
                persona.Teléfono = viewModel.Teléfono;
                persona.dirección = viewModel.dirección;

                db.Entry(persona).State = EntityState.Modified;

                // Actualiza el usuario
                var usuario = db.Usuarios.Find(viewModel.ID_Usuario);
                usuario.ID_Rol = viewModel.ID_Rol;
                usuario.usuario = viewModel.usuario;
                usuario.estado = viewModel.estado;

                // Actualiza la contraseña solo si se proporcionó una nueva
                if (!string.IsNullOrEmpty(viewModel.Password))
                {
                    usuario.hash_clave = GetSHA256(viewModel.Password);
                }

                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Usuario actualizado correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
            return View(viewModel);
        }

        // POST: Usuarios/ToggleEstado
        [HttpPost]
        public ActionResult ToggleEstado(int id)
        {
            var usuario = db.Usuarios.Find(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            // Cambia el estado (0: Inactivo, 1: Activo, 2: Bloqueado)
            usuario.estado = usuario.estado == 1 ? 0 : 1;
            db.Entry(usuario).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { success = true, estado = usuario.estado });
        }

        // GET: Usuarios/CambiarPassword
        public ActionResult CambiarPassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Usuarios usuario = db.Usuarios
                .Include(u => u.Personas)
                .FirstOrDefault(u => u.ID_Usuario == id);

            if (usuario == null)
            {
                return HttpNotFound();
            }

            var viewModel = new UsuarioViewModel
            {
                ID_Usuario = usuario.ID_Usuario,
                usuario = usuario.usuario,
                Nombre = usuario.Personas.Nombre,
                Apellido_1 = usuario.Personas.Apellido_1
            };

            return View(viewModel);
        }

        // POST: Usuarios/CambiarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarPassword(UsuarioViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.Password))
            {
                ModelState.AddModelError("Password", "La contraseña es obligatoria");
            }

            if (string.IsNullOrEmpty(viewModel.ConfirmPassword))
            {
                ModelState.AddModelError("ConfirmPassword", "La confirmación de contraseña es obligatoria");
            }

            if (viewModel.Password != viewModel.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Las contraseñas no coinciden");
            }

            if (ModelState.IsValid)
            {
                var usuario = db.Usuarios.Find(viewModel.ID_Usuario);
                if (usuario == null)
                {
                    return HttpNotFound();
                }

                usuario.hash_clave = GetSHA256(viewModel.Password);
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "Contraseña actualizada correctamente";
                TempData["MessageType"] = "success";

                return RedirectToAction("Index");
            }

            var usuarioDb = db.Usuarios
                .Include(u => u.Personas)
                .FirstOrDefault(u => u.ID_Usuario == viewModel.ID_Usuario);

            viewModel.Nombre = usuarioDb.Personas.Nombre;
            viewModel.Apellido_1 = usuarioDb.Personas.Apellido_1;

            return View(viewModel);
        }

        // GET: Usuarios/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Usuarios/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var passwordHash = GetSHA256(model.Password);
                var usuario = db.Usuarios
                    .Include(u => u.Personas)
                    .Include(u => u.Roles)
                    .FirstOrDefault(u => u.usuario == model.Usuario && u.hash_clave == passwordHash);

                if (usuario != null)
                {
                    if (usuario.estado == 0) // 0: Inactivo
                    {
                        ModelState.AddModelError("", "El usuario está inactivo");
                        return View(model);
                    }

                    if (usuario.estado == 2) // 2: Bloqueado
                    {
                        ModelState.AddModelError("", "El usuario está bloqueado");
                        return View(model);
                    }

                    // Actualiza último login
                    usuario.ultimo_login = DateTime.Now;
                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    // Almacena información del usuario en sesión
                    Session["UserID"] = usuario.ID_Usuario;
                    Session["UserName"] = usuario.usuario;
                    Session["UserFullName"] = $"{usuario.Personas.Nombre} {usuario.Personas.Apellido_1}";
                    Session["UserRole"] = usuario.Roles.Nombre;
                    Session["UserRoleID"] = usuario.ID_Rol;

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos");
                }
            }

            return View(model);
        }

        // GET: Usuarios/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        // Método para generar hash SHA256
        private string GetSHA256(string str)
        {
            SHA256 sha256 = SHA256.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
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