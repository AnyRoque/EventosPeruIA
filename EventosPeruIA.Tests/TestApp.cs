using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using EventosPeruIA.API.Controllers;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Services;
using EventosPeruIA.CORE.Core.Settings;
using EventosPeruIA.CORE.Infrastructure.Data;
using EventosPeruIA.CORE.Infrastructure.Repositories;
using EventosPeruIA.CORE.Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace EventosPeruIA.Tests;

// HTTP real en memoria + BD SQLite aislada. No usa la configuración ni los datos del proyecto.
internal sealed class TestApp : IAsyncDisposable
{
    private const string Key = "clave-solo-para-pruebas-eventosperuia-0123456789abcdef";
    private readonly SqliteConnection _connection;
    private readonly WebApplication _app;
    public HttpClient Client { get; }

    private TestApp(SqliteConnection connection, WebApplication app)
    {
        _connection = connection;
        _app = app;
        Client = app.GetTestClient();
    }

    public static async Task<TestApp> Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContext<EventosPeruIAContext>(o => o.UseSqlite(connection));
        builder.Services.AddControllers().AddApplicationPart(typeof(CompraController).Assembly);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false, ValidateAudience = false, ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)),
                ClockSkew = TimeSpan.Zero
            };
        });
        builder.Services.AddAuthorization();
        builder.Services.AddTransient<ICompraRepository, CompraRepository>();
        builder.Services.AddTransient<ICompraService, CompraService>();
        builder.Services.AddTransient<IEventoRepository, EventoRepository>();
        builder.Services.AddTransient<INotificacionService, NotificacionService>();
        builder.Services.AddTransient<INotificacionRepository, NotificacionRepository>();
        builder.Services.AddTransient<IEntradaPdfService, EntradaPdfService>();
        builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
        builder.Services.AddTransient<IRecomendacionRepository, RecomendacionRepository>();
        builder.Services.AddTransient<IRecomendacionService, RecomendacionService>();
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        var result = new TestApp(connection, app);
        await result.WithDb(async db =>
        {
            await db.Database.EnsureCreatedAsync();
            for (var id = 1; id <= 4; id++)
                db.Usuario.Add(new Usuario
                {
                    IdUsuario = id, Nombre = $"Usuario {id}", Apellido = "Muñante Cárdenas",
                    Correo = $"usuario{id}@example.test", Celular = "999999999", PasswordHash = "prueba",
                    FechaNacimiento = new DateOnly(2000, 1, 1), Departamento = id == 3 ? "Cusco" : "Lima",
                    Distrito = "Miraflores", Estado = id == 4 ? "Suspendido" : "Activo", FechaRegistro = DateTime.Now
                });
            db.Categoria.AddRange(new Categoria { IdCategoria = 1, Nombre = "Concierto", Estado = true },
                new Categoria { IdCategoria = 2, Nombre = "Charla", Estado = true });
            db.Interes.AddRange(new Interes { IdInteres = 1, Nombre = "Música", Estado = true },
                new Interes { IdInteres = 2, Nombre = "Tecnología", Estado = true },
                new Interes { IdInteres = 3, Nombre = "Inactivo", Estado = false });
            db.Evento.AddRange(Evento(100, "Concierto de música", 1, 10),
                Evento(101, "Charla de tecnología", 2, 50),
                Evento(102, "Concierto agotado", 1, 100),
                Evento(103, "Concierto cancelado", 1, estado: EstadosEvento.Cancelado),
                Evento(104, "Concierto suspendido", 1, estado: EstadosEvento.Suspendido),
                Evento(105, "Concierto borrador", 1, estado: EstadosEvento.Borrador),
                Evento(106, "Concierto pasado", 1, dias: -1),
                Evento(107, "Concierto propio", 1, organizador: 1),
                Evento(108, "Charla de tecnología fuera de Lima", 2, 80, departamento: "Cusco"));
            await db.SaveChangesAsync();
        });
        result.Login(1);
        return result;
    }

    public void Login(int? id)
    {
        var claims = id.HasValue ? new[] { new Claim("UserId", id.Value.ToString()) } : Array.Empty<Claim>();
        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key)), SecurityAlgorithms.HmacSha256));
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new JwtSecurityTokenHandler().WriteToken(token));
    }

    public async Task WithDb(Func<EventosPeruIAContext, Task> action)
    {
        using var scope = _app.Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<EventosPeruIAContext>());
    }

    public static Evento Evento(int id, string nombre, int categoria, int vendidas = 0,
        string estado = EstadosEvento.Publicado, int dias = 10, int organizador = 2, string departamento = "Lima") => new()
    {
        IdEvento = id, IdOrganizador = organizador, IdCategoria = categoria, Nombre = nombre,
        Descripcion = nombre + " con actividades para los asistentes.", Departamento = departamento, Distrito = "Miraflores",
        Direccion = "Av. José Pardo 123", AforoMaximo = 100, EntradasVendidas = vendidas, Precio = 45.50m,
        Estado = estado, FechaHoraInicio = DateTime.Now.AddDays(dias), FechaHoraFin = DateTime.Now.AddDays(dias).AddHours(2),
        FechaCreacion = DateTime.Now
    };

    public async Task<int> Comprar(int idEvento = 100, int idUsuario = 1, string estado = EstadosCompra.Pagada)
    {
        var idCompra = 0;
        await WithDb(async db =>
        {
            var compra = new Compra
            {
                IdUsuario = idUsuario, CodigoOperacion = Guid.NewGuid(), Estado = estado,
                MontoTotal = 80m, FechaCompra = DateTime.Now,
                DetalleCompra = new List<DetalleCompra>
                {
                    new() { IdEvento = idEvento, Cantidad = 2, PrecioUnitario = 40m, Subtotal = 80m,
                        Entrada = Enumerable.Range(1, 2).Select(n => new Entrada
                        {
                            NumeroEntrada = n, CodigoEntrada = Guid.NewGuid(), Estado = EstadosEntrada.Valida,
                            FechaGeneracion = DateTime.Now
                        }).ToList() }
                }
            };
            db.Compra.Add(compra);
            await db.SaveChangesAsync();
            idCompra = compra.IdCompra;
        });
        return idCompra;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
