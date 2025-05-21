// Services/VentaService.cs
using LavaderoMotos.Data;
using LavaderoMotos.Models;
using LavaderoMotos.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LavaderoMotos.Services
{
    public class VentaService : IVentaService
    {
        private readonly ApplicationDbContext _context;

        public VentaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Crear(Venta venta)
        {
            // Asegurar que la fecha esté establecida
            venta.Fecha = DateTime.Now;

            // Asegurar que los productos estén vinculados
            if (venta.Productos != null)
            {
                foreach (var producto in venta.Productos)
                {
                    producto.VentaId = venta.Id;
                    producto.Venta = venta;
                }
            }

            _context.Ventas.Add(venta);
            _context.SaveChanges();
        }
        public List<Venta> ObtenerTodas() =>
                    _context.Ventas.Include(v => v.Productos).ToList();

        public Venta? ObtenerPorId(int id) => // Nota el ? para indicar que puede ser null
      _context.Ventas.Include(v => v.Productos).FirstOrDefault(v => v.Id == id);

        public List<Venta> FiltrarPorFecha(DateTime fecha) =>
            _context.Ventas.Include(v => v.Productos)
                .Where(v => v.Fecha.Date == fecha.Date).ToList();

        public void Actualizar(Venta venta)
        {
            var existente = _context.Ventas.Include(v => v.Productos)
                .FirstOrDefault(v => v.Id == venta.Id);

            if (existente != null)
            {
                // Actualizar propiedades básicas
                existente.Placa = venta.Placa;
                existente.Kilometraje = venta.Kilometraje;

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
                            productoExistente.Precio = Math.Round(producto.Precio, 2);
                        }
                    }
                    else // Nuevo producto
                    {
                        producto.Precio = Math.Round(producto.Precio, 2);
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
            }
        }
        public void Eliminar(int id)
        {
            // Cargar la venta con sus productos relacionados
            var venta = _context.Ventas
                .Include(v => v.Productos)
                .FirstOrDefault(v => v.Id == id);

            if (venta != null)
            {
                // Primero eliminar los productos relacionados
                _context.ProductosVenta.RemoveRange(venta.Productos);

                // Luego eliminar la venta
                _context.Ventas.Remove(venta);

                _context.SaveChanges();
            }
        }

        public Producto? ObtenerProductoInventario(string nombreProducto)
        {
            return _context.Productos
                .FirstOrDefault(p => p.Nombre.ToLower() == nombreProducto.ToLower());
        }


        public void CrearConControlInventario(Venta venta)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using var transaction = _context.Database.BeginTransaction();

                try
                {
                    venta.Fecha = DateTime.Now;

                    // Validar inventario
                    foreach (var productoVenta in venta.Productos)
                    {
                        var productoInventario = _context.Productos
                            .FirstOrDefault(p => p.Nombre.ToLower() == productoVenta.Producto.ToLower());

                        if (productoInventario == null)
                            throw new Exception($"Producto {productoVenta.Producto} no encontrado en inventario");

                        if (productoInventario.Cantidad < productoVenta.Cantidad)
                            throw new Exception($"Stock insuficiente para {productoVenta.Producto}");
                    }

                    // Actualizar inventario
                    foreach (var productoVenta in venta.Productos)
                    {
                        var productoInventario = _context.Productos
                            .FirstOrDefault(p => p.Nombre.ToLower() == productoVenta.Producto.ToLower());

                        productoInventario.Cantidad -= productoVenta.Cantidad;
                        _context.Productos.Update(productoInventario);
                    }

                    // Crear la venta (sin productos primero)
                    _context.Ventas.Add(venta);
                    _context.SaveChanges();

                    // Asignar VentaId a los productos y asegurar que no tengan ID asignado
                    foreach (var producto in venta.Productos)
                    {
                        producto.VentaId = venta.Id;
                        producto.Id = 0; // Asegurar que el ID sea 0 para que la BD lo autoincremente
                        _context.ProductosVenta.Add(producto);
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Error al crear la venta con control de inventario", ex);
                }
            });
        }



        public List<Venta> FiltrarPorPlaca(string placa) =>
            _context.Ventas.Include(v => v.Productos)
                .Where(v => v.Placa.Contains(placa))
                .ToList();
    }

}