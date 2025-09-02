using LavaderoMotos.Data;
using LavaderoMotos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LavaderoMotos.Controllers
{
    [Authorize]
    public class CajaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CajaController> _logger;

        public CajaController(ApplicationDbContext context, ILogger<CajaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var cajas = await _context.Cajas
                .OrderByDescending(c => c.FechaApertura)
                .ToListAsync();
            return View(cajas);
        }

        public IActionResult Apertura()
        {
            try
            {
                // Verificar si ya hay una caja abierta
                var cajaAbierta = _context.Cajas.FirstOrDefault(c => c.FechaCierre == null);
                if (cajaAbierta != null)
                {
                    TempData["ErrorMessage"] = "Ya hay una caja abierta. Debe cerrarla antes de abrir una nueva.";
                    return RedirectToAction(nameof(Index));
                }

                // Crear nueva caja con valores por defecto
                var nuevaCaja = new Caja
                {
                    FechaApertura = DateTime.Now,
                    SaldoInicialEfectivo = 0,
                    SaldoFinalEfectivo = 0,
                    SaldoFinalTransferencia = 0,
                    UsuarioApertura = User.Identity?.Name ?? "Sistema"
                };

                return View(nuevaCaja);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar apertura de caja");
                TempData["ErrorMessage"] = "Ocurrió un error al preparar la apertura de caja.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apertura([Bind("SaldoInicialEfectivo")] Caja caja)
        {
            try
            {
                // Verificar si ya hay una caja abierta
                var cajaAbierta = await _context.Cajas.FirstOrDefaultAsync(c => c.FechaCierre == null);
                if (cajaAbierta != null)
                {
                    TempData["ErrorMessage"] = "Ya hay una caja abierta. Debe cerrarla antes de abrir una nueva.";
                    return RedirectToAction(nameof(Index));
                }

                // Crear nueva instancia para evitar problemas de binding
                var nuevaCaja = new Caja
                {
                    SaldoInicialEfectivo = caja.SaldoInicialEfectivo,
                    FechaApertura = DateTime.Now,
                    UsuarioApertura = User.Identity?.Name ?? "Sistema",
                    SaldoFinalEfectivo = caja.SaldoInicialEfectivo,
                    SaldoFinalTransferencia = 0
                };

                // Validar manualmente el saldo inicial
                if (nuevaCaja.SaldoInicialEfectivo < 0)
                {
                    ModelState.AddModelError("SaldoInicialEfectivo", "El saldo debe ser mayor o igual a 0");
                    return View(nuevaCaja);
                }

                // Saltar validación del modelo y guardar directamente
                _context.Add(nuevaCaja);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Caja abierta correctamente con saldo inicial: {nuevaCaja.SaldoInicialEfectivo:C}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir caja");
                TempData["ErrorMessage"] = $"Error al abrir caja: {ex.Message}";
                return View(caja);
            }
        }

        public async Task<IActionResult> Cierre(int id)
        {
            var caja = await _context.Cajas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caja == null || caja.FechaCierre != null)
            {
                return NotFound();
            }

            // Calcular totales separando ventas y adelantos
            var movimientosIngresoEfectivo = caja.Movimientos
                .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.FormaPago == FormaPagoMovimiento.Efectivo);

            var movimientosIngresoTransferencia = caja.Movimientos
                .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.FormaPago == FormaPagoMovimiento.Transferencia);

            var resumen = new ResumenCierreCaja
            {
                Caja = caja,
                TotalEfectivoVentas = movimientosIngresoEfectivo
                    .Where(m => m.VentaId != null)
                    .Sum(m => m.Monto),
                TotalEfectivoAdelantos = movimientosIngresoEfectivo
                    .Where(m => m.OrdenTrabajoId != null)
                    .Sum(m => m.Monto),
                TotalTransferenciaVentas = movimientosIngresoTransferencia
                    .Where(m => m.VentaId != null)
                    .Sum(m => m.Monto),
                TotalTransferenciaAdelantos = movimientosIngresoTransferencia
                    .Where(m => m.OrdenTrabajoId != null)
                    .Sum(m => m.Monto),
                TotalEgresosEfectivo = caja.Movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Egreso && m.FormaPago == FormaPagoMovimiento.Efectivo)
                    .Sum(m => m.Monto),
                TotalEgresosTransferencia = caja.Movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Egreso && m.FormaPago == FormaPagoMovimiento.Transferencia)
                    .Sum(m => m.Monto)
            };

            return View(resumen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCierre(int id, decimal saldoFinalEfectivo, decimal saldoFinalTransferencia)
        {
            var caja = await _context.Cajas.FindAsync(id);
            if (caja == null || caja.FechaCierre != null)
            {
                return NotFound();
            }

            caja.FechaCierre = DateTime.Now;
            caja.SaldoFinalEfectivo = saldoFinalEfectivo;
            caja.SaldoFinalTransferencia = saldoFinalTransferencia;
            caja.UsuarioCierre = User.Identity?.Name ?? "Sistema";

            _context.Update(caja);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Caja cerrada correctamente";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CrearGasto(int cajaId)
        {
            var movimiento = new MovimientoCaja
            {
                CajaId = cajaId,
                Fecha = DateTime.Now,
                Tipo = TipoMovimiento.Egreso,
                Usuario = User.Identity?.Name ?? "Sistema"
            };
            return View(movimiento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearGasto(MovimientoCaja movimiento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movimiento);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Gasto registrado correctamente";
                return RedirectToAction(nameof(Detalle), new { id = movimiento.CajaId });
            }
            return View(movimiento);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var caja = await _context.Cajas
                .Include(c => c.Movimientos)
                    .ThenInclude(m => m.Venta)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (caja == null)
            {
                return NotFound();
            }

            return View(caja);
        }
    }

    public class ResumenCierreCaja
    {
        public Caja Caja { get; set; }
        public decimal TotalEfectivoVentas { get; set; }
        public decimal TotalTransferenciaVentas { get; set; }
        public decimal TotalEfectivoAdelantos { get; set; }
        public decimal TotalTransferenciaAdelantos { get; set; }
        public decimal TotalEgresosEfectivo { get; set; }
        public decimal TotalEgresosTransferencia { get; set; }

        public decimal TotalIngresosEfectivo => Caja.SaldoInicialEfectivo + TotalEfectivoVentas + TotalEfectivoAdelantos;
        public decimal TotalEgresosEfectivoTotal => TotalEgresosEfectivo;
        public decimal SaldoTeoricoEfectivo => TotalIngresosEfectivo - TotalEgresosEfectivoTotal;

        public decimal SaldoTeoricoTransferencia => (TotalTransferenciaVentas + TotalTransferenciaAdelantos) - TotalEgresosTransferencia;
    }
}