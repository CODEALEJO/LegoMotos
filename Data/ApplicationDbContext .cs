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

            // Tus configuraciones personalizadas
            modelBuilder.Entity<ProductoVenta>()
                .HasOne(p => p.Venta)
                .WithMany(v => v.Productos)
                .HasForeignKey(p => p.VentaId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ProductoVenta>()
                .Property(p => p.Precio)
                .ValueGeneratedOnAdd()
                .HasColumnType("decimal(18,2)");
        }
    }
}