using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.Models.TableViewModels;
using Proyecto_Final_Desarrollo_Web.Models.ViewModels;
using System.Data.Entity;

namespace Proyecto_Final_Desarrollo_Web.Controllers
{
	public class MedicamentoController : Controller
	{
		private readonly FarmaUEntities db = new FarmaUEntities();

		public ActionResult Index()
		{
			List<MedicamentoTableViewModel> listaMedicamentos;
			using (var db = new FarmaUEntities())
			{
				listaMedicamentos = db.Medicamentos
					.Select(a => new MedicamentoTableViewModel
					{
						ID_Medicamento = a.ID_Medicamento,
						Nombre = a.Nombre,
						principio_activo = a.principio_activo,
						NombreCategoria = a.Categorias.Nombre,
						NombreLaboratorio = a.Laboratorios.Nombre,
						precio_compra = a.precio_compra,
						precio_venta = a.precio_venta,
						StockTotal = a.Lotes.Sum(x => x.cantidad),
						estado = a.estado
					})
					.ToList();
			}
			return View(listaMedicamentos);
		}

		public ActionResult Create()
		{
			ViewBag.Categorias = new SelectList(db.Categorias, "ID_Categoria", "Nombre");
			ViewBag.Laboratorios = new SelectList(db.Laboratorios, "ID_Laboratorio", "Nombre");
			return View();
		}

		[HttpPost]
		public ActionResult Create(MedicamentoViewModel model)
		{
			if (ModelState.IsValid)
			{
				using (var db = new FarmaUEntities())
				{
					var medicamento = model.ToEntity();
					db.Medicamentos.Add(medicamento);
					db.SaveChanges();
				}
				return RedirectToAction("Index");
			}

			ViewBag.Categorias = new SelectList(db.Categorias, "ID_Categoria", "Nombre", model.ID_Categoria);
			ViewBag.Laboratorios = new SelectList(db.Laboratorios, "ID_Laboratorio", "Nombre", model.ID_Laboratorio);
			return View(model);
		}

		public ActionResult Edit(int? id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			Medicamento medicamento;
			using (var db = new FarmaUEntities())
			{
				medicamento = db.Medicamentos.Find(id);
			}

			if (medicamento == null)
				return HttpNotFound();

			var model = MedicamentoViewModel.FromEntity(medicamento);

			ViewBag.Categorias = new SelectList(db.Categorias, "ID_Categoria", "Nombre", model.ID_Categoria);
			ViewBag.Laboratorios = new SelectList(db.Laboratorios, "ID_Laboratorio", "Nombre", model.ID_Laboratorio);

			return View(model);
		}

		[HttpPost]
		public ActionResult Edit(MedicamentoViewModel model)
		{
			if (ModelState.IsValid)
			{
				using (var db = new FarmaUEntities())
				{
					var medicamento = model.ToEntity();
					db.Entry(medicamento).State = EntityState.Modified;
					db.SaveChanges();
				}
				return RedirectToAction("Index");
			}

			ViewBag.Categorias = new SelectList(db.Categorias, "ID_Categoria", "Nombre", model.ID_Categoria);
			ViewBag.Laboratorios = new SelectList(db.Laboratorios, "ID_Laboratorio", "Nombre", model.ID_Laboratorio);

			return View(model);
		}

		public ActionResult Delete(int? id)
		{
			if (id == null)
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			Medicamento medicamento;
			using (var db = new FarmaUEntities())
			{
				medicamento = db.Medicamentos.Find(id);
			}

			if (medicamento == null)
				return HttpNotFound();

			return View(MedicamentoViewModel.FromEntity(medicamento));
		}

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			using (var db = new FarmaUEntities())
			{
				var medicamento = db.Medicamentos.Find(id);
				if (medicamento != null)
				{
					db.Medicamentos.Remove(medicamento);
					db.SaveChanges();
				}
			}
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				db.Dispose();
			base.Dispose(disposing);
		}
	}
}
