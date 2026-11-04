using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventosPeruIA.API.Controllers;

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
        if (!resultado.Exito || resultado.Datos is null)
            return Responder(resultado);

        var archivo = await _reporteService.ExportarExcel(resultado.Datos);
        return File(archivo,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "ReporteVentas.xlsx");
    }

    [HttpGet("exportar/pdf")]
    public async Task<IActionResult> ExportarPdf([FromQuery] FiltroReporteDTO filtro)
    {
        var resultado = await _reporteService.GenerarReporte(UsuarioIdActual!.Value, filtro);
        if (!resultado.Exito || resultado.Datos is null)
            return Responder(resultado);

        var archivo = await _reporteService.ExportarPdf(resultado.Datos);
        return File(archivo, "application/pdf", "ReporteVentas.pdf");
    }
}
