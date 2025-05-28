using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace LavaderoMotos.Models
{
    public class ProductoVenta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio")]
        public string Producto { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
        public int Cantidad { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public int VentaId { get; set; }

        [ForeignKey("VentaId")]
        public Venta? Venta { get; set; }

        // Modificar el cálculo del Total para manejar redondeo consistente
        [NotMapped]
        public decimal Total => Math.Round(Cantidad * Precio, 2, MidpointRounding.AwayFromZero);


        [NotMapped]
        public string TotalFormateado => Total.ToString("N0", CultureInfo.InvariantCulture);

        [NotMapped]
        public string PrecioFormateado => Precio.ToString("N0", CultureInfo.InvariantCulture);
    }
}