using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs;

// US-11 Panel del organizador con reportes de ventas

// Filtros: evento y rango de fechas de compra (desde – hasta)
public class FiltroReporteDTO
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un evento válido.")]
    public int? IdEvento { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

public class ReporteResumenDTO
{
    public bool HayVentas { get; set; }                 // false: "No hay ventas en el periodo seleccionado" y exportar deshabilitado

    // Tarjetas de resumen (también son la fila de totales de cada tabla)
    public decimal TotalRecaudado { get; set; }         // S/, sin eventos cancelados / reembolsados
    public int EntradasVendidas { get; set; }
    public int AforoTotal { get; set; }
    public decimal PorcentajeAforoOcupado { get; set; }
    public int AsistentesIngresaron { get; set; }       // entradas con check-in

    public List<ReporteEventoDTO> VentasPorEvento { get; set; } = new();
    public List<ReporteDiaDTO> VentasPorDia { get; set; } = new();          // para el gráfico de barras
    public List<ReporteVentaDetalleDTO> DetalleVentas { get; set; } = new();
}

public class ReporteEventoDTO
{
    public int IdEvento { get; set; }
    public string Evento { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public int Aforo { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal PorcentajeOcupacion { get; set; }
    public decimal MontoRecaudado { get; set; }
    public string Estado { get; set; } = string.Empty;  // "Cancelado / Reembolsado" si el evento se canceló
    public bool SumaAlTotal { get; set; }               // false en eventos cancelados
}

public class ReporteDiaDTO
{
    public DateTime Fecha { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal Monto { get; set; }
}

public class ReporteVentaDetalleDTO
{
    public string NumeroCompra { get; set; } = string.Empty;
    public string Comprador { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaCompra { get; set; }
    public string Estado { get; set; } = string.Empty;  // "Pagada" o "Cancelado / Reembolsado"
    public bool SumaAlTotal { get; set; }
}
