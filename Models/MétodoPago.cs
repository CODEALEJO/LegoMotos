using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace LavaderoMotos.Models
{
    public enum TipoMetodoPago
    {
        Efectivo,
        Transferencia,
        Credito
    }

    public class MetodoPago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public TipoMetodoPago Tipo { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El valor debe ser mayor que cero")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        public int VentaId { get; set; }

        [ForeignKey("VentaId")]
        public Venta? Venta { get; set; }

        [NotMapped]
        public string ValorFormateado => Valor.ToString("N0", CultureInfo.InvariantCulture);
    }
}