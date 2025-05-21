using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavaderoMotos.Models
{
    // En Models/ProductoVenta.cs
    public class ProductoVenta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Esta línea es crucial
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        public string Producto { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public int VentaId { get; set; }

        [ForeignKey("VentaId")]
        public Venta? Venta { get; set; } // Hacer nullable para evitar warning

        [NotMapped]
        public decimal Total => Cantidad * Precio;

        [NotMapped]
        public string TotalFormateado => Total.ToString("N0", new System.Globalization.CultureInfo("es-CO"));

        [NotMapped]
        public string PrecioFormateado => Precio.ToString("N0", new System.Globalization.CultureInfo("es-CO"));

        [NotMapped]
        public int StockDisponible { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Cantidad > StockDisponible)
            {
                yield return new ValidationResult(
                    $"No hay suficiente stock. Solo hay {StockDisponible} unidades disponibles",
                    new[] { nameof(Cantidad) });
            }
        }
    }
}