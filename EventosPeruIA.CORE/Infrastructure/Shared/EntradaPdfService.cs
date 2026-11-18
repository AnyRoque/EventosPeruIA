using System.Globalization;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace EventosPeruIA.CORE.Infrastructure.Shared
{
    // US-06: un voucher por página. El QR contiene el mismo GUID que valida CheckInService.
    public class EntradaPdfService : IEntradaPdfService
    {
        public byte[] GenerarPdf(IReadOnlyCollection<EntradaDTO> entradas)
        {
            if (entradas.Count == 0)
                throw new ArgumentException("Se necesita al menos una entrada.", nameof(entradas));

            return Document.Create(documento =>
            {
                foreach (var entrada in entradas)
                {
                    using var datosQr = QRCodeGenerator.GenerateQrCode(
                        entrada.CodigoEntrada.ToString("D"), QRCodeGenerator.ECCLevel.Q);
                    using var qr = new PngByteQRCode(datosQr);
                    var imagenQr = qr.GetGraphic(8);
                    var anulada = entrada.EventoCancelado || entrada.Estado == EstadosEntrada.Anulada;
                    var utilizada = entrada.Estado == EstadosEntrada.Utilizada;

                    documento.Page(pagina =>
                    {
                        pagina.Size(PageSizes.A4);
                        pagina.Margin(40);
                        pagina.DefaultTextStyle(estilo => estilo.FontSize(11));

                        pagina.Header().Column(columna =>
                        {
                            columna.Item().Text("EventosPeruIA").FontSize(22).Bold();
                            columna.Item().Text("Entrada digital").FontSize(12).FontColor(Colors.Grey.Darken2);
                        });

                        // Mantiene cada voucher en una página aun con los tamaños máximos de los campos.
                        pagina.Content().PaddingTop(24).ScaleToFit().Column(columna =>
                        {
                            columna.Spacing(12);
                            columna.Item().Text(entrada.Evento).FontSize(20).SemiBold();
                            columna.Item().Text($"Compra {entrada.NumeroCompra} · Entrada {entrada.NumeroEntrada} de {entrada.TotalEntradas}");
                            columna.Item().Text($"Asistente: {entrada.Asistente}");
                            columna.Item().Text($"Fecha y hora: {entrada.FechaHoraInicio:dd/MM/yyyy HH:mm}");
                            columna.Item().Text($"Lugar: {entrada.Lugar}");
                            columna.Item().Text(entrada.PrecioPagado == 0
                                ? "Precio pagado: Gratis"
                                : $"Precio pagado: S/ {entrada.PrecioPagado.ToString("0.00", CultureInfo.InvariantCulture)}");

                            var estado = anulada ? "ENTRADA ANULADA - NO PERMITE EL INGRESO"
                                : utilizada ? "ENTRADA UTILIZADA - NO PERMITE OTRO INGRESO"
                                : "ENTRADA VÁLIDA";
                            columna.Item().Text(estado).Bold()
                                .FontColor(anulada || utilizada ? Colors.Red.Darken2 : Colors.Green.Darken3);

                            if (entrada.EventoCancelado && !string.IsNullOrWhiteSpace(entrada.MotivoCancelacion))
                                columna.Item().Text($"Motivo de cancelación: {entrada.MotivoCancelacion}").FontSize(10);

                            columna.Item().AlignCenter().Width(170).Image(imagenQr).FitWidth();
                            columna.Item().AlignCenter().Text(entrada.CodigoEntrada.ToString("D")).FontSize(9);
                            columna.Item().Text("Presenta este QR al ingresar. Su estado se verifica en el servidor; cada entrada permite un solo ingreso.").FontSize(10);
                        });

                        pagina.Footer().AlignCenter().Text("EventosPeruIA · Conserva tu entrada digital").FontSize(9).FontColor(Colors.Grey.Darken2);
                    });
                }
            }).GeneratePdf();
        }
    }
}
