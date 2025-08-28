using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavaderoMotos.Models
{
    public class OrdenTrabajo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del vehículo es requerido")]
        [Display(Name = "Vehículo")]
        public string NombreVehiculo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La placa es requerida")]
        public string Placa { get; set; } = string.Empty;

        [Display(Name = "Kilometraje")]
        public int? Kilometraje { get; set; }

        [Display(Name = "Fecha de Ingreso")]
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        [Display(Name = "Adelanto")]
        [Range(0, double.MaxValue, ErrorMessage = "El adelanto debe ser mayor o igual a 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Adelanto { get; set; }

        [Display(Name = "Total Servicios")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalServicios { get; set; }

        [Display(Name = "Pendiente a Pagar")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PendientePagar { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, En Proceso, Completada

        // Relación con los servicios
        public virtual ICollection<ServicioOrden> Servicios { get; set; } = new List<ServicioOrden>();

        // Propiedades de solo lectura para formato
        [NotMapped]
        public string TotalServiciosFormateado => TotalServicios.ToString("N0", new System.Globalization.CultureInfo("es-CO"));
        
        [NotMapped]
        public string AdelantoFormateado => (Adelanto ?? 0).ToString("N0", new System.Globalization.CultureInfo("es-CO"));
        
        [NotMapped]
        public string PendientePagarFormateado => PendientePagar.ToString("N0", new System.Globalization.CultureInfo("es-CO"));
        
        [NotMapped]
        public string FechaIngresoFormateada => FechaIngreso.ToString("dd/MM/yyyy HH:mm");
    }



   
}