﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_Final_Desarrollo_Web.ViewModels
{
    public class ReporteInventarioViewModel
    {
        public List<ReporteInventarioDetalleViewModel> DetallesInventario { get; set; }
        public DateTime FechaReporte { get; set; }
        public decimal ValorTotalInventario { get; set; }
        public int TotalMedicamentos { get; set; }
        public int TotalUnidades { get; set; }
        public int MedicamentosVencidos { get; set; }
        public int MedicamentosPorVencer { get; set; }

        public ReporteInventarioViewModel()
        {
            DetallesInventario = new List<ReporteInventarioDetalleViewModel>();
            FechaReporte = DateTime.Now;
        }
    }

    public class ReporteInventarioDetalleViewModel
    {
        public int ID_Medicamento { get; set; }

        [Display(Name = "Medicamento")]
        public string NombreMedicamento { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Laboratorio")]
        public string Laboratorio { get; set; }

        [Display(Name = "Stock Total")]
        public int StockTotal { get; set; }

        [Display(Name = "Valor Unitario")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PrecioCompra { get; set; }

        [Display(Name = "Valor Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal ValorTotal { get; set; }

        [Display(Name = "Lotes")]
        public List<LoteResumenViewModel> Lotes { get; set; }

        public ReporteInventarioDetalleViewModel()
        {
            Lotes = new List<LoteResumenViewModel>();
        }
    }

    public class LoteResumenViewModel
    {
        public int id_Lote { get; set; }

        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Fecha Vencimiento")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime FechaVencimiento { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }

        [Display(Name = "Ubicación")]
        public string Ubicacion { get; set; }

        [Display(Name = "Días para Vencer")]
        public int DiasParaVencer
        {
            get
            {
                return (int)(FechaVencimiento - DateTime.Now).TotalDays;
            }
        }
    }

    public class ReporteVentasViewModel
    {
        // Esto es para los datos del los filtros
        [Display(Name = "Fecha Inicio")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaInicio { get; set; }

        [Display(Name = "Fecha Fin")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime FechaFin { get; set; }

        // Esto es para los resultados del reporte
        public DateTime FechaReporte { get; set; }
        public int TotalVentas { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal MontoTotalVentas { get; set; }

        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PromedioVenta { get; set; }

        public List<VentaReporteViewModel> Ventas { get; set; }
        public List<VentaPorDiaViewModel> VentasPorDia { get; set; }
        public List<TopMedicamentoVentaViewModel> TopMedicamentos { get; set; }

        public ReporteVentasViewModel()
        {
            FechaReporte = DateTime.Now;
            Ventas = new List<VentaReporteViewModel>();
            VentasPorDia = new List<VentaPorDiaViewModel>();
            TopMedicamentos = new List<TopMedicamentoVentaViewModel>();

            // Por defecto, es del último mes
            FechaFin = DateTime.Now;
            FechaInicio = DateTime.Now.AddMonths(-1);
        }
    }

    public class VentaReporteViewModel
    {
        public int id_Factura { get; set; }

        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? Fecha { get; set; }

        [Display(Name = "Cliente")]
        public string Cliente { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Total { get; set; }

        [Display(Name = "Productos")]
        public int CantidadProductos { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; }
    }

    public class VentaPorDiaViewModel
    {
        [Display(Name = "Fecha")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Fecha { get; set; }

        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal Total { get; set; }
    }

    public class TopMedicamentoVentaViewModel
    {
        public int ID_Medicamento { get; set; }

        [Display(Name = "Medicamento")]
        public string Nombre { get; set; }

        [Display(Name = "Categoría")]
        public string Categoria { get; set; }

        [Display(Name = "Unidades Vendidas")]
        public int Cantidad { get; set; }

        [Display(Name = "Total Ventas")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal TotalVentas { get; set; }

        [Display(Name = "Porcentaje")]
        [DisplayFormat(DataFormatString = "{0:P2}")]
        public decimal Porcentaje { get; set; }
    }
}