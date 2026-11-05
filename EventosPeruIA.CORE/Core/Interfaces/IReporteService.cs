using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces;

public interface IReporteService
{
    Task<ResultadoDTO<ReporteResumenDTO>> GenerarReporte(int idOrganizador, FiltroReporteDTO filtro);
    byte[] ExportarExcel(ReporteResumenDTO reporte);
    byte[] ExportarPdf(ReporteResumenDTO reporte);
}
