using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LavaderoMotos.Models
{
    public class ServicioViewModel
    {
        public string Descripcion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Completado { get; set; }
    }
}

