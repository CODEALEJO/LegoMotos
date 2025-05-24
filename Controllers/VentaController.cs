using LavaderoMotos.Models;
using LavaderoMotos.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LavaderoMotos.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly IVentaService _ventaService;
        private readonly ILogger<VentaController> _logger;

        public VentaController(IVentaService ventaService, ILogger<VentaController> logger)
        {
            _ventaService = ventaService;
            _logger = logger;
        }

        public IActionResult Index(DateTime? fecha, string placa)
        {
            List<Venta> ventas;

            if (fecha.HasValue)
            {
                ventas = _ventaService.FiltrarPorFecha(fecha.Value);
            }
            else if (!string.IsNullOrEmpty(placa))
            {
                ventas = _ventaService.FiltrarPorPlaca(placa);
            }
            else
            {
                ventas = _ventaService.ObtenerTodas();
            }

            return View(ventas);
        }

        public IActionResult Create()
        {
            // Inicializa una nueva venta con la lista de productos vacía
            var venta = new Venta
            {
                Productos = new List<ProductoVenta>() // Asegura que Productos no sea null
            };
            return View(venta);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Venta venta)
        {
            try
            {
                // Normalizar valores antes de validar
                if (venta.Productos != null)
                {
                    foreach (var producto in venta.Productos)
                    {
                        producto.Precio = Math.Round(producto.Precio, 2);
                    }
                }

                if (ModelState.IsValid)
                {
                    // Eliminar productos duplicados
                    venta.Productos = venta.Productos?
                        .GroupBy(p => p.Producto)
                        .Select(g => new ProductoVenta
                        {
                            Producto = g.Key,
                            Cantidad = g.Sum(p => p.Cantidad),
                            Precio = Math.Round(g.First().Precio, 2)
                        })
                        .ToList();

                    _ventaService.CrearConControlInventario(venta);
                    TempData["SuccessMessage"] = "Venta creada exitosamente!";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Por favor corrija los errores en el formulario";
                return View(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                TempData["ErrorMessage"] = $"Error al crear la venta: {ex.GetBaseException().Message}";
                return View(venta);
            }
        }


        public IActionResult Edit(int id)
        {
            var venta = _ventaService.ObtenerPorId(id);
            if (venta == null)
            {
                _logger.LogWarning($"Venta con ID {id} no encontrada");
                return NotFound();
            }
            return View(venta);
        }

        [HttpPost]
        public IActionResult Edit(Venta venta)
        {
            try
            {
                // Normalizar valores
                foreach (var producto in venta.Productos)
                {
                    producto.Precio = Math.Round(producto.Precio, 2);
                }

                if (ModelState.IsValid)
                {
                    _ventaService.Actualizar(venta);
                    TempData["SuccessMessage"] = "Venta actualizada correctamente!";
                    return RedirectToAction("Index");
                }

                return View(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar venta");
                TempData["ErrorMessage"] = "Error al actualizar la venta";
                return View(venta);
            }
        }

        public IActionResult Delete(int id)
        {
            var venta = _ventaService.ObtenerPorId(id);
            if (venta == null)
            {
                _logger.LogWarning($"Venta con ID {id} no encontrada para eliminar");
                return NotFound();
            }
            return View(venta);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var venta = _ventaService.ObtenerPorId(id);
                if (venta == null)
                {
                    _logger.LogWarning($"Venta con ID {id} no encontrada");
                    return NotFound();
                }

                _ventaService.Eliminar(id);
                _logger.LogInformation($"Venta ID {id} eliminada correctamente");

                TempData["SuccessMessage"] = "La venta se ha eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar venta ID {id}");
                TempData["ErrorMessage"] = "Ocurrió un error al eliminar la venta.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Print(int id)
        {
            var venta = _ventaService.ObtenerPorId(id);
            if (venta == null)
            {
                _logger.LogWarning($"Venta con ID {id} no encontrada para imprimir");
                return NotFound();
            }

            return View("Print", venta);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}