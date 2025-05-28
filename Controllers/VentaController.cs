using LavaderoMotos.Models;
using LavaderoMotos.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;

namespace LavaderoMotos.Controllers
{
    [Authorize]
    public class VentaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VentaController> _logger;

        public VentaController(ApplicationDbContext context, ILogger<VentaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index(DateTime? fecha, string placa)
        {
            IQueryable<Venta> query = _context.Ventas.Include(v => v.Productos);

            if (fecha.HasValue)
            {
                query = query.Where(v => v.Fecha.Date == fecha.Value.Date);
            }
            else if (!string.IsNullOrEmpty(placa))
            {
                query = query.Where(v => v.Placa.Contains(placa));
            }

            var ventas = query.ToList();
            return View(ventas);
        }



        public IActionResult Create()
        {
            return View(new Venta { Productos = new List<ProductoVenta>() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venta venta)
        {
            try
            {
                if (venta.Productos == null || venta.Productos.Count == 0)
                {
                    TempData["ErrorMessage"] = "Debe agregar al menos un producto";
                    return View(venta);
                }

                // Normalización de valores
                foreach (var producto in venta.Productos)
                {
                    producto.Precio = NormalizarPrecio(producto.Precio);
                    producto.Cantidad = Convert.ToInt32(producto.Cantidad);
                }

                if (ModelState.IsValid)
                {
                    var executionStrategy = _context.Database.CreateExecutionStrategy();

                    await executionStrategy.ExecuteAsync(async () =>
                    {
                        using var transaction = await _context.Database.BeginTransactionAsync();
                        try
                        {
                            // Consolidar productos duplicados
                            venta.Productos = venta.Productos
                                .GroupBy(p => p.Producto)
                                .Select(g => new ProductoVenta
                                {
                                    Producto = g.Key,
                                    Cantidad = g.Sum(p => p.Cantidad),
                                    Precio = Math.Round(g.First().Precio, 2, MidpointRounding.AwayFromZero)
                                })
                                .ToList();

                            // Validar y actualizar inventario
                            foreach (var producto in venta.Productos)
                            {
                                var productoInventario = await _context.Productos
                                    .FirstOrDefaultAsync(p => p.Nombre.ToLower() == producto.Producto.ToLower());

                                if (productoInventario == null)
                                    throw new Exception($"Producto '{producto.Producto}' no encontrado");

                                if (productoInventario.Cantidad < producto.Cantidad)
                                    throw new Exception($"Stock insuficiente para '{producto.Producto}'");

                                productoInventario.Cantidad -= producto.Cantidad;
                                _context.Productos.Update(productoInventario);
                            }

                            // Guardar la venta
                            venta.Fecha = venta.Fecha == default ? DateTime.Now : venta.Fecha;
                            await _context.Ventas.AddAsync(venta);
                            await _context.SaveChangesAsync();

                            await transaction.CommitAsync();
                            TempData["SuccessMessage"] = "Venta creada exitosamente!";
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            TempData["ErrorMessage"] = $"Error al crear la venta: {ex.Message}";
                            throw; // Re-lanzar la excepción para que la estrategia de reintento la maneje
                        }
                    });

                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Por favor corrija los errores en el formulario";
                return View(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear venta");
                TempData["ErrorMessage"] = $"Error al crear la venta: {ex.Message}";
                return View(venta);
            }
        }

        public IActionResult Edit(int id)
        {
            var venta = _context.Ventas.Include(v => v.Productos).FirstOrDefault(v => v.Id == id);
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
                    producto.Precio = NormalizarPrecio(producto.Precio);
                }

                if (ModelState.IsValid)
                {
                    var existente = _context.Ventas.Include(v => v.Productos)
                        .FirstOrDefault(v => v.Id == venta.Id);

                    if (existente != null)
                    {
                        // Actualizar propiedades básicas
                        existente.Placa = venta.Placa;
                        existente.Kilometraje = venta.Kilometraje;
                        existente.Descuento = venta.Descuento;
                        existente.Fecha = venta.Fecha;

                        // Manejar productos
                        foreach (var producto in venta.Productos)
                        {
                            if (producto.Id > 0) // Producto existente
                            {
                                var productoExistente = existente.Productos
                                    .FirstOrDefault(p => p.Id == producto.Id);

                                if (productoExistente != null)
                                {
                                    productoExistente.Producto = producto.Producto;
                                    productoExistente.Cantidad = producto.Cantidad;
                                    productoExistente.Precio = producto.Precio;
                                }
                            }
                            else // Nuevo producto
                            {
                                existente.Productos.Add(producto);
                            }
                        }

                        // Eliminar productos que no están en la lista actualizada
                        var idsProductosActualizados = venta.Productos.Select(p => p.Id).ToList();
                        foreach (var productoExistente in existente.Productos.ToList())
                        {
                            if (!idsProductosActualizados.Contains(productoExistente.Id))
                            {
                                _context.ProductosVenta.Remove(productoExistente);
                            }
                        }

                        _context.SaveChanges();
                        TempData["SuccessMessage"] = "Venta actualizada correctamente!";
                        return RedirectToAction("Index");
                    }
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
            var venta = _context.Ventas.Include(v => v.Productos).FirstOrDefault(v => v.Id == id);
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
                var venta = _context.Ventas.Include(v => v.Productos).FirstOrDefault(v => v.Id == id);
                if (venta == null)
                {
                    _logger.LogWarning($"Venta con ID {id} no encontrada");
                    return NotFound();
                }

                // Primero eliminar los productos relacionados
                _context.ProductosVenta.RemoveRange(venta.Productos);
                // Luego eliminar la venta
                _context.Ventas.Remove(venta);
                _context.SaveChanges();

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
            var venta = _context.Ventas.Include(v => v.Productos).FirstOrDefault(v => v.Id == id);
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

        private decimal NormalizarPrecio(object precio)
        {
            if (precio == null) return 0;

            // Si ya es decimal, devolverlo directamente
            if (precio is decimal decimalValue)
            {
                return decimalValue;
            }

            // Si es string, parsearlo conservando el formato original
            var precioStr = precio.ToString();

            // Reemplazar comas por puntos si es necesario (para culturas que usan coma como separador decimal)
            precioStr = precioStr.Replace(",", ".");

            if (decimal.TryParse(precioStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return 0;
        }
    }
}