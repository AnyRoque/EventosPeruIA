using System.Net;
using System.Text;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventosPeruIA.Tests;

public class EntradaPdfTests
{
    [Fact]
    public async Task DescargaIndividualYCompra_DevuelvenPdfPrivado_YNoCambianEntrada()
    {
        await using var app = await TestApp.Create();
        var compra = await app.Comprar();
        var idEntrada = 0;
        await app.WithDb(async db => idEntrada = await db.Entrada.Select(e => e.IdEntrada).FirstAsync());
        foreach (var ruta in new[] { $"/api/Compra/entradas/{idEntrada}/pdf", $"/api/Compra/{compra}/pdf" })
        {
            var response = await app.Client.GetAsync(ruta);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/pdf", response.Content.Headers.ContentType!.MediaType);
            Assert.Equal("attachment", response.Content.Headers.ContentDisposition!.DispositionType);
            Assert.True(response.Headers.CacheControl!.NoStore);
            var bytes = await response.Content.ReadAsByteArrayAsync();
            Assert.StartsWith("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
            var destino = Environment.GetEnvironmentVariable("EVENTOS_PDF_QA_DIR");
            if (!string.IsNullOrWhiteSpace(destino) && ruta == $"/api/Compra/{compra}/pdf")
            {
                Directory.CreateDirectory(destino);
                await File.WriteAllBytesAsync(Path.Combine(destino, "compra-prueba.pdf"), bytes);
            }
        }
        await app.WithDb(async db =>
        {
            Assert.All(await db.Entrada.ToListAsync(), e => Assert.Equal(EstadosEntrada.Valida, e.Estado));
            Assert.Empty(await db.CheckIn.ToListAsync());
        });
    }

    [Fact]
    public async Task NoPuedeDescargarEntradasAjenasNiSinSesion()
    {
        await using var app = await TestApp.Create();
        var compra = await app.Comprar(idUsuario: 2);
        var idEntrada = 0;
        await app.WithDb(async db => idEntrada = await db.Entrada.Select(e => e.IdEntrada).FirstAsync());
        foreach (var ruta in new[] { $"/api/Compra/entradas/{idEntrada}/pdf", $"/api/Compra/{compra}/pdf" })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await app.Client.GetAsync(ruta)).StatusCode);
            app.Client.DefaultRequestHeaders.Authorization = null;
            Assert.Equal(HttpStatusCode.Unauthorized, (await app.Client.GetAsync(ruta)).StatusCode);
            app.Login(null);
            Assert.Equal(HttpStatusCode.Unauthorized, (await app.Client.GetAsync(ruta)).StatusCode);
            app.Login(1);
        }
        Assert.Equal(HttpStatusCode.NotFound, (await app.Client.GetAsync("/api/Compra/entradas/999999/pdf")).StatusCode);
    }

    [Theory]
    [InlineData(EstadosCompra.Pendiente)]
    [InlineData(EstadosCompra.Rechazada)]
    public async Task CompraNoPagada_NoTienePdfDescargable(string estado)
    {
        await using var app = await TestApp.Create();
        var compra = await app.Comprar(estado: estado);
        Assert.Equal(HttpStatusCode.Conflict, (await app.Client.GetAsync($"/api/Compra/{compra}/pdf")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await app.Client.GetAsync("/api/Compra/entradas/1/pdf")).StatusCode);
    }

    [Fact]
    public void GeneraVouchersDePrueba_ConAcentosTextosLargosYEstados()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var entradas = new List<EntradaDTO>();
        for (var n = 1; n <= 3; n++)
        {
            entradas.Add(new EntradaDTO
            {
                IdEntrada = n, CodigoEntrada = Guid.Parse($"11111111-2222-3333-4444-{n:D12}"), NumeroEntrada = n,
                TotalEntradas = 3, NumeroCompra = "EP-000001", Asistente = n == 1 ? "Jesús Cárdenas Rodríguez" : new string('W', 50) + " " + new string('W', 50),
                Evento = n == 1 ? "Noche de música y tradición en Barranco" : new string('M', 150),
                FechaHoraInicio = new DateTime(2026, 12, 12, 19, 30, 0),
                Lugar = n == 1 ? "Av. Pedro de Osma 135, Barranco, Lima" : new string('W', 250) + ", " + new string('W', 50) + ", " + new string('W', 50),
                PrecioPagado = n == 1 ? 45.50m : 0,
                Estado = n == 1 ? EstadosEntrada.Valida : n == 2 ? EstadosEntrada.Utilizada : EstadosEntrada.Anulada,
                EventoCancelado = n == 3,
                MotivoCancelacion = n == 3 ? new string('W', 500) : null
            });
        }
        var pdf = new EntradaPdfService().GenerarPdf(entradas);
        Assert.True(pdf.Length > 1000);
        var destino = Environment.GetEnvironmentVariable("EVENTOS_PDF_QA_DIR");
        if (!string.IsNullOrWhiteSpace(destino))
        {
            Directory.CreateDirectory(destino);
            File.WriteAllBytes(Path.Combine(destino, "entradas-prueba.pdf"), pdf);
        }
    }
}
