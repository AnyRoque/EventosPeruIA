using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Services;
using EventosPeruIA.CORE.Infrastructure.Data;
using EventosPeruIA.CORE.Infrastructure.Repositories;
using EventosPeruIA.CORE.Infrastructure.Shared;
using EventosPeruIA.API.Workers;
using Microsoft.Extensions.AI;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

// Proyecto académico: licencia Community de QuestPDF.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var _config = builder.Configuration;
var cnx = _config.GetConnectionString("DevConnection");
builder.Services.AddDbContext<EventosPeruIAContext>(options => options.UseSqlServer(cnx));

// US-01 / US-02 Usuarios
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddTransient<IUsuarioService, UsuarioService>();
// US-13 Categorías
builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddTransient<ICategoriaService, CategoriaService>();
// US-05 / US-09 / US-10 / US-03 Eventos
builder.Services.AddTransient<IEventoRepository, EventoRepository>();
builder.Services.AddTransient<IEventoService, EventoService>();
// US-06 Compras
builder.Services.AddTransient<ICompraRepository, CompraRepository>();
builder.Services.AddTransient<ICompraService, CompraService>();
builder.Services.AddTransient<IEntradaPdfService, EntradaPdfService>();
// US-08 Intereses y recomendaciones personalizadas
builder.Services.AddTransient<IRecomendacionRepository, RecomendacionRepository>();
builder.Services.AddTransient<IRecomendacionService, RecomendacionService>();
// US-07 Check-in
builder.Services.AddTransient<ICheckInRepository, CheckInRepository>();
builder.Services.AddTransient<ICheckInService, CheckInService>();
// US-11 Reportes del organizador
builder.Services.AddTransient<IReporteRepository, ReporteRepository>();
builder.Services.AddTransient<IReporteService, ReporteService>();
// US-12 Reseñas
builder.Services.AddTransient<IResenaRepository, ResenaRepository>();
builder.Services.AddTransient<IResenaService, ResenaService>();
// US-15 Notificaciones (+ recordatorio automático 24 h antes)
builder.Services.AddTransient<INotificacionRepository, NotificacionRepository>();
builder.Services.AddTransient<INotificacionService, NotificacionService>();
builder.Services.AddHostedService<RecordatorioWorker>();

// US-14 PeruIA: cliente de IA (Gemini u Ollama, se elige en appsettings "AI:Provider")
// Si no hay clave de Gemini, se usa Ollama: la API arranca igual y PeruIA responde "no disponible"
var usarGemini = _config["AI:Provider"] == "Gemini" && !string.IsNullOrWhiteSpace(_config["AI:Gemini:ApiKey"]);
IChatClient chatClient = usarGemini
    ? new Google.GenAI.Client(apiKey: _config["AI:Gemini:ApiKey"]).AsIChatClient(_config["AI:Gemini:Model"])
    : new OllamaApiClient(new Uri(_config["AI:Ollama:Endpoint"]!), _config["AI:Ollama:Model"]!);
builder.Services.AddChatClient(chatClient);
builder.Services.AddTransient<IPeruIARepository, PeruIARepository>();
builder.Services.AddTransient<IPeruIAService, PeruIAService>();

// US-16 Moderación
builder.Services.AddTransient<IModeracionRepository, ModeracionRepository>();
builder.Services.AddTransient<IModeracionService, ModeracionService>();

// JWT
builder.Services.AddSharedInfrastructure(_config);

// Permite que el frontend (Vue + Quasar) consuma la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(_config["FrontendUrl"] ?? "http://localhost:9000")
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition")
              .AllowAnyMethod());
});

// Errores de validación con el mismo formato que el resto: { mensaje, errores }
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errores = context.ModelState
                .Where(x => x.Value!.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
            return new BadRequestObjectResult(new { mensaje = "Revisa los datos ingresados.", errores });
        };
    });
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
