using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LavaderoMotos.Models;
namespace LavaderoMotos.Models
{
    public class OrdenTrabajoViewModel
    {
        public OrdenTrabajo Orden { get; set; } = new OrdenTrabajo();
        public List<ServicioViewModel> Servicios { get; set; } = new List<ServicioViewModel>();

        [Display(Name = "Método de Pago del Adelanto")]
        public TipoPago? MetodoPagoAdelanto { get; set; }

    }

    public enum TipoPago
    {
        Efectivo = 0,
        Transferencia = 1,
        Credito = 2
    }
}

