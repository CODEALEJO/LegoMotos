// Controllers/FacturaController.cs
using Microsoft.AspNetCore.Mvc;
using LavaderoMotos.Models;
using System.Collections.Generic;
using System.Linq;

namespace LavaderoMotos.Controllers
{
    public class FacturaController : Controller
    {
        // Lista simulada en memoria
        private static List<Factura> _facturas = new List<Factura>();

        public IActionResult Index()
        {
            return View(_facturas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Factura factura)
        {
            factura.Id = _facturas.Count + 1;
            _facturas.Add(factura);
            return RedirectToAction("Index");
        }

        public IActionResult Details(int id)
        {
            var factura = _facturas.FirstOrDefault(f => f.Id == id);
            if (factura == null) return NotFound();
            return View(factura);
        }

        public IActionResult Delete(int id)
        {
            var factura = _facturas.FirstOrDefault(f => f.Id == id);
            if (factura == null) return NotFound();
            return View(factura);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var factura = _facturas.FirstOrDefault(f => f.Id == id);
            if (factura != null)
                _facturas.Remove(factura);
            return RedirectToAction("Index");
        }
    }
}
