using ClosedXML.Excel;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace EventosPeruIA.CORE.Core.Services;

// US-11 Panel del organizador con reportes de ventas
public class ReporteService : IReporteService
{
    public const string SinVentas = "No hay ventas en el periodo seleccionado.";
    private const string EtiquetaCancelado = "Cancelado / Reembolsado";

    private readonly IReporteRepository _reporteRepository;

    public ReporteService(IReporteRepository reporteRepository)
    {
        _reporteRepository = reporteRepository;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ResultadoDTO<ReporteResumenDTO>> GenerarReporte(int idOrganizador, FiltroReporteDTO filtro)
    {
        // Criterio 3: "hasta" no puede ser anterior a "desde"
        if (filtro.Desde.HasValue && filtro.Hasta.HasValue && filtro.Hasta.Value.Date < filtro.Desde.Value.Date)
            return ResultadoDTO<ReporteResumenDTO>.Error(400, "La fecha «hasta» no puede ser anterior a la fecha «desde».");

        // Criterio 1: solo sus propios eventos
        var eventos = (await _reporteRepository.GetEventosOrganizador(idOrganizador, filtro.IdEvento)).ToList();
        if (filtro.IdEvento.HasValue && eventos.Count == 0)
            return ResultadoDTO<ReporteResumenDTO>.Error(404, "El evento no existe o no te pertenece.");

        var ventas = (await _reporteRepository.GetVentas(idOrganizador, filtro)).ToList();

        // Criterio 10
        if (ventas.Count == 0)
            return ResultadoDTO<ReporteResumenDTO>.Ok(new ReporteResumenDTO { HayVentas = false }, SinVentas);

        // Criterio 9: lo cancelado / reembolsado se muestra, pero no suma al total recaudado
        var validas = ventas.Where(SumaAlTotal).ToList();
        var eventosVigentes = eventos.Where(e => e.Estado != EstadosEvento.Cancelado).ToList();
        var aforoTotal = eventosVigentes.Sum(e => e.AforoMaximo);
        var entradasVendidas = validas.Sum(d => d.Cantidad);

        var reporte = new ReporteResumenDTO
        {
            HayVentas = true,

            // Criterio 2: tarjetas de resumen
            TotalRecaudado = validas.Sum(d => d.Subtotal),
            EntradasVendidas = entradasVendidas,
            AforoTotal = aforoTotal,
            PorcentajeAforoOcupado = Porcentaje(entradasVendidas, aforoTotal),
            AsistentesIngresaron = validas.SelectMany(d => d.Entrada).Count(en => en.Estado == EstadosEntrada.Utilizada),

            // Criterio 4: ventas por evento
            VentasPorEvento = eventos.Select(e =>
            {
                var delEvento = ventas.Where(d => d.IdEvento == e.IdEvento).ToList();
                var vendidas = delEvento.Sum(d => d.Cantidad);
                var cancelado = e.Estado == EstadosEvento.Cancelado;
                return new ReporteEventoDTO
                {
                    IdEvento = e.IdEvento,
                    Evento = e.Nombre,
                    Fecha = e.FechaHoraInicio,
                    Aforo = e.AforoMaximo,
                    EntradasVendidas = vendidas,
                    PorcentajeOcupacion = Porcentaje(vendidas, e.AforoMaximo),
                    MontoRecaudado = delEvento.Sum(d => d.Subtotal),
                    Estado = cancelado ? EtiquetaCancelado : e.Estado,
                    SumaAlTotal = !cancelado
                };
            }).ToList(),

            // Criterio 5: ventas por fecha (gráfico de barras)
            VentasPorDia = validas
                .GroupBy(d => d.Compra.FechaCompra.Date)
                .OrderBy(g => g.Key)
                .Select(g => new ReporteDiaDTO
                {
                    Fecha = g.Key,
                    EntradasVendidas = g.Sum(d => d.Cantidad),
                    Monto = g.Sum(d => d.Subtotal)
                }).ToList(),

            // Criterio 6: detalle de ventas
            DetalleVentas = ventas.Select(d => new ReporteVentaDetalleDTO
            {
                NumeroCompra = $"EP-{d.IdCompra:D6}",
                Comprador = $"{d.Compra.Usuario.Nombre} {d.Compra.Usuario.Apellido}",
                Evento = d.Evento.Nombre,
                Cantidad = d.Cantidad,
                Total = d.Subtotal,
                FechaCompra = d.Compra.FechaCompra,
                Estado = SumaAlTotal(d) ? d.Compra.Estado : EtiquetaCancelado,
                SumaAlTotal = SumaAlTotal(d)
            }).ToList()
        };

        return ResultadoDTO<ReporteResumenDTO>.Ok(reporte, "Reporte generado correctamente.");
    }

    // ===================== Criterio 8: exportar según los filtros aplicados =====================

    public byte[] ExportarExcel(ReporteResumenDTO r)
    {
        using var libro = new XLWorkbook();

        var resumen = libro.Worksheets.Add("Resumen");
        resumen.Cell(1, 1).Value = "Reporte de ventas - EventosPerúIA";
        resumen.Cell(1, 1).Style.Font.Bold = true;
        resumen.Cell(3, 1).Value = "Total recaudado";
        resumen.Cell(3, 2).Value = r.TotalRecaudado;
        resumen.Cell(3, 2).Style.NumberFormat.Format = FormatoSoles;
        resumen.Cell(4, 1).Value = "Entradas vendidas";
        resumen.Cell(4, 2).Value = r.EntradasVendidas;
        resumen.Cell(5, 1).Value = "% de aforo ocupado";
        resumen.Cell(5, 2).Value = r.PorcentajeAforoOcupado / 100;
        resumen.Cell(5, 2).Style.NumberFormat.Format = "0.00%";
        resumen.Cell(6, 1).Value = "Asistentes que ingresaron";
        resumen.Cell(6, 2).Value = r.AsistentesIngresaron;
        resumen.Columns().AdjustToContents();

        // Por evento
        var hoja = libro.Worksheets.Add("Por evento");
        Encabezado(hoja, "Evento", "Fecha", "Aforo", "Entradas vendidas", "% ocupación", "Monto (S/)", "Estado");
        var fila = 2;
        foreach (var e in r.VentasPorEvento)
        {
            hoja.Cell(fila, 1).Value = e.Evento;
            hoja.Cell(fila, 2).Value = e.Fecha;
            hoja.Cell(fila, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            hoja.Cell(fila, 3).Value = e.Aforo;
            hoja.Cell(fila, 4).Value = e.EntradasVendidas;
            hoja.Cell(fila, 5).Value = e.PorcentajeOcupacion / 100;
            hoja.Cell(fila, 5).Style.NumberFormat.Format = "0.00%";
            hoja.Cell(fila, 6).Value = e.MontoRecaudado;
            hoja.Cell(fila, 6).Style.NumberFormat.Format = FormatoSoles;
            hoja.Cell(fila, 7).Value = e.Estado;
            fila++;
        }
        hoja.Cell(fila, 1).Value = "TOTAL";
        hoja.Cell(fila, 3).Value = r.AforoTotal;
        hoja.Cell(fila, 4).Value = r.EntradasVendidas;
        hoja.Cell(fila, 5).Value = r.PorcentajeAforoOcupado / 100;
        hoja.Cell(fila, 5).Style.NumberFormat.Format = "0.00%";
        hoja.Cell(fila, 6).Value = r.TotalRecaudado;
        hoja.Cell(fila, 6).Style.NumberFormat.Format = FormatoSoles;
        hoja.Row(fila).Style.Font.Bold = true;
        hoja.Columns().AdjustToContents();

        // Por fecha
        hoja = libro.Worksheets.Add("Por fecha");
        Encabezado(hoja, "Fecha", "Entradas vendidas", "Monto (S/)");
        fila = 2;
        foreach (var d in r.VentasPorDia)
        {
            hoja.Cell(fila, 1).Value = d.Fecha;
            hoja.Cell(fila, 1).Style.DateFormat.Format = "dd/MM/yyyy";
            hoja.Cell(fila, 2).Value = d.EntradasVendidas;
            hoja.Cell(fila, 3).Value = d.Monto;
            hoja.Cell(fila, 3).Style.NumberFormat.Format = FormatoSoles;
            fila++;
        }
        hoja.Cell(fila, 1).Value = "TOTAL";
        hoja.Cell(fila, 2).Value = r.EntradasVendidas;
        hoja.Cell(fila, 3).Value = r.TotalRecaudado;
        hoja.Cell(fila, 3).Style.NumberFormat.Format = FormatoSoles;
        hoja.Row(fila).Style.Font.Bold = true;
        hoja.Columns().AdjustToContents();

        // Detalle
        hoja = libro.Worksheets.Add("Detalle de ventas");
        Encabezado(hoja, "N° de compra", "Comprador", "Evento", "Cantidad", "Total (S/)", "Fecha y hora", "Estado");
        fila = 2;
        foreach (var v in r.DetalleVentas)
        {
            hoja.Cell(fila, 1).Value = v.NumeroCompra;
            hoja.Cell(fila, 2).Value = v.Comprador;
            hoja.Cell(fila, 3).Value = v.Evento;
            hoja.Cell(fila, 4).Value = v.Cantidad;
            hoja.Cell(fila, 5).Value = v.Total;
            hoja.Cell(fila, 5).Style.NumberFormat.Format = FormatoSoles;
            hoja.Cell(fila, 6).Value = v.FechaCompra;
            hoja.Cell(fila, 6).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            hoja.Cell(fila, 7).Value = v.Estado;
            fila++;
        }
        hoja.Cell(fila, 1).Value = "TOTAL";
        hoja.Cell(fila, 4).Value = r.EntradasVendidas;
        hoja.Cell(fila, 5).Value = r.TotalRecaudado;
        hoja.Cell(fila, 5).Style.NumberFormat.Format = FormatoSoles;
        hoja.Row(fila).Style.Font.Bold = true;
        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        libro.SaveAs(stream);
        return stream.ToArray();
    }

    public byte[] ExportarPdf(ReporteResumenDTO r)
    {
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));
                page.Header().Text("Reporte de ventas - EventosPerúIA").FontSize(16).SemiBold();

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Text($"Total recaudado: {Soles(r.TotalRecaudado)}   |   Entradas vendidas: {r.EntradasVendidas}   |   " +
                                    $"Aforo ocupado: {r.PorcentajeAforoOcupado:0.00}%   |   Asistentes que ingresaron: {r.AsistentesIngresaron}");

                    Tabla(col, "Ventas por evento",
                        new[] { "Evento", "Fecha", "Aforo", "Vendidas", "% ocup.", "Monto", "Estado" },
                        r.VentasPorEvento.Select(e => new[]
                        {
                            e.Evento, e.Fecha.ToString("dd/MM/yyyy"), e.Aforo.ToString(), e.EntradasVendidas.ToString(),
                            $"{e.PorcentajeOcupacion:0.00}%", Soles(e.MontoRecaudado), e.Estado
                        }),
                        new[] { "TOTAL", "", r.AforoTotal.ToString(), r.EntradasVendidas.ToString(),
                                $"{r.PorcentajeAforoOcupado:0.00}%", Soles(r.TotalRecaudado), "" });

                    Tabla(col, "Ventas por fecha",
                        new[] { "Fecha", "Entradas vendidas", "Monto" },
                        r.VentasPorDia.Select(d => new[] { d.Fecha.ToString("dd/MM/yyyy"), d.EntradasVendidas.ToString(), Soles(d.Monto) }),
                        new[] { "TOTAL", r.EntradasVendidas.ToString(), Soles(r.TotalRecaudado) });

                    Tabla(col, "Detalle de ventas",
                        new[] { "N° compra", "Comprador", "Evento", "Cant.", "Total", "Fecha y hora", "Estado" },
                        r.DetalleVentas.Select(v => new[]
                        {
                            v.NumeroCompra, v.Comprador, v.Evento, v.Cantidad.ToString(), Soles(v.Total),
                            v.FechaCompra.ToString("dd/MM/yyyy HH:mm"), v.Estado
                        }),
                        new[] { "TOTAL", "", "", r.EntradasVendidas.ToString(), Soles(r.TotalRecaudado), "", "" });
                });

                page.Footer().AlignCenter().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
            });
        });

        return documento.GeneratePdf();
    }

    // ===================== Métodos de apoyo =====================

    private const string FormatoSoles = "\"S/\" #,##0.00";

    private static bool SumaAlTotal(DetalleCompra d) =>
        d.Compra.Estado == EstadosCompra.Pagada && d.Evento.Estado != EstadosEvento.Cancelado;

    private static decimal Porcentaje(int parte, int total) =>
        total > 0 ? Math.Round(parte * 100m / total, 2) : 0;

    private static string Soles(decimal monto) => $"S/ {monto:N2}";

    private static void Encabezado(IXLWorksheet hoja, params string[] titulos)
    {
        for (var i = 0; i < titulos.Length; i++)
            hoja.Cell(1, i + 1).Value = titulos[i];

        var rango = hoja.Range(1, 1, 1, titulos.Length);
        rango.Style.Font.Bold = true;
        rango.Style.Fill.BackgroundColor = XLColor.LightBlue;
    }

    private static void Tabla(ColumnDescriptor col, string titulo, string[] encabezados,
        IEnumerable<string[]> filas, string[] totales)
    {
        col.Item().Text(titulo).FontSize(12).SemiBold();
        col.Item().Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                foreach (var _ in encabezados) c.RelativeColumn();
            });

            table.Header(h =>
            {
                foreach (var e in encabezados)
                    h.Cell().Element(Celda).Text(e).Bold();
            });

            foreach (var fila in filas)
                foreach (var valor in fila)
                    table.Cell().Element(Celda).Text(valor);

            foreach (var valor in totales)
                table.Cell().Element(Celda).Text(valor).Bold();
        });
    }

    private static IContainer Celda(IContainer c) => c.BorderBottom(0.5f).PaddingVertical(3).PaddingHorizontal(2);
}
