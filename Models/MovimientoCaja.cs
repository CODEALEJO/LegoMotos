
// Models/MovimientoCaja.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavaderoMotos.Models
{
    public enum TipoMovimiento
    {
        Ingreso,
        Egreso
    }

    public enum FormaPagoMovimiento
    {
        Efectivo,
        Transferencia
    }

    public class MovimientoCaja
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public TipoMovimiento Tipo { get; set; }

        [Required]
        public FormaPagoMovimiento FormaPago { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; }

        public int? VentaId { get; set; }
        public Venta? Venta { get; set; }

        [Required]
        public int CajaId { get; set; }
        public Caja Caja { get; set; }

        [Required]
        public string Usuario { get; set; }
    }
}