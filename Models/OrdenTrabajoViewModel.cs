using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LavaderoMotos.Models;
namespace LavaderoMotos.Models
{
    public class OrdenTrabajoViewModel
    {
        public OrdenTrabajo Orden { get; set; } = new OrdenTrabajo();
        public List<ServicioViewModel> Servicios { get; set; } = new List<ServicioViewModel>();
    }
}

