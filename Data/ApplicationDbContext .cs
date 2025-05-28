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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Necesario para Identity

            modelBuilder.Entity<ProductoVenta>()
                   .Property(p => p.Precio)
                   .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Venta>()
                .Property(v => v.Descuento)
                .HasColumnType("decimal(5,2)");
        }
    }
}