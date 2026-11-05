using ClosedXML.Excel;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace EventosPeruIA.CORE.Core.Services;

public class ReporteService : IReporteService
{
    private readonly EventosPeruIAContext _dbContext;

    public ReporteService(EventosPeruIAContext dbContext)
    {
        _dbContext = dbContext;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ResultadoDTO<ReporteResumenDTO>> GenerarReporte(
        int idOrganizador, FiltroReporteDTO filtro)
    {
        if (filtro.Desde.HasValue && filtro.Hasta.HasValue && filtro.Desde > filtro.Hasta)
            return ResultadoDTO<ReporteResumenDTO>.Error(400,
                "La fecha inicial no puede ser posterior a la fecha final.");

        var queryEventos = _dbContext.Evento
            .AsNoTracking()
            .Where(e => e.IdOrganizador == idOrganizador);

        if (filtro.IdEvento.HasValue)
            queryEventos = queryEventos.Where(e => e.IdEvento == filtro.IdEvento.Value);

        var eventos = await queryEventos.ToListAsync();
        if (eventos.Count == 0)
            return ResultadoDTO<ReporteResumenDTO>.Error(404,
                "No se encontraron eventos para los filtros seleccionados.");

        var eventoIds = eventos.Select(e => e.IdEvento).ToList();
        var queryCompras = _dbContext.Compra
            .AsNoTracking()
            .Include(c => c.Usuario)
            .Include(c => c.DetalleCompra)
                .ThenInclude(d => d.Evento)
            .Where(c => c.DetalleCompra.Any(d => eventoIds.Contains(d.IdEvento)));

        if (filtro.Desde.HasValue)
            queryCompras = queryCompras.Where(c => c.FechaCompra >= filtro.Desde.Value.Date);

        if (filtro.Hasta.HasValue)
        {
            var hastaExclusivo = filtro.Hasta.Value.Date.AddDays(1);
            queryCompras = queryCompras.Where(c => c.FechaCompra < hastaExclusivo);
        }

        var compras = await queryCompras.ToListAsync();
        if (compras.Count == 0)
            return ResultadoDTO<ReporteResumenDTO>.Error(404,
                "No hay ventas en el periodo seleccionado.");

        var ventasValidas = compras.Where(c => c.Estado != EstadosCompra.Rechazada).ToList();
        var entradasVendidas = ventasValidas
            .SelectMany(c => c.DetalleCompra)
            .Where(d => eventoIds.Contains(d.IdEvento))
            .Sum(d => d.Cantidad);
        var aforoTotal = eventos.Sum(e => e.AforoMaximo);
        var montoTotal = compras
            .Where(c => c.Estado != EstadosCompra.Reembolsada && c.Estado != EstadosCompra.Rechazada)
            .Sum(c => c.MontoTotal);

        var reportePorDia = compras
            .GroupBy(c => c.FechaCompra.Date)
            .Select(g => new ReporteDiaDTO
            {
                Fecha = g.Key,
                EntradasVendidas = g.Where(c => c.Estado != EstadosCompra.Rechazada)
                    .SelectMany(c => c.DetalleCompra)
                    .Where(d => eventoIds.Contains(d.IdEvento))
                    .Sum(d => d.Cantidad),
                MontoTotal = g.Where(c => c.Estado != EstadosCompra.Reembolsada &&
                                          c.Estado != EstadosCompra.Rechazada)
                    .Sum(c => c.MontoTotal)
            })
            .OrderBy(d => d.Fecha)
            .ToList();

        var detalleVentas = compras
            .Select(c => new ReporteVentaDetalleDTO
            {
                IdCompra = c.IdCompra,
                Evento = string.Join(", ", c.DetalleCompra
                    .Where(d => eventoIds.Contains(d.IdEvento))
                    .Select(d => d.Evento.Nombre)
                    .Distinct()),
                FechaCompra = c.FechaCompra,
                Comprador = $"{c.Usuario.Nombre} {c.Usuario.Apellido}".Trim(),
                CantidadEntradas = c.DetalleCompra
                    .Where(d => eventoIds.Contains(d.IdEvento))
                    .Sum(d => d.Cantidad),
                MontoTotal = c.MontoTotal,
                Estado = c.Estado == EstadosCompra.Reembolsada || c.Estado == EstadosCompra.Rechazada
                    ? "Cancelado / Reembolsado"
                    : c.Estado
            })
            .OrderByDescending(v => v.FechaCompra)
            .ToList();

        var reporte = new ReporteResumenDTO
        {
            AforoTotal = aforoTotal,
            EntradasVendidas = entradasVendidas,
            PorcentajeAforoOcupado = aforoTotal > 0
                ? Math.Round((decimal)entradasVendidas / aforoTotal * 100, 2)
                : 0,
            MontoTotalRecaudado = montoTotal,
            VentasPorDia = reportePorDia,
            DetalleVentas = detalleVentas
        };

        return ResultadoDTO<ReporteResumenDTO>.Ok(reporte, "Reporte generado correctamente.");
    }

    public Task<byte[]> ExportarExcel(ReporteResumenDTO reporte)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Reporte de Ventas");

        worksheet.Cell(1, 1).Value = "Aforo Total";
        worksheet.Cell(1, 2).Value = reporte.AforoTotal;
        worksheet.Cell(2, 1).Value = "Entradas Vendidas";
        worksheet.Cell(2, 2).Value = reporte.EntradasVendidas;
        worksheet.Cell(3, 1).Value = "% Aforo Ocupado";
        worksheet.Cell(3, 2).Value = reporte.PorcentajeAforoOcupado / 100;
        worksheet.Cell(3, 2).Style.NumberFormat.Format = "0.00%";
        worksheet.Cell(4, 1).Value = "Monto Recaudado";
        worksheet.Cell(4, 2).Value = reporte.MontoTotalRecaudado;
        worksheet.Cell(4, 2).Style.NumberFormat.Format = "S/ #,##0.00";

        var headers = new[] { "ID Compra", "Evento", "Fecha", "Comprador", "Entradas", "Monto (S/)", "Estado" };
        for (var i = 0; i < headers.Length; i++)
            worksheet.Cell(6, i + 1).Value = headers[i];

        var headerRange = worksheet.Range(6, 1, 6, headers.Length);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

        var row = 7;
        foreach (var item in reporte.DetalleVentas)
        {
            worksheet.Cell(row, 1).Value = item.IdCompra;
            worksheet.Cell(row, 2).Value = item.Evento;
            worksheet.Cell(row, 3).Value = item.FechaCompra;
            worksheet.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            worksheet.Cell(row, 4).Value = item.Comprador;
            worksheet.Cell(row, 5).Value = item.CantidadEntradas;
            worksheet.Cell(row, 6).Value = item.MontoTotal;
            worksheet.Cell(row, 6).Style.NumberFormat.Format = "S/ #,##0.00";
            worksheet.Cell(row, 7).Value = item.Estado;
            row++;
        }

        worksheet.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> ExportarPdf(ReporteResumenDTO reporte)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text("Reporte de Ventas y Aforo - EventosPerúIA")
                    .FontSize(16).SemiBold();
                page.Content().Column(col =>
                {
                    col.Item().Text($"Aforo ocupado: {reporte.PorcentajeAforoOcupado}% " +
                        $"({reporte.EntradasVendidas}/{reporte.AforoTotal})");
                    col.Item().Text($"Recaudación total: S/ {reporte.MontoTotalRecaudado:F2}");
                    col.Item().PaddingVertical(10);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });
                        table.Header(header =>
                        {
                            header.Cell().Text("Evento").Bold();
                            header.Cell().Text("Comprador").Bold();
                            header.Cell().Text("Monto").Bold();
                            header.Cell().Text("Estado").Bold();
                        });
                        foreach (var item in reporte.DetalleVentas)
                        {
                            table.Cell().Text(item.Evento);
                            table.Cell().Text(item.Comprador);
                            table.Cell().Text($"S/ {item.MontoTotal:F2}");
                            table.Cell().Text(item.Estado);
                        }
                    });
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }
}
