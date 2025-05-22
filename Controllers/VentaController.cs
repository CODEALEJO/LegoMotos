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
                if (ModelState.IsValid)
                {
                    // Eliminar productos duplicados antes de procesar
                    if (venta.Productos != null)
                    {
                        venta.Productos = venta.Productos
                            .GroupBy(p => p.Producto)
                            .Select(g => new ProductoVenta
                            {
                                Producto = g.Key,
                                Cantidad = g.Sum(p => p.Cantidad),
                                Precio = g.First().Precio
                            })
                            .ToList();
                    }

                    // Si no se especificó fecha, usar la actual
                    if (venta.Fecha == default)
                    {
                        venta.Fecha = DateTime.Now;
                    }

                    // Validar que haya al menos un producto
                    if (venta.Productos == null || venta.Productos.Count == 0)
                    {
                        TempData["ErrorMessage"] = "Debe agregar al menos un producto";
                        return View(venta);
                    }

                    // Validar stock
                    var productosConStockInsuficiente = new List<string>();
                    foreach (var productoVenta in venta.Productos)
                    {
                        var productoInventario = _ventaService.ObtenerProductoInventario(productoVenta.Producto);

                        if (productoInventario == null)
                        {
                            TempData["ErrorMessage"] = $"Producto '{productoVenta.Producto}' no encontrado en inventario";
                            return View(venta);
                        }

                        if (productoInventario.Cantidad < productoVenta.Cantidad)
                        {
                            productosConStockInsuficiente.Add($"{productoVenta.Producto} (Stock: {productoInventario.Cantidad}, Pedido: {productoVenta.Cantidad})");
                        }
                    }

                    if (productosConStockInsuficiente.Any())
                    {
                        TempData["ErrorMessage"] = "Stock insuficiente para:<br>" +
                            string.Join("<br>", productosConStockInsuficiente);
                        return View(venta);
                    }

                    // Si todo está bien, crear la venta
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
            if (venta.Productos == null || !venta.Productos.Any())
            {
                ModelState.AddModelError("", "La venta debe tener al menos un producto");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Asegurar que los IDs de productos se mantengan
                    foreach (var producto in venta.Productos)
                    {
                        producto.VentaId = venta.Id;
                    }

                    _ventaService.Actualizar(venta);
                    TempData["SuccessMessage"] = "La venta se ha actualizado correctamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al editar venta");
                    TempData["ErrorMessage"] = "Ocurrió un error al actualizar la venta";
                }
            }

            return View(venta);
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