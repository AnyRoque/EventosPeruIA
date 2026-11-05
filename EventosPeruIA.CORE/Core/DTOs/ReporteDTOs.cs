namespace EventosPeruIA.CORE.Core.DTOs;

public class FiltroReporteDTO
{
    public int? IdEvento { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

public class ReporteResumenDTO
{
    public int AforoTotal { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal PorcentajeAforoOcupado { get; set; }
    public decimal MontoTotalRecaudado { get; set; }
    public List<ReporteDiaDTO> VentasPorDia { get; set; } = new();
    public List<ReporteVentaDetalleDTO> DetalleVentas { get; set; } = new();
}

public class ReporteDiaDTO
{
    public DateTime Fecha { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal MontoTotal { get; set; }
}

public class ReporteVentaDetalleDTO
{
    public int IdCompra { get; set; }
    public string Evento { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; }
    public string Comprador { get; set; } = string.Empty;
    public int CantidadEntradas { get; set; }
    public decimal MontoTotal { get; set; }
    public string Estado { get; set; } = string.Empty;
}
