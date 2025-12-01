using Microsoft.EntityFrameworkCore;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tablas existentes
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Recibo> Recibos { get; set; }
        
        // Nueva tabla para autenticación de usuarios
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración existente para Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Dni);
                entity.Property(e => e.Dni).HasMaxLength(9).IsRequired();
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Apellidos).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TipoCliente).HasConversion<string>().IsRequired();
                entity.Property(e => e.CuotaMaxima).HasPrecision(10, 2);
                entity.Property(e => e.FechaAlta).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuración existente para Recibo
            modelBuilder.Entity<Recibo>(entity =>
            {
                entity.HasKey(e => e.NumeroRecibo);
                entity.Property(e => e.NumeroRecibo).IsRequired();
                entity.Property(e => e.DniCliente).HasMaxLength(9).IsRequired();
                entity.Property(e => e.Importe).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.FechaEmision).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(r => r.Cliente)
                      .WithMany(c => c.Recibos)
                      .HasForeignKey(r => r.DniCliente)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Nueva configuración para Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NombreUsuario).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.NombreCompleto).HasMaxLength(100);
                entity.Property(e => e.FechaRegistro).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Índices únicos
                entity.HasIndex(e => e.NombreUsuario).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}