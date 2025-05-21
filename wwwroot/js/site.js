function eliminarProducto(button) {
    const productoItem = button.closest('.producto-item');
    productoItem.remove();
    
    // Reindexar todos los productos
    const container = document.getElementById("productos-container");
    const items = container.querySelectorAll('.producto-item');
    
    items.forEach((item, index) => {
        // Actualizar los nombres de los inputs
        item.querySelectorAll('[name^="Productos["]').forEach(input => {
            const name = input.name.replace(/Productos\[\d+\]/, `Productos[${index}]`);
            input.name = name;
        });
    });
    
    actualizarTotalVenta();
    productoIndex = items.length; // Actualizar el índice para nuevos productos
}

// Modificar el evento de eliminación en los botones:
// Cambiar onclick="this.parentNode.parentNode.parentNode.remove(); actualizarTotalVenta()"
// por:
onclick="eliminarProducto(this)"


document.querySelector('form').addEventListener('submit', function(e) {
    // Convertir todos los valores de precio al formato correcto
    document.querySelectorAll('.precio-input').forEach(input => {
        const value = input.value.replace(/\./g, '').replace(',', '.');
        input.value = parseFloat(value).toFixed(2);
    });
    
    // Reindexar productos por si hay huecos
    const container = document.getElementById("productos-container");
    const items = container.querySelectorAll('.producto-item');
    items.forEach((item, index) => {
        item.querySelectorAll('[name^="Productos["]').forEach(input => {
            input.name = input.name.replace(/Productos\[\d+\]/, `Productos[${index}]`);
        });
    });
});