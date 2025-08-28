using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LavaderoMotos.Models;

namespace LavaderoMotos.Models
{
    public class ServicioOrden
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción del servicio es requerida")]
        [Display(Name = "Servicio")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Completado")]
        public bool Completado { get; set; }

        [Display(Name = "Precio")]
        [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor o igual a 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        // Foreign key
        public int OrdenTrabajoId { get; set; }
        public virtual OrdenTrabajo OrdenTrabajo { get; set; } = new OrdenTrabajo();

        // Propiedad de solo lectura para formato
        [NotMapped]
        public string PrecioFormateado => Precio.ToString("N0", new System.Globalization.CultureInfo("es-CO"));
    }
}