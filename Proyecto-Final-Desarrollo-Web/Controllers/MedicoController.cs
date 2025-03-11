using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.TableViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
    public class MedicoController : Controller
    {
        private readonly FarmaUEntities db = new FarmaUEntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetMedicos(MedicoTableRequest request)
        {
            var medicosQuery = db.Medicos
                .Join(db.Personas, m => m.ID_Persona, p => p.ID_Persona, (m, p) => new MedicoTableViewModel
                {
                    id_Medico = m.id_Medico,
                    ID_Persona = m.ID_Persona,
                    NombreCompleto = p.Nombre + " " + p.Apellido_1 + " " + p.Apellido_2,
                    Especialidad = m.Especialidad,
                    registro_medico = m.registro_medico,
                    Documento = p.numero_documento.ToString(),
                    Telefono = p.Teléfono,
                    Correo = p.Correo,
                    Direccion = p.dirección
                });

            if (!string.IsNullOrEmpty(request.Especialidad))
            {
                medicosQuery = medicosQuery.Where(m => m.Especialidad.Contains(request.Especialidad));
            }

            if (!string.IsNullOrEmpty(request.SortColumn))
            {
                if (request.SortColumn == "NombreCompleto")
                {
                    medicosQuery = request.SortDirection == "asc" ? medicosQuery.OrderBy(m => m.NombreCompleto) : medicosQuery.OrderByDescending(m => m.NombreCompleto);
                }
                else if (request.SortColumn == "Especialidad")
                {
                    medicosQuery = request.SortDirection == "asc" ? medicosQuery.OrderBy(m => m.Especialidad) : medicosQuery.OrderByDescending(m => m.Especialidad);
                }
            }

            var totalRecords = medicosQuery.Count();
            var data = medicosQuery
                .Skip(request.Start)
                .Take(request.Length)
                .ToList();

            var response = new MedicoTableResponse
            {
                draw = request.Start / request.Length + 1,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            };

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id)
        {
            var medico = db.Medicos
                .Where(m => m.id_Medico == id)
                .Join(db.Personas, m => m.ID_Persona, p => p.ID_Persona, (m, p) => new MedicoTableViewModel
                {
                    id_Medico = m.id_Medico,
                    ID_Persona = m.ID_Persona,
                    NombreCompleto = p.Nombre + " " + p.Apellido_1 + " " + p.Apellido_2,
                    Especialidad = m.Especialidad,
                    registro_medico = m.registro_medico,
                    Documento = p.numero_documento.ToString(),
                    Telefono = p.Teléfono,
                    Correo = p.Correo,
                    Direccion = p.dirección
                })
                .FirstOrDefault();

            if (medico == null)
            {
                return HttpNotFound();
            }

            return View(medico);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MedicoTableViewModel model)
        {
            if (ModelState.IsValid)
            {
                var persona = new Personas
                {
                    Nombre = model.NombreCompleto.Split(' ')[0],
                    Apellido_1 = model.NombreCompleto.Split(' ').Length > 1 ? model.NombreCompleto.Split(' ')[1] : "",
                    Apellido_2 = model.NombreCompleto.Split(' ').Length > 2 ? model.NombreCompleto.Split(' ')[2] : "",
                    tipo_documento = "DNI", 
                    numero_documento = Convert.ToInt64(model.Documento),
                    Correo = model.Correo,
                    Teléfono = model.Telefono,
                    dirección = model.Direccion
                };

                db.Personas.Add(persona);
                db.SaveChanges();

                var medico = new Medicos
                {
                    ID_Persona = persona.ID_Persona,
                    Especialidad = model.Especialidad,
                    registro_medico = model.registro_medico
                };

                db.Medicos.Add(medico);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var medico = db.Medicos
                .Where(m => m.id_Medico == id)
                .Join(db.Personas, m => m.ID_Persona, p => p.ID_Persona, (m, p) => new MedicoTableViewModel
                {
                    id_Medico = m.id_Medico,
                    ID_Persona = m.ID_Persona,
                    NombreCompleto = p.Nombre + " " + p.Apellido_1 + " " + p.Apellido_2,
                    Especialidad = m.Especialidad,
                    registro_medico = m.registro_medico,
                    Documento = p.numero_documento.ToString(),
                    Telefono = p.Teléfono,
                    Correo = p.Correo,
                    Direccion = p.dirección
                })
                .FirstOrDefault();

            if (medico == null)
            {
                return HttpNotFound();
            }

            return View(medico);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MedicoTableViewModel model)
        {
            if (ModelState.IsValid)
            {
                var medico = db.Medicos.FirstOrDefault(m => m.id_Medico == model.id_Medico);
                var persona = db.Personas.FirstOrDefault(p => p.ID_Persona == model.ID_Persona);

                if (medico != null && persona != null)
                {
                    persona.Nombre = model.NombreCompleto.Split(' ')[0];
                    persona.Apellido_1 = model.NombreCompleto.Split(' ').Length > 1 ? model.NombreCompleto.Split(' ')[1] : "";
                    persona.Apellido_2 = model.NombreCompleto.Split(' ').Length > 2 ? model.NombreCompleto.Split(' ')[2] : "";
                    persona.numero_documento = Convert.ToInt64(model.Documento);
                    persona.Correo = model.Correo;
                    persona.Teléfono = model.Telefono;
                    persona.dirección = model.Direccion;

                    medico.Especialidad = model.Especialidad;
                    medico.registro_medico = model.registro_medico;

                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var medico = db.Medicos.FirstOrDefault(m => m.id_Medico == id);
            if (medico == null)
            {
                return HttpNotFound();
            }

            db.Medicos.Remove(medico);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
