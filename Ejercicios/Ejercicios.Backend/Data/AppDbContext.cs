using Microsoft.EntityFrameworkCore;
using Ejercicios.Backend.Models;

namespace Ejercicios.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Recibo> Recibos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("clientes");
                entity.HasKey(e => e.Dni);
                entity.Property(e => e.Dni).HasColumnName("dni").HasMaxLength(9);
                entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Apellidos).HasColumnName("apellidos").HasMaxLength(100).IsRequired();
                entity.Property(e => e.TipoCliente).HasColumnName("tipo_cliente").HasMaxLength(20).HasConversion<string>().IsRequired();
                entity.Property(e => e.CuotaMaxima).HasColumnName("cuota_maxima").HasColumnType("decimal(10,2)");
                entity.Property(e => e.FechaAlta).HasColumnName("fecha_alta").HasColumnType("timestamptz").IsRequired();
            });

            // Configuración de Recibo
            modelBuilder.Entity<Recibo>(entity =>
            {
                entity.ToTable("recibos");
                entity.HasKey(e => e.NumeroRecibo);
                entity.Property(e => e.NumeroRecibo).HasColumnName("numero_recibo").HasMaxLength(50);
                entity.Property(e => e.DniCliente).HasColumnName("dni_cliente").HasMaxLength(9).IsRequired();
                entity.Property(e => e.Importe).HasColumnName("importe").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision").HasColumnType("timestamptz").IsRequired();

                // Relación con Cliente
                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Recibos)
                      .HasForeignKey(e => e.DniCliente)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}