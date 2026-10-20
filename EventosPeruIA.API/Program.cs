using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Services;
using EventosPeruIA.CORE.Infrastructure.Data;
using EventosPeruIA.CORE.Infrastructure.Repositories;
using EventosPeruIA.CORE.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

var _config = builder.Configuration;
var cnx = _config.GetConnectionString("DevConnection");
builder.Services.AddDbContext<EventosPeruIAContext>(options => options.UseSqlServer(cnx));

// US-01 / US-05 Usuarios
builder.Services.AddTransient<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddTransient<IUsuarioService, UsuarioService>();
// US-04 Categorías
builder.Services.AddTransient<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddTransient<ICategoriaService, CategoriaService>();
// US-02 / US-03 / US-07 / US-09 Eventos
builder.Services.AddTransient<IEventoRepository, EventoRepository>();
builder.Services.AddTransient<IEventoService, EventoService>();
// US-06 Compras
builder.Services.AddTransient<ICompraRepository, CompraRepository>();
builder.Services.AddTransient<ICompraService, CompraService>();
// US-10 Check-in
builder.Services.AddTransient<ICheckInRepository, CheckInRepository>();
builder.Services.AddTransient<ICheckInService, CheckInService>();

// JWT
builder.Services.AddSharedInfrastructure(_config);

// Permite que el frontend (Vue + Quasar) consuma la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(_config["FrontendUrl"] ?? "http://localhost:9000")
              .AllowAnyHeader()
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
