// Logística Centro País (LCP) - Interactive JavaScript & AJAX Handlers

// 1. Agregar Producto al Carrito vía AJAX
function agregarAlCarrito(productoId, cantidad = 1) {
    $.ajax({
        url: '/Carrito/Agregar',
        type: 'POST',
        data: {
            productoId: productoId,
            cantidad: cantidad
        },
        success: function (response) {
            if (response.success) {
                // Actualizar badge en navbar
                const badge = $('#navbar-cart-count');
                if (badge.length) {
                    badge.text(response.totalItems);
                    badge.removeClass('d-none');
                }

                Swal.fire({
                    icon: 'success',
                    title: '¡Agregado al Carrito!',
                    text: response.message,
                    showCancelButton: true,
                    confirmButtonText: '<i class="fas fa-shopping-cart me-1"></i> Ir al Carrito',
                    cancelButtonText: 'Seguir Comprando',
                    confirmButtonColor: '#0d6efd',
                    cancelButtonColor: '#6c757d'
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = '/Carrito/Index';
                    }
                });
            } else {
                Swal.fire({
                    icon: 'warning',
                    title: 'Aviso',
                    text: response.message,
                    confirmButtonColor: '#dc3545'
                });
            }
        },
        error: function () {
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'No se pudo comunicar con el servidor para agregar el producto.',
                confirmButtonColor: '#dc3545'
            });
        }
    });
}

// 2. Búsqueda Sensitiva en Tiempo Real (AJAX Live Search)
function configurarBusquedaSensitiva(inputId, resultadosContainerId) {
    const input = $('#' + inputId);
    const container = $('#' + resultadosContainerId);
    let debounceTimer;

    if (!input.length || !container.length) return;

    input.on('input', function () {
        const query = $(this).val().trim();
        clearTimeout(debounceTimer);

        if (query.length < 2) {
            container.empty().addClass('d-none');
            return;
        }

        debounceTimer = setTimeout(function () {
            $.ajax({
                url: '/Catalogo/BuscarSensitivo',
                type: 'GET',
                data: { q: query },
                success: function (data) {
                    container.empty();

                    if (data && data.length > 0) {
                        data.forEach(function (item) {
                            const html = `
                                <a href="/Catalogo/Detalle/${item.id}" class="search-item d-flex align-items-center p-2 text-decoration-none border-bottom">
                                    <img src="${item.imagenUrl}" alt="${item.nombre}" class="rounded me-3" style="width: 45px; height: 45px; object-fit: cover;">
                                    <div class="flex-grow-1">
                                        <div class="fw-bold text-dark text-truncate" style="max-width: 320px;">${item.nombre}</div>
                                        <div class="small text-muted">${item.categoria} - Stock: ${item.stock} u.</div>
                                    </div>
                                    <div class="fw-bold text-primary fs-6 ms-2">$${item.precio.toLocaleString('es-AR', { minimumFractionDigits: 2 })}</div>
                                </a>
                            `;
                            container.append(html);
                        });
                        container.removeClass('d-none');
                    } else {
                        container.html('<div class="p-3 text-muted small text-center">No se encontraron productos coincidentes.</div>').removeClass('d-none');
                    }
                }
            });
        }, 300);
    });

    // Cerrar sugerencias al hacer clic fuera
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#' + inputId + ', #' + resultadosContainerId).length) {
            container.addClass('d-none');
        }
    });
}

// 3. Dropdowns Anidados (Provincias -> Nodos de Distribución LCP)
function configurarDropdownNodos(provinciaSelectId, nodoSelectId, tarifaDisplayId, totalDisplayId, subtotalVal) {
    const provSelect = $('#' + provinciaSelectId);
    const nodoSelect = $('#' + nodoSelectId);

    if (!provSelect.length || !nodoSelect.length) return;

    provSelect.on('change', function () {
        const provId = $(this).val();
        nodoSelect.empty().append('<option value="">Cargando nodos...</option>');

        if (!provId) return;

        $.ajax({
            url: '/Nodos/GetNodosPorProvincia',
            type: 'GET',
            data: { provinciaId: provId },
            success: function (data) {
                nodoSelect.empty();
                if (data && data.length > 0) {
                    data.forEach(function (nodo) {
                        nodoSelect.append(`<option value="${nodo.id}" data-tarifa="${nodo.tarifa}" data-tiempo="${nodo.tiempoHoras}">${nodo.nombre}</option>`);
                    });

                    // Actualizar tarifa del primer nodo
                    actualizarTarifaTotal();
                } else {
                    nodoSelect.append('<option value="">No hay nodos activos en esta provincia</option>');
                }
            }
        });
    });

    nodoSelect.on('change', function () {
        actualizarTarifaTotal();
    });

    function actualizarTarifaTotal() {
        const selectedOption = nodoSelect.find('option:selected');
        const tarifa = parseFloat(selectedOption.data('tarifa')) || 2500;

        if (tarifaDisplayId) {
            $('#' + tarifaDisplayId).text('$' + tarifa.toLocaleString('es-AR', { minimumFractionDigits: 2 }));
        }

        if (totalDisplayId && subtotalVal) {
            const nuevoTotal = subtotalVal + tarifa;
            $('#' + totalDisplayId).text('$' + nuevoTotal.toLocaleString('es-AR', { minimumFractionDigits: 2 }));
        }
    }
}
