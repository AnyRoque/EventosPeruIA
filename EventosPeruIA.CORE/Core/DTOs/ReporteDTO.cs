using System;
using System.Collections.Generic;
using System.Text;

namespace EventosPeruIA.CORE.Core.DTOs
{
    // Métricas generales del dashboard del organizador
    public class ReporteOrganizadorDTO
    {
        public decimal TotalRecaudado { get; set; }
        public int TotalEntradasVendidas { get; set; }
        public int TotalEventosPublicados { get; set; }
        public decimal PorcentajeAsistenciaGlobal { get; set; }
        public List<ReporteEventoResumenDTO> Eventos { get; set; } = new();
    }

    public class ReporteEventoResumenDTO
    {
        public int IdEvento { get; set; }
        public string Nombre { get; set; } = null!;
        public DateTime FechaHoraInicio { get; set; }
        public string Estado { get; set; } = null!;
        public int AforoMaximo { get; set; }
        public int EntradasVendidas { get; set; }
        public decimal TotalRecaudado { get; set; }
    }

    // Reporte detallado de un evento específico
    public class ReporteEventoDetalleDTO
    {
        public int IdEvento { get; set; }
        public string NombreEvento { get; set; } = null!;
        public int AforoMaximo { get; set; }
        public int EntradasVendidas { get; set; }
        public int EntradasAsistieron { get; set; } // Entradas con Check-In
        public decimal PorcentajeAsistencia { get; set; }
        public decimal TotalRecaudado { get; set; }
        public List<VentaDetalleDTO> Ventas { get; set; } = new();
    }

    public class VentaDetalleDTO
    {
        public int IdCompra { get; set; }
        public string NumeroCompra { get; set; } = null!;
        public string Comprador { get; set; } = null!;
        public string CorreoComprador { get; set; } = null!;
        public int CantidadEntradas { get; set; }
        public decimal TotalPagado { get; set; }
        public DateTime FechaCompra { get; set; }
    }
}