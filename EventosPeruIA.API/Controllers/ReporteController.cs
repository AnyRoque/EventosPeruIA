using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosPeruIA.API.Controllers;

// US-11 Panel del organizador con reportes de ventas (solo ve sus propios eventos)
// GET api/Reporte?idEvento=1&desde=2026-10-01&hasta=2026-10-31
[Route("api/[controller]")]
[Authorize(Roles = Roles.Organizador)]
public class ReporteController : BaseApiController
{
    private readonly IReporteService _reporteService;

    public ReporteController(IReporteService reporteService)
    {
        _reporteService = reporteService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerReporte([FromQuery] FiltroReporteDTO filtro)
    {
        return Responder(await _reporteService.GenerarReporte(UsuarioIdActual!.Value, filtro));
    }

    [HttpGet("exportar/excel")]
    public async Task<IActionResult> ExportarExcel([FromQuery] FiltroReporteDTO filtro)
    {
        var resultado = await _reporteService.GenerarReporte(UsuarioIdActual!.Value, filtro);
        if (!resultado.Exito || resultado.Datos is null || !resultado.Datos.HayVentas)
            return SinArchivo(resultado);

        return File(_reporteService.ExportarExcel(resultado.Datos),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ReporteVentas_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [HttpGet("exportar/pdf")]
    public async Task<IActionResult> ExportarPdf([FromQuery] FiltroReporteDTO filtro)
    {
        var resultado = await _reporteService.GenerarReporte(UsuarioIdActual!.Value, filtro);
        if (!resultado.Exito || resultado.Datos is null || !resultado.Datos.HayVentas)
            return SinArchivo(resultado);

        return File(_reporteService.ExportarPdf(resultado.Datos), "application/pdf",
            $"ReporteVentas_{DateTime.Now:yyyyMMdd}.pdf");
    }

    // Sin ventas no hay nada que exportar (en el frontend los botones están deshabilitados)
    private IActionResult SinArchivo(ResultadoDTO<ReporteResumenDTO> resultado)
    {
        return resultado.Exito
            ? StatusCode(404, new { mensaje = resultado.Mensaje, datos = (object?)null })
            : Responder(resultado);
    }
}
