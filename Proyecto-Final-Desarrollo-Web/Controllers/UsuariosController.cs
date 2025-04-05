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
using Microsoft.AspNet.Identity;
using Proyecto_Final_Desarrollo_Web.Helpers;
using Proyecto_Final_Desarrollo_Web.Filters;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class UsuariosController : Controller
    {
        private FarmaUEntities db = new FarmaUEntities();
        private PBKDF2PasswordHasher _passwordHasher = new PBKDF2PasswordHasher();

        [AuthorizeRoles("Administrador")]
        // GET: Usuarios
        public ActionResult Index(int page = 1, int pageSize = 10, string searchTerm = "", int? rolId = null, int? estado = null)
        {
            try
            {
                // Carga lista de roles para el filtro
                ViewBag.Roles = new SelectList(db.Roles, "ID_Rol", "Nombre");

                // Consulta base
                var query = db.Usuarios
                    .Include(u => u.Personas)
                    .Include(u => u.Roles)
                    .AsQueryable();

                // Aplica filtros si están presentes
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(u =>
                        u.usuario.ToLower().Contains(searchTerm) ||
                        u.Personas.Nombre.ToLower().Contains(searchTerm) ||
                        u.Personas.Apellido_1.ToLower().Contains(searchTerm) ||
                        u.Personas.Correo.ToLower().Contains(searchTerm) ||
                        u.Roles.Nombre.ToLower().Contains(searchTerm)
                    );
                }

                if (rolId.HasValue)
                {
                    query = query.Where(u => u.ID_Rol == rolId.Value);
                }

                if (estado.HasValue)
                {
                    query = query.Where(u => u.estado == estado.Value);
                }

                // Obtiene el total de registros para paginación
                var totalRecords = query.Count();

                // Aplica ordenamiento y paginación
                var usuariosData = query
                    .OrderBy(u => u.usuario)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Transforma a ViewModel
                var usuarios = usuariosData.Select(u => new UsuarioTableViewModel
                {
                    ID_Usuario = u.ID_Usuario,
                    ID_Persona = u.ID_Persona,
                    usuario = u.usuario,
                    NombreCompleto = u.Personas.Nombre + " " + u.Personas.Apellido_1,
                    NombreRol = u.Roles.Nombre,
                    Correo = u.Personas.Correo,
                    Telefono = u.Personas.Teléfono,
                    estado = u.estado,
                    ultimo_login = u.ultimo_login,
                    fecha_creacion = u.fecha_creacion
                }).ToList();

                // Configura la paginación usando el helper
                PaginationHelper.ConfigurePagination(ViewData, totalRecords, page, pageSize);

                // Guarda los filtros en ViewBag para mantenerlos en la paginación
                ViewBag.SearchTerm = searchTerm;
                ViewBag.RolId = rolId;
                ViewBag.Estado = estado;

                return View(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar usuarios: {ex.Message}");
                ViewBag.ErrorMessage = "Error al cargar los usuarios: " + ex.Message;
                return View(new List<UsuarioTableViewModel>());
            }
        }

        [AuthorizeRoles("Administrador")]
        [HttpPost]
        public ActionResult GetUsuarios(UsuarioTableRequest request)
        {
            try
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

                // Aplica filtros
                if (request.RolId.HasValue)
                {
                    query = query.Where(u => u.ID_Rol == request.RolId.Value);
                }

                if (request.Estado.HasValue)
                {
                    query = query.Where(u => u.estado == request.Estado.Value);
                }

                // Busca término
                if (!string.IsNullOrWhiteSpace(request.SearchValue))
                {
                    var searchValue = request.SearchValue.ToLower();
                    query = query.Where(u => u.usuario.ToLower().Contains(searchValue) ||
                                           u.Personas.Nombre.ToLower().Contains(searchValue) ||
                                           u.Personas.Apellido_1.ToLower().Contains(searchValue) ||
                                           u.Personas.Correo.ToLower().Contains(searchValue) ||
                                           u.Roles.Nombre.ToLower().Contains(searchValue));
                }

                // Total sin filtrar
                response.recordsTotal = db.Usuarios.Count();

                // Total filtrado
                response.recordsFiltered = query.Count();

                // Aplica ordenación
                if (!string.IsNullOrEmpty(request.SortColumn))
                {
                    // Configuración de ordenación...
                }
                else
                {
                    query = query.OrderBy(u => u.usuario);
                }

                // Primero trae a memoria con paginación
                var usuariosData = query.Skip(request.Start).Take(request.Length).ToList();

                // Luego hace transformaciones
                response.data = usuariosData.Select(u => new UsuarioTableViewModel
                {
                    ID_Usuario = u.ID_Usuario,
                    ID_Persona = u.ID_Persona,
                    usuario = u.usuario,
                    NombreCompleto = u.Personas.Nombre + " " + u.Personas.Apellido_1, // Ahora es seguro concatenar
                    NombreRol = u.Roles.Nombre,
                    Correo = u.Personas.Correo,
                    Telefono = u.Personas.Teléfono,
                    estado = u.estado,
                    ultimo_login = u.ultimo_login,
                    fecha_creacion = u.fecha_creacion
                }).ToList();

                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, success = false }, JsonRequestBehavior.AllowGet);
            }
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

        [AuthorizeRoles("Administrador")]
        // GET: Usuarios/Create
        public ActionResult Create()
        {
            ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre");
            ViewBag.MostrarRol = true;
            return View(new UsuarioViewModel());
        }

        [AuthorizeRoles("Administrador")]
        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UsuarioViewModel viewModel)
        {
            // Valida formato de campos importantes
            ValidarCamposImportantes(viewModel);

            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica si ya existe un usuario con el mismo nombre de usuario
                    if (db.Usuarios.Any(u => u.usuario == viewModel.usuario))
                    {
                        ModelState.AddModelError("usuario", "El nombre de usuario ya está en uso");
                        ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                        ViewBag.MostrarRol = true;
                        return View(viewModel);
                    }

                    // Verifica si ya existe una persona con el mismo número de documento
                    if (db.Personas.Any(p => p.numero_documento == viewModel.numero_documento))
                    {
                        ModelState.AddModelError("numero_documento", "Ya existe una persona con este número de documento");
                        ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                        ViewBag.MostrarRol = true;
                        return View(viewModel);
                    }

                    // Verifica si ya existe una persona con el mismo correo electrónico
                    if (db.Personas.Any(p => p.Correo == viewModel.Correo))
                    {
                        ModelState.AddModelError("Correo", "El correo electrónico ya está en uso");
                        ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                        ViewBag.MostrarRol = true;
                        return View(viewModel);
                    }

                    // Verifica si ya existe una persona con el mismo número de teléfono
                    if (db.Personas.Any(p => p.Teléfono == viewModel.Teléfono))
                    {
                        ModelState.AddModelError("Teléfono", "El número de teléfono ya está en uso");
                        ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                        ViewBag.MostrarRol = true;
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
                        hash_clave = _passwordHasher.HashPassword(viewModel.Password),
                        estado = 1, // Activo
                        fecha_creacion = DateTime.Now
                    };

                    db.Usuarios.Add(usuario);
                    db.SaveChanges();

                    TempData["Message"] = "Usuario creado correctamente";
                    TempData["MessageDetail"] = $"El usuario {usuario.usuario} ha sido creado con éxito.";
                    TempData["MessageType"] = "success";
                    TempData["UseSweet"] = true;

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al crear usuario: {ex.Message}");
                    ModelState.AddModelError("", $"Ha ocurrido un error al crear el usuario: {ex.Message}");
                }
            }

            ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
            ViewBag.MostrarRol = true;
            return View(viewModel);
        }

        // GET: Usuarios/Register
        public ActionResult Register()
        {
            // Esto asigna por defecto el rol de cliente (el ID 51 es el rol Cliente)
            var viewModel = new UsuarioViewModel { ID_Rol = 51 };
            ViewBag.MostrarRol = false; // Para no mostrar el selector de rol
            return View(viewModel);
        }

        // POST: Usuarios/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UsuarioViewModel viewModel)
        {
            // Esto asigna por defecto el rol de cliente (el ID 51 es el rol Cliente)
            viewModel.ID_Rol = 51;

            // Valida formato de campos importantes
            ValidarCamposImportantes(viewModel);

            if (ModelState.IsValid)
            {
                try
                {
                    // Esto verifica si ya existe un usuario con el mismo nombre de usuario
                    if (db.Usuarios.Any(u => u.usuario == viewModel.usuario))
                    {
                        ModelState.AddModelError("usuario", "El nombre de usuario ya está en uso");
                        ViewBag.MostrarRol = false;
                        return View(viewModel);
                    }

                    // Esto verifica si ya existe una persona con el mismo número de documento
                    if (db.Personas.Any(p => p.numero_documento == viewModel.numero_documento))
                    {
                        ModelState.AddModelError("numero_documento", "Ya existe una persona con este número de documento");
                        ViewBag.MostrarRol = false;
                        return View(viewModel);
                    }

                    // Verifica si ya existe una persona con el mismo correo electrónico
                    if (db.Personas.Any(p => p.Correo == viewModel.Correo))
                    {
                        ModelState.AddModelError("Correo", "El correo electrónico ya está en uso");
                        ViewBag.MostrarRol = false;
                        return View(viewModel);
                    }

                    // Verifica si ya existe una persona con el mismo número de teléfono
                    if (db.Personas.Any(p => p.Teléfono == viewModel.Teléfono))
                    {
                        ModelState.AddModelError("Teléfono", "El número de teléfono ya está en uso");
                        ViewBag.MostrarRol = false;
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
                        ID_Rol = viewModel.ID_Rol, // Rol Cliente
                        usuario = viewModel.usuario,
                        hash_clave = _passwordHasher.HashPassword(viewModel.Password),
                        estado = 1, // Activo
                        fecha_creacion = DateTime.Now
                    };

                    db.Usuarios.Add(usuario);
                    db.SaveChanges();

                    TempData["Message"] = "¡Registro exitoso!";
                    TempData["MessageDetail"] = "Tu cuenta ha sido creada correctamente. Ahora puedes iniciar sesión con tus credenciales.";
                    TempData["MessageType"] = "success";
                    TempData["UseSweet"] = true;

                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al registrar usuario: {ex.Message}");
                    ModelState.AddModelError("", $"Ha ocurrido un error al registrar el usuario: {ex.Message}");
                }
            }

            ViewBag.MostrarRol = false;
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
            ViewBag.MostrarRol = true;
            return View(viewModel);
        }

        // POST: Usuarios/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UsuarioViewModel viewModel)
        {
            // Elimina la validación de contraseña para ediciones
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            // Valida formato de campos importantes
            ValidarCamposImportantes(viewModel);

            if (ModelState.IsValid)
            {
                try
                {
                    // Esto verifica si ya existe otro usuario con el mismo nombre de usuario
                    if (db.Usuarios.Any(u => u.usuario == viewModel.usuario && u.ID_Usuario != viewModel.ID_Usuario))
                    {
                        ModelState.AddModelError("usuario", "El nombre de usuario ya está en uso");
                        ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                        ViewBag.MostrarRol = true;
                        return View(viewModel);
                    }

                    // Esto verifica si ya existe otra persona con el mismo número de documento
                    if (db.Personas.Any(p => p.numero_documento == viewModel.numero_documento && p.ID_Persona != viewModel.ID_Persona))
                    {
                        ModelState.AddModelError("numero_documento", "Ya existe otra persona con este número de documento");
                        ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                        ViewBag.MostrarRol = true;
                        return View(viewModel);
                    }

                    // Verifica si ya existe otra persona con el mismo correo electrónico
                    if (db.Personas.Any(p => p.Correo == viewModel.Correo && p.ID_Persona != viewModel.ID_Persona))
                    {
                        ModelState.AddModelError("Correo", "El correo electrónico ya está en uso por otra persona");
                        ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                        ViewBag.MostrarRol = true;
                        return View(viewModel);
                    }

                    // Verifica si ya existe otra persona con el mismo número de teléfono
                    if (db.Personas.Any(p => p.Teléfono == viewModel.Teléfono && p.ID_Persona != viewModel.ID_Persona))
                    {
                        ModelState.AddModelError("Teléfono", "El número de teléfono ya está en uso por otra persona");
                        ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
                        ViewBag.MostrarRol = true;
                        return View(viewModel);
                    }

                    // Actualiza la persona
                    var persona = db.Personas.Find(viewModel.ID_Persona);
                    if (persona == null)
                    {
                        return HttpNotFound();
                    }

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
                    if (usuario == null)
                    {
                        return HttpNotFound();
                    }

                    usuario.ID_Rol = viewModel.ID_Rol;
                    usuario.usuario = viewModel.usuario;
                    usuario.estado = viewModel.estado;

                    db.Entry(usuario).State = EntityState.Modified;
                    db.SaveChanges();

                    TempData["Message"] = "Usuario actualizado correctamente";
                    TempData["MessageDetail"] = "Los datos del usuario han sido actualizados exitosamente.";
                    TempData["MessageType"] = "success";
                    TempData["UseSweet"] = true;

                    return RedirectToAction("Details", new { id = viewModel.ID_Usuario });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al editar usuario: {ex.Message}");
                    ModelState.AddModelError("", $"Ha ocurrido un error al actualizar el usuario: {ex.Message}");
                }
            }

            ViewBag.ID_Rol = new SelectList(db.Roles, "ID_Rol", "Nombre", viewModel.ID_Rol);
            ViewBag.MostrarRol = true;
            return View(viewModel);
        }

        // Método para validar campos importantes
        private void ValidarCamposImportantes(UsuarioViewModel viewModel)
        {
            // Valida formato de correo electrónico
            if (!string.IsNullOrEmpty(viewModel.Correo))
            {
                var emailRegex = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                if (!emailRegex.IsMatch(viewModel.Correo))
                {
                    ModelState.AddModelError("Correo", "El formato del correo electrónico no es válido. Debe seguir el formato: usuario@dominio.com");
                }
            }

            // Valida formato de teléfono
            if (!string.IsNullOrEmpty(viewModel.Teléfono))
            {
                // Limpia el teléfono de espacios, guiones, etc.
                var phoneClean = System.Text.RegularExpressions.Regex.Replace(viewModel.Teléfono, @"[\s\-\(\)]", "");

                // Verifica que el teléfono tenga solo números y posiblemente un '+' al inicio
                var phoneRegex = new System.Text.RegularExpressions.Regex(@"^\+?[0-9]{8,15}$");
                if (!phoneRegex.IsMatch(phoneClean))
                {
                    ModelState.AddModelError("Teléfono", "El formato del teléfono no es válido. Debe contener entre 8 y 15 dígitos, puede incluir el prefijo internacional con '+' al inicio.");
                }

                // Asigna el teléfono limpio al modelo
                viewModel.Teléfono = phoneClean;
            }

            // Valida número de documento
            var docString = viewModel.numero_documento.ToString();
            if (!System.Text.RegularExpressions.Regex.IsMatch(docString, @"^[0-9]{6,12}$"))
            {
                ModelState.AddModelError("numero_documento", "El número de documento debe tener entre 6 y 12 dígitos y solo puede contener números.");
            }
        }

        [AuthorizeRoles("Administrador")]
        // POST: Usuarios/ToggleEstado
        [HttpPost]
        public ActionResult ToggleEstado(int id)
        {
            try
            {
                var usuario = db.Usuarios
                    .Include(u => u.Personas)
                    .FirstOrDefault(u => u.ID_Usuario == id);

                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado" });
                }

                // Cambia el estado (0: Inactivo, 1: Activo, 2: Bloqueado)
                string oldStatus = usuario.estado == 1 ? "Activo" : "Inactivo";
                usuario.estado = usuario.estado == 1 ? 0 : 1;
                string newStatus = usuario.estado == 1 ? "Activo" : "Inactivo";

                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    estado = usuario.estado,
                    message = $"El usuario {usuario.usuario} ha cambiado de estado {oldStatus} a {newStatus}",
                    userName = $"{usuario.Personas.Nombre} {usuario.Personas.Apellido_1}"
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cambiar estado: {ex.Message}");
                return Json(new { success = false, message = "Error al cambiar el estado: " + ex.Message });
            }
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

            var viewModel = new CambiarPasswordViewModel
            {
                ID_Usuario = usuario.ID_Usuario,
                usuario = usuario.usuario,  // Me tengo que asegurar de que esto esté definido
                Nombre = usuario.Personas.Nombre,
                Apellido_1 = usuario.Personas.Apellido_1
            };

            return View(viewModel);
        }

        // POST: Usuarios/CambiarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarPassword(CambiarPasswordViewModel viewModel)
        {
            try
            {
                // Esto verifica que el modelo sea válido
                if (!ModelState.IsValid)
                {
                    // Registra los errores para depuración
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error de validación: {error.ErrorMessage}");
                        }
                    }

                    // Esto obtiene datos del usuario para la vista
                    var usuarioInfo = db.Usuarios
                        .Include(u => u.Personas)
                        .FirstOrDefault(u => u.ID_Usuario == viewModel.ID_Usuario);

                    if (usuarioInfo != null)
                    {
                        viewModel.Nombre = usuarioInfo.Personas.Nombre;
                        viewModel.Apellido_1 = usuarioInfo.Personas.Apellido_1;
                    }

                    // Añade un mensaje de error general si no hay errores específicos
                    if (!ModelState.Values.Any(v => v.Errors.Count > 0))
                    {
                        ModelState.AddModelError("", "Por favor verifica los datos ingresados");
                    }

                    return View(viewModel);
                }

                // Busca el usuario
                var usuario = db.Usuarios.Find(viewModel.ID_Usuario);
                if (usuario == null)
                {
                    ModelState.AddModelError("", "Usuario no encontrado");
                    return View(viewModel);
                }

                // Esto verifica contraseña actual (solo si no es administrador cambiando la contraseña de otro)
                bool isAdmin = Session["UserRoleID"] != null && Convert.ToInt32(Session["UserRoleID"]) == 1;
                bool isSelfChange = Session["UserID"] != null && Convert.ToInt32(Session["UserID"]) == viewModel.ID_Usuario;

                if (!isAdmin || isSelfChange)
                {
                    // Verifica la contraseña actual
                    if (_passwordHasher.VerifyHashedPassword(usuario.hash_clave, viewModel.CurrentPassword) != PasswordVerificationResult.Success)
                    {
                        ModelState.AddModelError("CurrentPassword", "La contraseña actual es incorrecta");

                        // Recarga datos para la vista
                        var usuarioDetalle = db.Usuarios
                            .Include(u => u.Personas)
                            .FirstOrDefault(u => u.ID_Usuario == viewModel.ID_Usuario);

                        if (usuarioDetalle != null)
                        {
                            viewModel.Nombre = usuarioDetalle.Personas.Nombre;
                            viewModel.Apellido_1 = usuarioDetalle.Personas.Apellido_1;
                        }

                        return View(viewModel);
                    }
                }

                // Actualiza contraseña
                usuario.hash_clave = _passwordHasher.HashPassword(viewModel.Password);
                db.Entry(usuario).State = EntityState.Modified;
                db.SaveChanges();

                TempData["Message"] = "¡Contraseña actualizada exitosamente!";
                TempData["MessageDetail"] = "Tu contraseña ha sido cambiada. Puedes usar tu nueva contraseña para acceder a partir de ahora.";
                TempData["MessageType"] = "success";
                TempData["UseSweet"] = true; // Indicador para usar SweetAlert2

                // Redirecciona a la página de perfil 
                if (isSelfChange)
                {
                    return RedirectToAction("Details", new { id = viewModel.ID_Usuario });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                // Esto captura y registra cualquier excepción para depuración
                System.Diagnostics.Debug.WriteLine($"Error en CambiarPassword: {ex.Message}");
                ModelState.AddModelError("", "Ha ocurrido un error al cambiar la contraseña: " + ex.Message);

                // Obtiene datos del usuario para la vista
                var usuarioInfo = db.Usuarios
                    .Include(u => u.Personas)
                    .FirstOrDefault(u => u.ID_Usuario == viewModel.ID_Usuario);

                if (usuarioInfo != null)
                {
                    viewModel.Nombre = usuarioInfo.Personas.Nombre;
                    viewModel.Apellido_1 = usuarioInfo.Personas.Apellido_1;
                }

                return View(viewModel);
            }
        }

        // GET: Usuarios/Login
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: Usuarios/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Primero se busca por nombre de usuario
                    var usuario = db.Usuarios
                        .Include(u => u.Personas)
                        .Include(u => u.Roles)
                        .FirstOrDefault(u => u.usuario == model.Usuario);

                    // Si no se encuentra por usuario, busca por correo
                    if (usuario == null)
                    {
                        usuario = db.Usuarios
                            .Include(u => u.Personas)
                            .Include(u => u.Roles)
                            .FirstOrDefault(u => u.Personas.Correo == model.Usuario);
                    }

                    // Verifica si el usuario existe y la contraseña es correcta
                    if (usuario == null || _passwordHasher.VerifyHashedPassword(usuario.hash_clave, model.Password) != Microsoft.AspNet.Identity.PasswordVerificationResult.Success)
                    {
                        ModelState.AddModelError("", "Las credenciales ingresadas no son válidas. Verifique su usuario/correo y contraseña.");
                        return View(model);
                    }

                    if (usuario.estado == 0) // 0: Inactivo
                    {
                        ModelState.AddModelError("", "Esta cuenta está inactiva. Contacte al administrador.");
                        return View(model);
                    }

                    if (usuario.estado == 2) // 2: Bloqueado
                    {
                        ModelState.AddModelError("", "Esta cuenta está bloqueada. Contacte al administrador.");
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

                    TempData["Message"] = "¡Bienvenido!";
                    TempData["MessageDetail"] = $"Hola {usuario.Personas.Nombre}, has iniciado sesión correctamente.";
                    TempData["MessageType"] = "success";
                    TempData["UseSweet"] = true;

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al iniciar sesión: {ex.Message}");
                    ModelState.AddModelError("", $"Ha ocurrido un error al iniciar sesión: {ex.Message}");
                }
            }
            return View(model);
        }

        // GET: Usuarios/Logout
        public ActionResult Logout()
        {
            Session.Clear();

            TempData["Message"] = "Sesión cerrada";
            TempData["MessageDetail"] = "Has cerrado la sesión correctamente.";
            TempData["MessageType"] = "success";
            TempData["UseSweet"] = true;

            return RedirectToAction("Index", "Home");
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