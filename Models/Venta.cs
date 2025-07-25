using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace LavaderoMotos.Models
{
    public class Venta
    {
        public Venta()
        {
            Productos = new List<ProductoVenta>();
            MetodosPago = new List<MetodoPago>();
        }

        public int Id { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Fecha de Venta")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La placa es obligatoria.")]
        [RegularExpression(@"^[A-Z0-9]{1,6}$", ErrorMessage = "La placa debe tener máximo 6 caracteres alfanuméricos en MAYÚSCULAS.")]
        [StringLength(6, ErrorMessage = "La placa no puede exceder 6 caracteres")]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "El kilometraje es obligatorio.")]
        [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser un número positivo.")]
        public int Kilometraje { get; set; }

        public List<ProductoVenta> Productos { get; set; }
        public List<MetodoPago> MetodosPago { get; set; }

        [Range(0, 100, ErrorMessage = "El descuento debe estar entre 0 y 100%")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal Descuento { get; set; } = 0;

        [NotMapped]
        public decimal Subtotal => Productos?.Sum(p => p.Total) ?? 0;

        [NotMapped]
        public decimal Total => Subtotal * (1 - Descuento / 100m);

        [NotMapped]
        public decimal TotalPagado => MetodosPago?.Sum(m => m.Valor) ?? 0;

        [NotMapped]
        public decimal SaldoPendiente => Total - TotalPagado;

        [NotMapped]
        public string SubtotalFormateado => Subtotal.ToString("N0", CultureInfo.InvariantCulture);

        [NotMapped]
        public string TotalFormateado => Total.ToString("N0", CultureInfo.InvariantCulture);

        [NotMapped]
        public string DescuentoFormateado => (Subtotal * Descuento / 100m).ToString("N0", CultureInfo.InvariantCulture);

        [NotMapped]
        public string TotalPagadoFormateado => TotalPagado.ToString("N0", CultureInfo.InvariantCulture);

        [NotMapped]
        public string SaldoPendienteFormateado => SaldoPendiente.ToString("N0", CultureInfo.InvariantCulture);
    }
}