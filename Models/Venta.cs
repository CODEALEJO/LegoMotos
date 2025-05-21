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
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "La placa es obligatoria.")]
        [RegularExpression(@"^[A-Z0-9]{1,6}$", ErrorMessage = "La placa debe tener máximo 6 caracteres alfanuméricos en MAYÚSCULAS.")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El kilometraje es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser un número positivo.")]
        public int Kilometraje { get; set; }

        public List<ProductoVenta> Productos { get; set; }

        [NotMapped]
        public decimal Total => Productos?.Sum(p => p.Total) ?? 0;

        [NotMapped]
        public string TotalFormateado => Total.ToString("N0", new System.Globalization.CultureInfo("es-CO"));
    }
}