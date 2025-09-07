
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
        Transferencia,
        Credito
    }

    public class MovimientoCaja
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required]
        public TipoMovimiento Tipo { get; set; }

        [Required]
        public FormaPagoMovimiento FormaPago { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }
        [Required]
        public int Cantidad { get; set; } = 1;

        [Required]
        [StringLength(500)]
        public string Descripcion { get; set; }

        public int? VentaId { get; set; }
        public Venta? Venta { get; set; }

        public int? OrdenTrabajoId { get; set; } // Nueva propiedad para relacionar con orden de trabajo
        public OrdenTrabajo? OrdenTrabajo { get; set; } // Nueva propiedad

        [Required]
        public int CajaId { get; set; }
        public Caja Caja { get; set; }

        [Required]
        public string Usuario { get; set; }
    }
}