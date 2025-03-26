using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Razor.Generator;
using Productos;

public class ProductosController : Controller
{
    // Simulación de la base de datos, es que aun no tengo conexion con la misma.
    private List<Medicamento> medicamentos = new List<Medicamento>

    {
        new Medicamento { Id = 1, Nombre = "Paracetamol", Descripcion = "Analgésico", Precio = 10.50m, ImagenUrl ="/Content/img/Paracetamol.webp"},
        new Medicamento { Id = 2, Nombre = "Ibuprofeno", Descripcion = "Antiinflamatorio", Precio = 12.00m, ImagenUrl = "Content/img/Ibuprofeno.jpg" },
        new Medicamento { Id = 3, Nombre = "Amoxicilina", Descripcion = "Antibiótico", Precio = 15.00m, ImagenUrl= "Content/img/images.jpg" }
    };

    public ActionResult Medicamentos()
    {
        return View(medicamentos);
    }

    private List<CuidadoPersonal> cuidadoPersonal = new List<CuidadoPersonal>
    {
        new CuidadoPersonal { Id = 1, Nombre = "Jabón", Descripcion = "Jabón para el cuerpo", Precio = 5.00m },
        new CuidadoPersonal { Id = 2, Nombre = "Shampoo", Descripcion = "Shampoo para cabello", Precio = 8.00m },
        new CuidadoPersonal { Id = 3, Nombre = "Crema Hidratante", Descripcion = "Crema para la piel", Precio = 20.00m }
    };
    public ActionResult CuidadoPersonal()
    {
        return View(cuidadoPersonal);
    }
    public ActionResult Vitaminas()
    {
        // Simulación de datos para vitaminas
        var vitaminas = new List<Vitaminas>
    {
        new Vitaminas { Id = 1, Nombre = "Vitamina C", Descripcion = "Suplemento de Vitamina C", Precio = 10.00m,},
        new Vitaminas { Id = 2, Nombre = "Multivitamínico", Descripcion = "Suplemento multivitamínico", Precio = 15.00m },
        new Vitaminas { Id = 3, Nombre = "Vitamina D", Descripcion = "Suplemento de Vitamina D", Precio = 12.50m }
    };
        return View(vitaminas);
    }

    public ActionResult Bebes()
    {
        // Simulación de datos para productos de bebés
        var bebes = new List<Bebes>
    {
        new Bebes { Id = 1, Nombre = "Pañales", Descripcion = "Pañales desechables", Precio = 25.00m },
        new Bebes { Id = 2, Nombre = "Toallitas Húmedas", Descripcion = "Toallitas para bebés", Precio = 10.00m },
        new Bebes { Id = 3, Nombre = "Leche en Polvo", Descripcion = "Leche en polvo para bebés", Precio = 50.00m }
    };
        return View(bebes); // Devuelve la vista con la lista de productos para bebés
    }
}




namespace Productos

{
    public class Medicamento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; } // propiedad para la imagen

    }


    public class CuidadoPersonal
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
    }

    public class Vitaminas
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
    }


    public class Bebes
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
    }
}





