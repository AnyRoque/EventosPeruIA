using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Settings;
using Xunit;

namespace EventosPeruIA.Tests;

public class RecomendacionTests
{
    private static async Task<RecomendacionResultadoDTO> Obtener(TestApp app)
    {
        var response = await app.Client.GetAsync("/api/Recomendacion");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("datos").Deserialize<RecomendacionResultadoDTO>(new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
    }

    [Fact]
    public async Task SinHistorial_UsaPopularesDelDepartamento_YExcluyeNoDisponiblesYPropios()
    {
        await using var app = await TestApp.Create();
        var datos = await Obtener(app);
        Assert.True(datos.EsPopular);
        Assert.Equal("Populares cerca de ti", datos.Titulo);
        Assert.Equal(new[] { 101, 100 }, datos.Eventos.Select(e => e.IdEvento));
        Assert.All(datos.Eventos, e => Assert.False(string.IsNullOrWhiteSpace(e.Motivo)));
    }

    [Fact]
    public async Task Intereses_SeGuardan_SeReemplazan_YCambianElOrdenSinCache()
    {
        await using var app = await TestApp.Create();
        var guardar = await app.Client.PutAsJsonAsync("/api/Recomendacion/mis-intereses", new { idIntereses = new[] { 1 } });
        Assert.Equal(HttpStatusCode.OK, guardar.StatusCode);
        var musica = await Obtener(app);
        Assert.False(musica.EsPopular);
        Assert.True(musica.TieneIntereses);
        Assert.Equal(100, musica.Eventos[0].IdEvento);
        Assert.Contains("Música", musica.Eventos[0].Motivo);

        guardar = await app.Client.PutAsJsonAsync("/api/Recomendacion/mis-intereses", new { idIntereses = new[] { 2 } });
        Assert.Equal(HttpStatusCode.OK, guardar.StatusCode);
        var tecnologia = await Obtener(app);
        Assert.Equal(101, tecnologia.Eventos[0].IdEvento);
        Assert.Contains("Tecnología", tecnologia.Eventos[0].Motivo);
        var perfil = await app.Client.GetFromJsonAsync<JsonElement>("/api/Recomendacion/mis-intereses");
        Assert.Single(perfil.GetProperty("datos").EnumerateArray());
        Assert.Equal(2, perfil.GetProperty("datos")[0].GetProperty("idInteres").GetInt32());
        Assert.DoesNotContain("Cerca de ti", tecnologia.Eventos.Single(e => e.IdEvento == 108).Motivo);
    }

    [Theory]
    [InlineData(new int[] { })]
    [InlineData(new[] { 1, 1 })]
    [InlineData(new[] { 3 })]
    [InlineData(new[] { 999 })]
    [InlineData(new[] { 0 })]
    [InlineData(new[] { 1, 2, 3, 4, 5, 6 })]
    public async Task InteresesInvalidos_NoModificanLaSeleccion(int[] ids)
    {
        await using var app = await TestApp.Create();
        await app.Client.PutAsJsonAsync("/api/Recomendacion/mis-intereses", new { idIntereses = new[] { 1 } });
        var response = await app.Client.PutAsJsonAsync("/api/Recomendacion/mis-intereses", new { idIntereses = ids });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var datos = await Obtener(app);
        Assert.Contains("Música", datos.Eventos[0].Motivo);
    }

    [Fact]
    public async Task ComprasYVisitas_Influyen_YCompraNuevaDejaDeRecomendarse()
    {
        await using var app = await TestApp.Create();
        await app.WithDb(async db =>
        {
            db.Evento.Add(TestApp.Evento(109, "Otro concierto", 1));
            db.EventoVisita.Add(new EventoVisita { IdUsuario = 1, IdEvento = 100, FechaVisita = DateTime.Now });
            await db.SaveChangesAsync();
        });
        var visitadas = await Obtener(app);
        Assert.Equal(100, visitadas.Eventos[0].IdEvento);
        Assert.Contains("visitaste", visitadas.Eventos[0].Motivo);
        await app.Comprar();
        var compradas = await Obtener(app);
        Assert.DoesNotContain(compradas.Eventos, e => e.IdEvento == 100);
        Assert.Equal(109, compradas.Eventos[0].IdEvento);
        Assert.Contains("compras", compradas.Eventos[0].Motivo);
    }

    [Fact]
    public async Task VisitasAntiguasYComprasRechazadas_NoSonSenales()
    {
        await using var app = await TestApp.Create();
        await app.Comprar(estado: EstadosCompra.Rechazada);
        await app.WithDb(async db =>
        {
            db.EventoVisita.Add(new EventoVisita { IdUsuario = 1, IdEvento = 100, FechaVisita = DateTime.Now.AddDays(-91) });
            await db.SaveChangesAsync();
        });
        var datos = await Obtener(app);
        Assert.True(datos.EsPopular);
        Assert.Contains(datos.Eventos, e => e.IdEvento == 100);
    }

    [Fact]
    public async Task DevuelveMaximoSeis_YReflejaAgotamientoYCancelacion()
    {
        await using var app = await TestApp.Create();
        await app.WithDb(async db =>
        {
            for (var id = 200; id < 210; id++) db.Evento.Add(TestApp.Evento(id, $"Concierto {id}", 1, id - 150));
            await db.SaveChangesAsync();
        });
        var antes = await Obtener(app);
        Assert.Equal(6, antes.Eventos.Count);
        Assert.Equal(209, antes.Eventos[0].IdEvento);
        await app.WithDb(async db =>
        {
            (await db.Evento.FindAsync(209))!.EntradasVendidas = 100;
            (await db.Evento.FindAsync(208))!.Estado = EstadosEvento.Cancelado;
            await db.SaveChangesAsync();
        });
        var despues = await Obtener(app);
        Assert.Equal(6, despues.Eventos.Count);
        Assert.DoesNotContain(despues.Eventos, e => e.IdEvento == 209 || e.IdEvento == 208);
    }

    [Fact]
    public async Task SinEventos_DevuelveListaVacia_YNoInventaSugerencias()
    {
        await using var app = await TestApp.Create();
        await app.WithDb(async db =>
        {
            (await db.Usuario.FindAsync(1))!.Departamento = "Tacna";
            await db.SaveChangesAsync();
        });
        Assert.Empty((await Obtener(app)).Eventos);
    }

    [Fact]
    public async Task Autorizacion_NoPermiteAccesoAnonimoSinClaimOSuspendido()
    {
        await using var app = await TestApp.Create();
        app.Client.DefaultRequestHeaders.Authorization = null;
        Assert.Equal(HttpStatusCode.Unauthorized, (await app.Client.GetAsync("/api/Recomendacion")).StatusCode);
        app.Login(null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await app.Client.GetAsync("/api/Recomendacion")).StatusCode);
        app.Login(4);
        Assert.Equal(HttpStatusCode.Forbidden, (await app.Client.GetAsync("/api/Recomendacion")).StatusCode);
        app.Login(999);
        Assert.Equal(HttpStatusCode.NotFound, (await app.Client.GetAsync("/api/Recomendacion")).StatusCode);
    }
}
