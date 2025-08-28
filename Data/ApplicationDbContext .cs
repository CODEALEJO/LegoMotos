using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LavaderoMotos.Models;
using Microsoft.AspNetCore.Identity;

namespace LavaderoMotos.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venta> Ventas { get; set; }
        public DbSet<ProductoVenta> ProductosVenta { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<MetodoPago> MetodoPagos { get; set; }
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<MovimientoCaja> MovimientoCajas { get; set; }
        public DbSet<OrdenTrabajo> OrdenesTrabajo { get; set; }
        public DbSet<ServicioOrden> ServiciosOrden { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Necesario para Identity

            // Tus configuraciones existentes...
            modelBuilder.Entity<ProductoVenta>()
                   .Property(p => p.Precio)
                   .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Venta>()
                .Property(v => v.Descuento)
                .HasColumnType("decimal(5,2)");

            // Configuraciones para Orden de Trabajo
            modelBuilder.Entity<OrdenTrabajo>()
                .Property(o => o.TotalServicios)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrdenTrabajo>()
                .Property(o => o.PendientePagar)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrdenTrabajo>()
                .Property(o => o.Adelanto)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ServicioOrden>()
                .Property(s => s.Precio)
                .HasColumnType("decimal(18,2)");

            // Configuración de la relación
            modelBuilder.Entity<ServicioOrden>()
                .HasOne(s => s.OrdenTrabajo)
                .WithMany(o => o.Servicios)
                .HasForeignKey(s => s.OrdenTrabajoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}