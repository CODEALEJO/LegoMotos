using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavaderoMotos.Models
{
    public enum EstadoCaja
    {
        Abierta,
        Cerrada
    }

    public class Caja
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de apertura es requerida")]
        [Display(Name = "Fecha de Apertura")]
        public DateTime FechaApertura { get; set; } = DateTime.UtcNow;

        [Display(Name = "Fecha de Cierre")]
        public DateTime? FechaCierre { get; set; }

        [Required(ErrorMessage = "El saldo inicial es obligatorio")]
        [Range(0, double.MaxValue, ErrorMessage = "El saldo debe ser mayor o igual a 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Saldo Inicial en Efectivo")]
        public decimal SaldoInicialEfectivo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Saldo Final en Efectivo")]
        public decimal SaldoFinalEfectivo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Saldo Final en Transferencia")]
        public decimal SaldoFinalTransferencia { get; set; }

        // Cambio crucial: Inicializar con valor por defecto y quitar Required
        [Display(Name = "Usuario que abre")]
        public string UsuarioApertura { get; set; } = "Sistema"; // Valor por defecto

        [Display(Name = "Usuario que cierra")]
        public string? UsuarioCierre { get; set; }

        [NotMapped]
        [Display(Name = "Estado")]
        public EstadoCaja Estado => FechaCierre == null ? EstadoCaja.Abierta : EstadoCaja.Cerrada;

        public ICollection<MovimientoCaja> Movimientos { get; set; } = new List<MovimientoCaja>();
    }
}