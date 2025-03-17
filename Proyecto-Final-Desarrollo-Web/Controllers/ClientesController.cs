using System.Collections.Generic;
using System;
using System.Linq;
using System.Web.Mvc;
using Proyecto_Final_Desarrollo_Web.Models;
using Proyecto_Final_Desarrollo_Web.ViewModels;

public class ClientesController : Controller
{
    // Datos ficticios de clientes
    private readonly List<Clientes> clientes = new List<Clientes>
    {
        new Clientes
        {
            id_cliente = 1,
            ID_Persona = 101,
            Personas = new Personas
            {
                ID_Persona = 101,
                Nombre = "Juan",
                Apellido_1 = "Pérez",
                Apellido_2 = "González",
                tipo_documento = "DNI",
                numero_documento = 12345678,
                Correo = "juan.perez@example.com",
                Teléfono = "123456789",
                dirección = "Calle Ficticia 123"
            },
            Facturas = new List<Facturas>
            {
                new Facturas
                {
                    id_Factura = 1,
                    fecha = DateTime.Now.AddMonths(-1),
                    total = 100.50m,
                    estado = "Completada",
                    Detalles_Factura = new List<Detalles_Factura>
                    {
                        new Detalles_Factura { id_Factura = 1, cantidad = 2, ID_Detalle_Factura = 15 }
                    }
                }
            }
        },
        new Clientes
        {
            id_cliente = 2,
            ID_Persona = 102,
            Personas = new Personas
            {
                ID_Persona = 102,
                Nombre = "Ana",
                Apellido_1 = "Martínez",
                Apellido_2 = "Rodríguez",
                tipo_documento = "DNI",
                numero_documento = 87654321,
                Correo = "ana.martinez@example.com",
                Teléfono = "987654321",
                dirección = "Avenida Ficticia 456"
            },
            Facturas = new List<Facturas>
            {
                new Facturas
                {
                    id_Factura = 2,
                    fecha = DateTime.Now.AddMonths(-2),
                    total = 250.75m,
                    estado = "Completada",
                    Detalles_Factura = new List<Detalles_Factura>
                    {
                        new Detalles_Factura { id_Factura = 2, cantidad = 3, ID_Detalle_Factura = 20 }
                    }
                }
            }
        }
    };

    // Acción para la vista que muestra la lista de clientes
    public ActionResult Clientes()
    {
        // Convertimos los clientes a ViewModel para pasarlos a la vista
        var clientesViewModel = clientes.Select(c => ClienteViewModel.FromEntity(c)).ToList();
        return View("~/Views/Cliente/Clientes.cshtml", clientesViewModel); // Asegúrate de que la ruta de la vista sea correcta
    }

    // Acción para la vista que muestra los detalles de un cliente específico
    public ActionResult Detalles(int id)
    {
        // Buscar el cliente por su id en los datos ficticios
        var cliente = clientes.FirstOrDefault(c => c.id_cliente == id);

        if (cliente == null)
        {
            return HttpNotFound(); // Si no se encuentra el cliente, retornamos 404
        }

        // Convertir el cliente a un ViewModel para la vista
        var clienteViewModel = ClienteViewModel.FromEntity(cliente);

        return View("~/Views/Cliente/Detalles.cshtml", clienteViewModel);  // Pasamos el cliente seleccionado a la vista de detalles
    }
}
