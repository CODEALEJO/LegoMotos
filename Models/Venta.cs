using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavaderoMotos.Models
{
    // En Models/Venta.cs
    public class Venta
    {
        public Venta()
        {
            Productos = new List<ProductoVenta>(); // Inicializa en el constructor
        }

        public int Id { get; set; }
        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Venta")]
        public DateTime Fecha { get; set; } = DateTime.Now; // Valor por defecto fecha actual
        [Required(ErrorMessage = "La placa es obligatoria.")]
        [RegularExpression(@"^[A-Z0-9]{1,6}$", ErrorMessage = "La placa debe tener máximo 6 caracteres alfanuméricos en MAYÚSCULAS.")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El kilometraje es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser un número positivo.")]
        public int Kilometraje { get; set; }

        public List<ProductoVenta> Productos { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100%")]
        public decimal Descuento { get; set; } = 0; // Valor por defecto 0

        [NotMapped]
        public decimal Subtotal => Productos?.Sum(p => p.Total) ?? 0;

        [NotMapped]
        public decimal Total => Subtotal * (1 - Descuento / 100);

        [NotMapped]
        public string SubtotalFormateado => Subtotal.ToString("N0", new System.Globalization.CultureInfo("es-CO"));

        [NotMapped]
        public string TotalFormateado => Total.ToString("N0", new System.Globalization.CultureInfo("es-CO"));

        [NotMapped]
        public string DescuentoFormateado => (Subtotal * Descuento / 100).ToString("N0", new System.Globalization.CultureInfo("es-CO"));
    }
}