using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces;

public interface IReporteService
{
    Task<ResultadoDTO<ReporteResumenDTO>> GenerarReporte(int idOrganizador, FiltroReporteDTO filtro);
    Task<byte[]> ExportarExcel(ReporteResumenDTO reporte);
    Task<byte[]> ExportarPdf(ReporteResumenDTO reporte);
}
