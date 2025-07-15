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
            return View("CreateEdit", new Venta
            {
                Fecha = DateTime.Now,
                Productos = new List<ProductoVenta>()
            });
        }

        public IActionResult Edit(int id)
        {
            var venta = _context.Ventas.Include(v => v.Productos).FirstOrDefault(v => v.Id == id);
            if (venta == null)
            {
                _logger.LogWarning($"Venta con ID {id} no encontrada");
                return NotFound();
            }
            return View("CreateEdit", venta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venta venta)
        {
            return await GuardarVenta(venta, isEdit: false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Venta venta)
        {
            return await GuardarVenta(venta, isEdit: true);
        }

        private async Task<IActionResult> GuardarVenta(Venta venta, bool isEdit)
        {
            try
            {
                if (venta.Productos == null || venta.Productos.Count == 0)
                {
                    TempData["ErrorMessage"] = "Debe agregar al menos un producto";
                    return View("CreateEdit", venta);
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

                                if (isEdit)
                                {
                                    // Para edición, necesitamos verificar la diferencia con la cantidad original
                                    var productoOriginal = await _context.ProductosVenta
                                        .FirstOrDefaultAsync(p => p.Id == producto.Id);

                                    if (productoOriginal != null)
                                    {
                                        var diferencia = producto.Cantidad - productoOriginal.Cantidad;
                                        if (productoInventario.Cantidad < diferencia)
                                            throw new Exception($"Stock insuficiente para '{producto.Producto}'");

                                        productoInventario.Cantidad -= diferencia;
                                    }
                                    else
                                    {
                                        if (productoInventario.Cantidad < producto.Cantidad)
                                            throw new Exception($"Stock insuficiente para '{producto.Producto}'");

                                        productoInventario.Cantidad -= producto.Cantidad;
                                    }
                                }
                                else
                                {
                                    if (productoInventario.Cantidad < producto.Cantidad)
                                        throw new Exception($"Stock insuficiente para '{producto.Producto}'");

                                    productoInventario.Cantidad -= producto.Cantidad;
                                }

                                _context.Productos.Update(productoInventario);
                            }

                            // Guardar la venta
                            if (isEdit)
                            {
                                var existente = await _context.Ventas
                                    .Include(v => v.Productos)
                                    .FirstOrDefaultAsync(v => v.Id == venta.Id);

                                if (existente == null)
                                    throw new Exception("Venta no encontrada para editar");

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

                                _context.Ventas.Update(existente);
                            }
                            else
                            {
                                venta.Fecha = venta.Fecha == default ? DateTime.Now : venta.Fecha;
                                await _context.Ventas.AddAsync(venta);
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            TempData["SuccessMessage"] = isEdit ?
                                "Venta actualizada exitosamente!" : "Venta creada exitosamente!";
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            TempData["ErrorMessage"] = $"Error al {(isEdit ? "editar" : "crear")} la venta: {ex.Message}";
                            throw;
                        }
                    });

                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Por favor corrija los errores en el formulario";
                return View("CreateEdit", venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al {(isEdit ? "editar" : "crear")} venta");
                TempData["ErrorMessage"] = $"Error al {(isEdit ? "editar" : "crear")} la venta: {ex.Message}";
                return View("CreateEdit", venta);
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


        [HttpGet]
        public async Task<IActionResult> ObtenerProductosDisponibles()
        {
            var productos = await _context.Productos
                .Where(p => p.Cantidad > 0)
                .OrderBy(p => p.Nombre)
                .Select(p => new
                {
                    nombre = p.Nombre,
                    cantidad = p.Cantidad,
                    precioVenta = p.PrecioVenta
                })
                .ToListAsync();

            return Json(productos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPrecioProducto(string nombre)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Nombre == nombre);

            if (producto == null)
            {
                return Json(new { precioVenta = 0 });
            }

            return Json(new { precioVenta = producto.PrecioVenta });
        }
    }
}