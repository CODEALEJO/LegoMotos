using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LavaderoMotos.Models
{
    public class Factura
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }

        public List<Venta> Ventas { get; set; } = new List<Venta>();

        // En Factura.cs
        public decimal Total => Ventas.Sum(v => v.Total);
        [Required]
        public string Cliente { get; set; } = string.Empty; // Inicializado con valor por defecto
    }

}