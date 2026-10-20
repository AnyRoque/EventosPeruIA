using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Infrastructure.Data;

public partial class EventosPeruIAContext : DbContext
{
    public EventosPeruIAContext(DbContextOptions<EventosPeruIAContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Rol> Rol { get; set; }
    public virtual DbSet<Usuario> Usuario { get; set; }
    public virtual DbSet<UsuarioRol> UsuarioRol { get; set; }
    public virtual DbSet<Interes> Interes { get; set; }
    public virtual DbSet<UsuarioInteres> UsuarioInteres { get; set; }
    public virtual DbSet<Categoria> Categoria { get; set; }
    public virtual DbSet<Evento> Evento { get; set; }
    public virtual DbSet<EventoVisita> EventoVisita { get; set; }
    public virtual DbSet<Compra> Compra { get; set; }
    public virtual DbSet<DetalleCompra> DetalleCompra { get; set; }
    public virtual DbSet<Entrada> Entrada { get; set; }
    public virtual DbSet<CheckIn> CheckIn { get; set; }
    public virtual DbSet<Resena> Resena { get; set; }
    public virtual DbSet<Notificacion> Notificacion { get; set; }
    public virtual DbSet<Moderacion> Moderacion { get; set; }
    public virtual DbSet<PreguntaFrecuente> PreguntaFrecuente { get; set; }
    public virtual DbSet<Embedding> Embedding { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol);
            entity.Property(e => e.Nombre).HasMaxLength(30).IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.Nombre).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Apellido).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Correo).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Celular).HasMaxLength(9).IsUnicode(false).IsFixedLength();
            entity.Property(e => e.Departamento).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Distrito).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.NombreOrganizador).HasMaxLength(80).IsUnicode(false);
            entity.Property(e => e.Estado).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.MotivoSuspension).HasMaxLength(500).IsUnicode(false);
        });

        modelBuilder.Entity<UsuarioRol>(entity =>
        {
            entity.HasKey(e => new { e.IdUsuario, e.IdRol });
            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioRol).HasForeignKey(d => d.IdUsuario);
            entity.HasOne(d => d.Rol).WithMany(p => p.UsuarioRol).HasForeignKey(d => d.IdRol);
        });

        modelBuilder.Entity<Interes>(entity =>
        {
            entity.HasKey(e => e.IdInteres);
            entity.Property(e => e.Nombre).HasMaxLength(80).IsUnicode(false);
        });

        modelBuilder.Entity<UsuarioInteres>(entity =>
        {
            entity.HasKey(e => new { e.IdUsuario, e.IdInteres });
            entity.HasOne(d => d.Usuario).WithMany(p => p.UsuarioInteres).HasForeignKey(d => d.IdUsuario);
            entity.HasOne(d => d.Interes).WithMany(p => p.UsuarioInteres).HasForeignKey(d => d.IdInteres);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria);
            entity.Property(e => e.Nombre).HasMaxLength(80).IsUnicode(false);
            entity.Property(e => e.Descripcion).HasMaxLength(250).IsUnicode(false);
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.IdEvento);
            entity.Property(e => e.Nombre).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Descripcion).HasMaxLength(1000).IsUnicode(false);
            entity.Property(e => e.Departamento).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Distrito).HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Direccion).HasMaxLength(250).IsUnicode(false);
            entity.Property(e => e.ImagenUrl).HasMaxLength(500).IsUnicode(false);
            entity.Property(e => e.Precio).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Estado).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.MotivoCancelacion).HasMaxLength(500).IsUnicode(false);
            entity.HasOne(d => d.Organizador).WithMany(p => p.Evento).HasForeignKey(d => d.IdOrganizador);
            entity.HasOne(d => d.Categoria).WithMany(p => p.Evento).HasForeignKey(d => d.IdCategoria);
        });

        modelBuilder.Entity<EventoVisita>(entity =>
        {
            entity.HasKey(e => e.IdEventoVisita);
            entity.HasOne(d => d.Usuario).WithMany().HasForeignKey(d => d.IdUsuario);
            entity.HasOne(d => d.Evento).WithMany().HasForeignKey(d => d.IdEvento);
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.IdCompra);
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TitularTarjeta).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.UltimosDigitos).HasMaxLength(4).IsUnicode(false).IsFixedLength();
            entity.Property(e => e.Estado).HasMaxLength(20).IsUnicode(false);
            entity.HasOne(d => d.Usuario).WithMany(p => p.Compra).HasForeignKey(d => d.IdUsuario);
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.IdDetalleCompra);
            entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Subtotal).HasColumnType("decimal(10, 2)");
            entity.HasOne(d => d.Compra).WithMany(p => p.DetalleCompra).HasForeignKey(d => d.IdCompra);
            entity.HasOne(d => d.Evento).WithMany(p => p.DetalleCompra).HasForeignKey(d => d.IdEvento);
        });

        modelBuilder.Entity<Entrada>(entity =>
        {
            entity.HasKey(e => e.IdEntrada);
            entity.Property(e => e.Estado).HasMaxLength(20).IsUnicode(false);
            entity.HasOne(d => d.DetalleCompra).WithMany(p => p.Entrada).HasForeignKey(d => d.IdDetalleCompra);
        });

        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasKey(e => e.IdCheckIn);
            entity.HasIndex(e => e.IdEntrada).IsUnique();
            entity.HasOne(d => d.Entrada).WithOne(p => p.CheckIn).HasForeignKey<CheckIn>(d => d.IdEntrada);
            entity.HasOne(d => d.UsuarioValidador).WithMany().HasForeignKey(d => d.IdUsuarioValidador);
        });

        modelBuilder.Entity<Resena>(entity =>
        {
            entity.HasKey(e => e.IdResena);
            entity.Property(e => e.Comentario).HasMaxLength(500).IsUnicode(false);
            entity.HasOne(d => d.Usuario).WithMany().HasForeignKey(d => d.IdUsuario);
            entity.HasOne(d => d.Evento).WithMany(p => p.Resena).HasForeignKey(d => d.IdEvento);
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion);
            entity.Property(e => e.Tipo).HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Titulo).HasMaxLength(150).IsUnicode(false);
            entity.Property(e => e.Mensaje).HasMaxLength(1000).IsUnicode(false);
            entity.HasOne(d => d.Usuario).WithMany().HasForeignKey(d => d.IdUsuario);
            entity.HasOne(d => d.Evento).WithMany().HasForeignKey(d => d.IdEvento);
        });

        modelBuilder.Entity<Moderacion>(entity =>
        {
            entity.HasKey(e => e.IdModeracion);
            entity.Property(e => e.Accion).HasMaxLength(30).IsUnicode(false);
            entity.Property(e => e.Motivo).HasMaxLength(500).IsUnicode(false);
            entity.HasOne(d => d.Administrador).WithMany().HasForeignKey(d => d.IdAdministrador).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.UsuarioAfectado).WithMany().HasForeignKey(d => d.IdUsuarioAfectado).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.EventoAfectado).WithMany().HasForeignKey(d => d.IdEventoAfectado).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.ResenaAfectada).WithMany().HasForeignKey(d => d.IdResenaAfectada).OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PreguntaFrecuente>(entity =>
        {
            entity.HasKey(e => e.IdPregunta);
            entity.Property(e => e.Pregunta).HasMaxLength(300).IsUnicode(false);
            entity.Property(e => e.Respuesta).HasMaxLength(2000).IsUnicode(false);
        });

        modelBuilder.Entity<Embedding>(entity =>
        {
            entity.HasKey(e => e.IdEmbedding);
            entity.Property(e => e.TipoOrigen).HasMaxLength(10).IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
