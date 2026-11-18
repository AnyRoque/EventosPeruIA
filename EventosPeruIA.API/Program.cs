using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Services;
using EventosPeruIA.CORE.Infrastructure.Data;
using EventosPeruIA.CORE.Infrastructure.Repositories;
using EventosPeruIA.CORE.Infrastructure.Shared;
using EventosPeruIA.API.Workers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

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
    // US-12 Reseñas
    builder.Services.AddTransient<IResenaService, ResenaService>();
// US-06 Compras
builder.Services.AddTransient<ICompraRepository, CompraRepository>();
builder.Services.AddTransient<ICompraService, CompraService>();
// US-07 Check-in
builder.Services.AddTransient<ICheckInRepository, CheckInRepository>();
builder.Services.AddTransient<ICheckInService, CheckInService>();
// US-15 Notificaciones (+ recordatorio automático 24 h antes)
builder.Services.AddTransient<INotificacionRepository, NotificacionRepository>();
builder.Services.AddTransient<INotificacionService, NotificacionService>();
builder.Services.AddHostedService<RecordatorioWorker>();

// JWT
builder.Services.AddSharedInfrastructure(_config);

// Permite que el frontend (Vue + Quasar) consuma la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:9000", "http://127.0.0.1:9000")
              .AllowAnyMethod()
              .AllowAnyHeader());
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

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
