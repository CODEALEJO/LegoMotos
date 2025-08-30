using LavaderoMotos.Data;
using LavaderoMotos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LavaderoMotos.Controllers
{
    [Authorize]
    public class OrdenTrabajoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdenTrabajoController> _logger;

        public OrdenTrabajoController(ApplicationDbContext context, ILogger<OrdenTrabajoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: OrdenTrabajo
        public async Task<IActionResult> Index()
        {
            try
            {
                var ordenes = await _context.OrdenesTrabajo
                    .Include(o => o.Servicios)
                    .OrderByDescending(o => o.FechaIngreso)
                    .ToListAsync();
                return View(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las órdenes de trabajo");
                TempData["ErrorMessage"] = "Error al cargar las órdenes de trabajo.";
                return View(new List<OrdenTrabajo>());
            }
        }

        // GET: OrdenTrabajo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var ordenTrabajo = await _context.OrdenesTrabajo
                    .Include(o => o.Servicios) // Esto es crucial
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (ordenTrabajo == null)
                {
                    return NotFound();
                }

                return View(ordenTrabajo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los detalles de la orden de trabajo");
                TempData["ErrorMessage"] = "Error al cargar los detalles de la orden de trabajo.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: OrdenTrabajo/Create
        public IActionResult Create()
        {
            try
            {
                var model = new OrdenTrabajoViewModel
                {
                    Orden = new OrdenTrabajo(),
                    Servicios = new List<ServicioViewModel>
                    {
                        new ServicioViewModel() // Servicio inicial vacío
                    }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al preparar la creación de orden de trabajo");
                TempData["ErrorMessage"] = "Error al preparar la creación de orden de trabajo.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: OrdenTrabajo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrdenTrabajoViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Crear la orden de trabajo
                    var orden = model.Orden;
                    orden.FechaIngreso = DateTime.Now;

                    // Calcular total de servicios
                    orden.TotalServicios = model.Servicios
                        .Where(s => !string.IsNullOrEmpty(s.Descripcion))
                        .Sum(s => s.Precio);

                    orden.PendientePagar = orden.TotalServicios - (orden.Adelanto ?? 0);

                    // Agregar servicios a la orden
                    foreach (var servicioVm in model.Servicios.Where(s => !string.IsNullOrEmpty(s.Descripcion)))
                    {
                        var servicio = new ServicioOrden
                        {
                            Descripcion = servicioVm.Descripcion,
                            Precio = servicioVm.Precio,
                            Completado = servicioVm.Completado
                        };
                        orden.Servicios.Add(servicio);
                    }

                    _context.OrdenesTrabajo.Add(orden);
                    await _context.SaveChangesAsync(); // SOLO UNA LLAMADA

                    TempData["SuccessMessage"] = "Orden de trabajo creada correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la orden de trabajo");
                TempData["ErrorMessage"] = $"Error al crear la orden de trabajo: {ex.Message}";
                return View(model);
            }
        }


        // GET: OrdenTrabajo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var ordenTrabajo = await _context.OrdenesTrabajo
                    .Include(o => o.Servicios)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (ordenTrabajo == null)
                {
                    return NotFound();
                }

                // En el método Edit
                var model = new OrdenTrabajoViewModel
                {
                    Orden = ordenTrabajo,
                    Servicios = ordenTrabajo.Servicios.Select(s => new ServicioViewModel
                    {
                        Id = s.Id, // Incluir el ID
                        Descripcion = s.Descripcion,
                        Precio = s.Precio,
                        Completado = s.Completado
                    }).ToList()
                };

                // Asegurar que haya al menos un servicio
                if (!model.Servicios.Any())
                {
                    model.Servicios.Add(new ServicioViewModel());
                }

                return View("Create", model); // Usamos la misma vista Create para Edit
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la orden de trabajo para editar");
                TempData["ErrorMessage"] = "Error al cargar la orden de trabajo para editar.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrdenTrabajoViewModel model)
        {
            if (id != model.Orden.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var ordenExistente = await _context.OrdenesTrabajo
                        .Include(o => o.Servicios)
                        .FirstOrDefaultAsync(o => o.Id == id);

                    if (ordenExistente == null)
                    {
                        return NotFound();
                    }

                    // Actualizar datos de la orden
                    ordenExistente.NombreVehiculo = model.Orden.NombreVehiculo;
                    ordenExistente.Placa = model.Orden.Placa;
                    ordenExistente.Kilometraje = model.Orden.Kilometraje;
                    ordenExistente.Adelanto = model.Orden.Adelanto;
                    ordenExistente.Estado = model.Orden.Estado;

                    // Calcular nuevos totales
                    ordenExistente.TotalServicios = model.Servicios
                        .Where(s => !string.IsNullOrEmpty(s.Descripcion))
                        .Sum(s => s.Precio);

                    ordenExistente.PendientePagar = ordenExistente.TotalServicios - (ordenExistente.Adelanto ?? 0);

                    // Eliminar servicios existentes
                    _context.ServiciosOrden.RemoveRange(ordenExistente.Servicios);

                    // Agregar nuevos servicios
                    foreach (var servicioVm in model.Servicios.Where(s => !string.IsNullOrEmpty(s.Descripcion)))
                    {
                        var servicio = new ServicioOrden
                        {
                            Descripcion = servicioVm.Descripcion,
                            Precio = servicioVm.Precio,
                            Completado = servicioVm.Completado,
                            OrdenTrabajoId = id
                        };
                        ordenExistente.Servicios.Add(servicio);
                    }

                    await _context.SaveChangesAsync(); // SOLO UNA LLAMADA

                    TempData["SuccessMessage"] = "Orden de trabajo actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                return View("Create", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar la orden de trabajo");
                TempData["ErrorMessage"] = $"Error al editar la orden de trabajo: {ex.Message}";
                return View("Create", model);
            }
        }

        // GET: OrdenTrabajo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var ordenTrabajo = await _context.OrdenesTrabajo
                    .Include(o => o.Servicios)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (ordenTrabajo == null)
                {
                    return NotFound();
                }

                return View(ordenTrabajo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la orden de trabajo para eliminar");
                TempData["ErrorMessage"] = "Error al cargar la orden de trabajo para eliminar.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: OrdenTrabajo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var ordenTrabajo = await _context.OrdenesTrabajo
                    .Include(o => o.Servicios)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (ordenTrabajo != null)
                {
                    // Eliminar servicios primero
                    _context.ServiciosOrden.RemoveRange(ordenTrabajo.Servicios);
                    _context.OrdenesTrabajo.Remove(ordenTrabajo);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Orden de trabajo eliminada correctamente.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No se encontró la orden de trabajo a eliminar.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la orden de trabajo");
                TempData["ErrorMessage"] = $"Error al eliminar la orden de trabajo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Método para agregar fila de servicio dinámicamente
        public IActionResult AgregarServicio()
        {
            return PartialView("_ServicioPartial", new ServicioViewModel());
        }

        private bool OrdenTrabajoExists(int id)
        {
            return _context.OrdenesTrabajo.Any(e => e.Id == id);
        }

        // GET: OrdenTrabajo/Imprimir/5
        public async Task<IActionResult> Imprimir(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var ordenTrabajo = await _context.OrdenesTrabajo
                    .Include(o => o.Servicios)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (ordenTrabajo == null)
                {
                    return NotFound();
                }

                return View(ordenTrabajo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la orden de trabajo para imprimir");
                TempData["ErrorMessage"] = "Error al cargar la orden de trabajo para imprimir.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}