using LavaderoMotos.Models;

namespace LavaderoMotos.Services.Interfaces
{
    public interface IVentaService
    {
        // void Crear(Venta venta);
        void CrearConControlInventario(Venta venta);
        List<Venta> ObtenerTodas();
        Venta? ObtenerPorId(int id);
        List<Venta> FiltrarPorFecha(DateTime fecha);
        void Actualizar(Venta venta);
        void Eliminar(int id);
        List<Venta> FiltrarPorPlaca(string placa);
        Producto? ObtenerProductoInventario(string nombreProducto);
    }
}
