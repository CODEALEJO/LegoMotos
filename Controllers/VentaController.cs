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
            IQueryable<Venta> query = _context.Ventas
                .Include(v => v.Productos)
                .Include(v => v.MetodosPago)
                .Include(v => v.Caja);

            if (fecha.HasValue)
            {
                query = query.Where(v => v.Fecha.Date == fecha.Value.Date);
            }
            else if (!string.IsNullOrEmpty(placa))
            {
                query = query.Where(v => v.Placa.Contains(placa));
            }

            // Ordenar de más reciente a más antigua
            query = query.OrderByDescending(v => v.Fecha);

            var ventas = query.ToList();
            return View(ventas);
        }

        public IActionResult Create()
        {
            // Verificar si hay caja abierta pero no bloquear la creación
            var cajaAbierta = _context.Cajas.FirstOrDefault(c => c.FechaCierre == null);

            if (cajaAbierta == null)
            {
                TempData["WarningMessage"] = "No hay caja abierta. La venta no se asociará a ninguna caja.";
            }

            return View("CreateEdit", new Venta
            {
                Fecha = DateTime.Now,
                Productos = new List<ProductoVenta>(),
                CajaId = cajaAbierta.Id // Asignar null si no hay caja abierta
            });
        }

        public IActionResult Edit(int id)
        {
            var venta = _context.Ventas
                .Include(v => v.Productos)
                .Include(v => v.MetodosPago)
                .Include(v => v.Caja)
                .FirstOrDefault(v => v.Id == id);

            if (venta == null)
            {
                _logger.LogWarning($"Venta con ID {id} no encontrada");
                return NotFound();
            }

            // Verificar si la caja está cerrada
            if (venta.Caja?.FechaCierre != null)
            {
                TempData["ErrorMessage"] = "No se puede editar una venta de una caja cerrada.";
                return RedirectToAction("Index", "Venta");
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
        public async Task<IActionResult> Edit(int id, Venta venta)
        {
            if (id != venta.Id)
            {
                return NotFound();
            }
            return await GuardarVenta(venta, isEdit: true);
        }


        private async Task<IActionResult> GuardarVenta(Venta venta, bool isEdit)
        {
            try
            {
                // Verificar si hay caja abierta (pero no bloquear si no hay)
                var cajaAbierta = await _context.Cajas.FirstOrDefaultAsync(c => c.FechaCierre == null);

                if (venta.Productos == null || venta.Productos.Count == 0)
                {
                    TempData["ErrorMessage"] = "Debe agregar al menos un producto";
                    return View("CreateEdit", venta);
                }

                if (venta.MetodosPago == null || venta.MetodosPago.Count == 0)
                {
                    TempData["ErrorMessage"] = "Debe agregar al menos un método de pago";
                    return View("CreateEdit", venta);
                }

                var subtotal = venta.Productos.Sum(p => p.Cantidad * p.Precio);
                var total = subtotal * (1 - venta.Descuento / 100m);
                var totalPagado = venta.MetodosPago.Sum(m => m.Valor);

                if (Math.Abs(totalPagado - total) > 0.01m)
                {
                    TempData["ErrorMessage"] = $"La suma de los métodos de pago ({totalPagado.ToString("N0")}) no coincide con el total de la venta ({total.ToString("N0")})";
                    return View("CreateEdit", venta);
                }

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
                            venta.Productos = venta.Productos
                                .GroupBy(p => p.Producto)
                                .Select(g => new ProductoVenta
                                {
                                    Producto = g.Key,
                                    Cantidad = g.Sum(p => p.Cantidad),
                                    Precio = Math.Round(g.First().Precio, 2, MidpointRounding.AwayFromZero)
                                })
                                .ToList();

                            foreach (var producto in venta.Productos)
                            {
                                var productoInventario = await _context.Productos
                                    .FirstOrDefaultAsync(p => p.Nombre.ToLower() == producto.Producto.ToLower());

                                if (productoInventario == null)
                                    throw new Exception($"Producto '{producto.Producto}' no encontrado");

                                if (isEdit)
                                {
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

                            if (isEdit)
                            {
                                var existente = await _context.Ventas
                                    .Include(v => v.Productos)
                                    .Include(v => v.MetodosPago)
                                    .FirstOrDefaultAsync(v => v.Id == venta.Id);

                                if (existente == null)
                                    throw new Exception("Venta no encontrada para editar");

                                existente.Placa = venta.Placa;
                                existente.Kilometraje = venta.Kilometraje;
                                existente.Descuento = venta.Descuento;
                                existente.Fecha = venta.Fecha;

                                foreach (var producto in venta.Productos)
                                {
                                    if (producto.Id > 0)
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
                                    else
                                    {
                                        existente.Productos.Add(producto);
                                    }
                                }

                                foreach (var metodoPago in venta.MetodosPago)
                                {
                                    if (metodoPago.Id > 0)
                                    {
                                        var metodoExistente = existente.MetodosPago
                                            .FirstOrDefault(m => m.Id == metodoPago.Id);

                                        if (metodoExistente != null)
                                        {
                                            metodoExistente.Tipo = metodoPago.Tipo;
                                            metodoExistente.Valor = metodoPago.Valor;
                                        }
                                    }
                                    else
                                    {
                                        existente.MetodosPago.Add(metodoPago);
                                    }
                                }

                                var idsProductosActualizados = venta.Productos.Select(p => p.Id).ToList();
                                foreach (var productoExistente in existente.Productos.ToList())
                                {
                                    if (!idsProductosActualizados.Contains(productoExistente.Id))
                                    {
                                        _context.ProductosVenta.Remove(productoExistente);
                                    }
                                }

                                var idsMetodosActualizados = venta.MetodosPago.Select(m => m.Id).ToList();
                                foreach (var metodoExistente in existente.MetodosPago.ToList())
                                {
                                    if (!idsMetodosActualizados.Contains(metodoExistente.Id))
                                    {
                                        _context.MetodoPagos.Remove(metodoExistente);
                                    }
                                }

                                _context.Ventas.Update(existente);
                            }
                            else
                            {
                                venta.Fecha = venta.Fecha == default ? DateTime.Now : venta.Fecha;

                                // Asignar caja solo si está abierta
                                if (cajaAbierta != null)
                                {
                                    venta.CajaId = cajaAbierta.Id;
                                }

                                await _context.Ventas.AddAsync(venta);
                            }

                            await _context.SaveChangesAsync();

                            // Registrar movimientos de caja solo si hay caja abierta y es nueva venta
                            if (!isEdit && cajaAbierta != null)
                            {
                                foreach (var metodoPago in venta.MetodosPago)
                                {
                                    var movimiento = new MovimientoCaja
                                    {
                                        CajaId = cajaAbierta.Id,
                                        Fecha = DateTime.Now,
                                        Tipo = TipoMovimiento.Ingreso,
                                        FormaPago = metodoPago.Tipo == TipoMetodoPago.Efectivo ?
                                            FormaPagoMovimiento.Efectivo : FormaPagoMovimiento.Transferencia,
                                        Monto = metodoPago.Valor,
                                        Descripcion = $"Venta #{venta.Id} - {metodoPago.Tipo}",
                                        VentaId = venta.Id,
                                        Usuario = User.Identity?.Name ?? "Sistema"
                                    };

                                    await _context.MovimientoCajas.AddAsync(movimiento);
                                }
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
            var venta = _context.Ventas
                .Include(v => v.Productos)
                .Include(v => v.Caja)
                .FirstOrDefault(v => v.Id == id);

            if (venta == null)
            {
                _logger.LogWarning($"Venta con ID {id} no encontrada para eliminar");
                return NotFound();
            }

            // Verificar si la caja está cerrada
            if (venta.Caja?.FechaCierre != null)
            {
                TempData["ErrorMessage"] = "No se puede eliminar una venta de una caja cerrada.";
                return RedirectToAction("Index");
            }

            return View(venta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Productos)
                .Include(v => v.MetodosPago)
                .Include(v => v.Caja)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
            {
                _logger.LogWarning($"Venta con ID {id} no encontrada");
                return NotFound();
            }

            // Verificar si la caja está cerrada
            if (venta.Caja?.FechaCierre != null)
            {
                TempData["ErrorMessage"] = "No se puede eliminar una venta de una caja cerrada.";
                return RedirectToAction("Index");
            }

            try
            {
                // Restaurar stock de productos
                foreach (var producto in venta.Productos)
                {
                    var productoInventario = await _context.Productos
                        .FirstOrDefaultAsync(p => p.Nombre.ToLower() == producto.Producto.ToLower());

                    if (productoInventario != null)
                    {
                        productoInventario.Cantidad += producto.Cantidad;
                        _context.Productos.Update(productoInventario);
                    }
                }

                // Eliminar movimientos de caja asociados
                var movimientos = await _context.MovimientoCajas
                    .Where(m => m.VentaId == id)
                    .ToListAsync();

                _context.MovimientoCajas.RemoveRange(movimientos);

                // Eliminar la venta
                _context.Ventas.Remove(venta);
                await _context.SaveChangesAsync();

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
            var venta = _context.Ventas
                .Include(v => v.Productos)
                .Include(v => v.MetodosPago)
                .Include(v => v.Caja)
                .FirstOrDefault(v => v.Id == id);

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

            if (precio is decimal decimalValue)
            {
                return decimalValue;
            }

            var precioStr = precio.ToString().Replace(",", ".");

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