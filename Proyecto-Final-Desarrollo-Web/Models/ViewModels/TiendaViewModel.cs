using Proyecto_Final_Desarrollo_Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    // ViewModel principal para la Tienda
    public class TiendaViewModel
    {
        public List<ProductoTiendaViewModel> Productos { get; set; }
        public List<CategoriaViewModel> Categorias { get; set; }
        public FiltroTiendaViewModel Filtros { get; set; }
        public List<ProductoTiendaViewModel> ProductosDestacados { get; set; }
        public List<ProductoTiendaViewModel> Ofertas { get; set; }
        public List<ProductoTiendaViewModel> NuevosProductos { get; set; }
        public string TituloPagina { get; set; }
        public string DescripcionPagina { get; set; }
        public int TotalProductos { get; set; }
        public int PaginaActual { get; set; }
        public int ProductosPorPagina { get; set; }
        public int TotalPaginas { get; set; }

        public TiendaViewModel()
        {
            Productos = new List<ProductoTiendaViewModel>();
            Categorias = new List<CategoriaViewModel>();
            ProductosDestacados = new List<ProductoTiendaViewModel>();
            Ofertas = new List<ProductoTiendaViewModel>();
            NuevosProductos = new List<ProductoTiendaViewModel>();
            Filtros = new FiltroTiendaViewModel();
            PaginaActual = 1;
            ProductosPorPagina = 12;
        }
    }

    // ViewModel para representar un producto en la tienda
    public class ProductoTiendaViewModel
    {
        public int ID_Producto { get; set; }

        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Display(Name = "Precio")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Precio { get; set; }

        [Display(Name = "Precio Original")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PrecioOriginal { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        public int ID_Categoria { get; set; }

        [Display(Name = "Marca")]
        public string Marca { get; set; }

        [Display(Name = "Disponibilidad")]
        public int Stock { get; set; }

        [Display(Name = "Imagen Principal")]
        public string UrlImagen { get; set; }

        [Display(Name = "Descuento")]
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public decimal PorcentajeDescuento { get; set; }

        public bool EsNuevo { get; set; }

        public bool EsOferta { get; set; }

        public bool EsDestacado { get; set; }

        [Display(Name = "Valoración")]
        public decimal Valoracion { get; set; }

        [Display(Name = "Ventas")]
        public int NumeroVentas { get; set; }

        [Display(Name = "Contenido")]
        public string ContenidoNeto { get; set; }

        // Método para convertir de Productos (entidad) a ProductoTiendaViewModel
        public static ProductoTiendaViewModel FromEntity(Productos producto, decimal descuento = 0)
        {
            // Obtiene la imagen principal
            var imagenPrincipal = producto.Imagenes_Producto
                                   ?.Where(i => i.Estado && i.EsPrincipal)
                                   ?.FirstOrDefault();

            // Calcula si hay descuento
            decimal precioOriginal = producto.precio_venta;
            decimal precioVenta = precioOriginal;
            decimal porcentajeDescuento = descuento;

            if (descuento > 0)
            {
                precioVenta = precioOriginal * (1 - descuento);
            }

            // Stock disponible (suma de todos los lotes)
            int stockDisponible = producto.Lotes?.Sum(l => l.cantidad) ?? 0;

            // Determina si es producto nuevo (últimos 15 días)
            // Como no hay fecha_creacion directamente, usamos la fecha de los lotes como aproximación
            bool esNuevo = producto.Lotes != null &&
                           producto.Lotes.Any(l => l.Inventario.Any() &&
                                                  (DateTime.Now - DateTime.Now.AddDays(-15)).TotalDays <= 15);

            return new ProductoTiendaViewModel
            {
                ID_Producto = producto.ID_Producto,
                Nombre = producto.Nombre,
                Precio = precioVenta,
                PrecioOriginal = precioOriginal,
                Descripcion = producto.Detalles,
                Categoria = producto.Categorias?.Nombre,
                ID_Categoria = producto.ID_Categoría,
                Marca = producto.Marca,
                Stock = stockDisponible,
                UrlImagen = imagenPrincipal?.URL ?? "/Content/images/product-placeholder.jpg",
                PorcentajeDescuento = porcentajeDescuento,
                EsNuevo = esNuevo,
                EsOferta = descuento > 0,
                // Como no existe la propiedad destacado, usamos un valor por defecto falso o alguna lógica alternativa
                EsDestacado = false,
                Valoracion = 4.5M, // Valor predeterminado, en un sistema real se obtendría de las reseñas
                NumeroVentas = producto.Detalles_Factura?.Sum(df => df.cantidad) ?? 0,
                ContenidoNeto = producto.ContenidoNeto
            };
        }
    }

    // ViewModel para categorías en la tienda
    public class CategoriaViewModel
    {
        public int ID_Categoria { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int CantidadProductos { get; set; }
        public string UrlImagen { get; set; }

        // Método para convertir de Categorias (entidad) a CategoriaViewModel
        public static CategoriaViewModel FromEntity(Categorias categoria)
        {
            return new CategoriaViewModel
            {
                ID_Categoria = categoria.ID_Categoría,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                CantidadProductos = categoria.Productos?.Count(p => p.estado == "Activo") ?? 0,
                UrlImagen = "/Content/images/categoria-" + categoria.ID_Categoría + ".jpg" // Imagen genérica basada en ID
            };
        }
    }

    // ViewModel para filtros de búsqueda en la tienda
    public class FiltroTiendaViewModel
    {
        // Búsqueda por texto
        public string Busqueda { get; set; }

        // Filtro por categoría
        public int? CategoriaId { get; set; }

        // Rango de precios
        public decimal? PrecioMinimo { get; set; }
        public decimal? PrecioMaximo { get; set; }

        // Filtro por marcas
        public string[] Marcas { get; set; }

        // Filtro por tipos de producto (para checkboxes)
        public string[] TipoProducto { get; set; }

        // Opciones de ordenamiento
        public string OrdenarPor { get; set; } // precio_asc, precio_desc, nombre_asc, nombre_desc, populares, nuevos

        // Filtros especiales
        public bool SoloOfertas { get; set; }
        public bool SoloNuevos { get; set; }
        public bool SoloEnStock { get; set; }

        // Filtros específicos de farmacia
        public bool RequiereReceta { get; set; }
        public bool GenericoDisponible { get; set; }

        public FiltroTiendaViewModel()
        {
            // Valores por defecto
            OrdenarPor = "populares";
            Marcas = new string[0];
            TipoProducto = new string[0];
            SoloOfertas = false;
            SoloNuevos = false;
            SoloEnStock = false;
            RequiereReceta = false;
            GenericoDisponible = false;
        }
    }
}