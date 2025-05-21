using Microsoft.AspNetCore.Mvc;
using LavaderoMotos.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using LavaderoMotos.Data;

namespace LavaderoMotos.Controllers
{
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string nombre = null)
        {
            var productos = from p in _context.Productos
                            select p;

            if (!string.IsNullOrEmpty(nombre))
            {
                productos = productos.Where(p => p.Nombre.Contains(nombre));
            }

            return View(await productos.ToListAsync());
        }


        [HttpGet]
        public IActionResult BuscarProducto(string nombre)
        {
            var producto = _context.Productos
                .Where(p => p.Nombre.ToLower().Contains(nombre.ToLower()))
                .OrderBy(p => p.Nombre) // Ordenar para consistencia
                .Select(p => new
                {
                    nombre = p.Nombre,
                    cantidad = p.Cantidad,
                    precioVenta = p.PrecioVenta
                })
                .FirstOrDefault(); // Solo el primer resultado

            return Json(producto);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto agregado correctamente";
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Producto actualizado correctamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Producto eliminado correctamente";
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}