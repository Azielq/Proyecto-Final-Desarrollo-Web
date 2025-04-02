/**
 * Archivo JavaScript global para FarmaU
 */

// Con esto documento cuando se carga el DOM
$(document).ready(function () {
    // Esto inicializa tooltips de Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Esto inicializa popovers de Bootstrap
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Configuración global para SweetAlert2 
    Swal.mixin({
        confirmButtonColor: '#4CAF50', // Verde primario de la aplicación
        cancelButtonColor: '#d33',     // Rojo para cancelar
        reverseButtons: true           // Botón de confirmar a la derecha
    });

    // Función para mostrar/ocultar contraseñas
    $('.toggle-password').on('click', function () {
        // Obtiene el ID del campo de contraseña asociado
        const targetId = $(this).data('for');
        const passwordField = $('#' + targetId);

        // Cambia el tipo de campo entre password y text
        const type = passwordField.attr('type') === 'password' ? 'text' : 'password';
        passwordField.attr('type', type);

        // Cambia el ícono
        $(this).find('i').toggleClass('bi-eye bi-eye-slash');
    });

    // Configuración de validación de cliente para jQuery Validate
    if ($.validator) {
        $.validator.setDefaults({
            errorElement: 'div',
            errorClass: 'invalid-feedback',
            highlight: function (element) {
                $(element)
                    .closest('.form-control, .form-select')
                    .addClass('is-invalid');
            },
            unhighlight: function (element) {
                $(element)
                    .closest('.form-control, .form-select')
                    .removeClass('is-invalid')
                    .addClass('is-valid');
            },
            errorPlacement: function (error, element) {
                if (element.parent('.input-group').length) {
                    error.insertAfter(element.parent());
                } else {
                    error.insertAfter(element);
                }
            }
        });

        // Esto agrega un método personalizado para validar contraseñas seguras
        $.validator.addMethod('passwordSegura',
            function (value, element) {
                return this.optional(element) ||
                    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$/.test(value);
            },
            'La contraseña debe contener al menos una letra minúscula, una mayúscula, un número y un carácter especial.'
        );
    }

    // Esto configura Notyf para notificaciones pequeñas
    if (typeof Notyf !== 'undefined') {
        window.notyf = new Notyf({
            duration: 3000,
            position: {
                x: 'right',
                y: 'top'
            },
            types: [
                {
                    type: 'success',
                    background: '#4CAF50',
                    icon: {
                        className: 'bi bi-check-circle-fill',
                        tagName: 'i'
                    }
                },
                {
                    type: 'error',
                    background: '#d33',
                    icon: {
                        className: 'bi bi-x-circle-fill',
                        tagName: 'i'
                    }
                },
                {
                    type: 'warning',
                    background: '#ff9800',
                    icon: {
                        className: 'bi bi-exclamation-triangle-fill',
                        tagName: 'i'
                    }
                },
                {
                    type: 'info',
                    background: '#2196F3',
                    icon: {
                        className: 'bi bi-info-circle-fill',
                        tagName: 'i'
                    }
                }
            ]
        });
    }

    // Función para cambiar el tamaño de página en la paginación
    window.changePageSize = function (pageSize) {
        var url = new URL(window.location.href);
        url.searchParams.set('page', 1);
        url.searchParams.set('pageSize', pageSize);
        window.location.href = url.toString();
    };

    // Animación suave para scroll a anclas
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const targetId = this.getAttribute('href');
            if (targetId !== '#' && document.querySelector(targetId)) {
                e.preventDefault();
                document.querySelector(targetId).scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });

    // Esto confirma acciones importantes (crear, editar, registrar) con SweetAlert2
    $('form[data-confirm-submit="true"]').on('submit', function (e) {
        var form = $(this);

        // Si el formulario tiene la clase "skip-confirmation", omite la confirmación
        if (form.hasClass('skip-confirmation')) {
            return true;
        }

        // Previene el envío del formulario predeterminado
        e.preventDefault();

        // Valida el formulario antes de continuar
        if (!form.valid()) {
            return false;
        }

        // Obtiene parámetros de confirmación
        var confirmTitle = form.data('confirm-title') || '¿Estás seguro?';
        var confirmText = form.data('confirm-text') || 'Esta acción no se puede deshacer.';
        var confirmButtonText = form.data('confirm-button-text') || 'Sí, continuar';
        var cancelButtonText = form.data('confirm-cancel-text') || 'Cancelar';

        // Muestra SweetAlert2 para confirmar
        Swal.fire({
            title: confirmTitle,
            html: confirmText,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#4CAF50',
            cancelButtonColor: '#d33',
            confirmButtonText: confirmButtonText,
            cancelButtonText: cancelButtonText
        }).then((result) => {
            if (result.isConfirmed) {
                // Agrega clase para evitar confirmación adicional si el usuario
                // vuelve a hacer clic en enviar después de confirmar
                form.addClass('skip-confirmation');

                // Envia el formulario
                console.log('Enviando formulario...');
                form[0].submit();
            }
        });

        return false;
    });

    // Esto evalua fortaleza de contraseña al escribir
    $('#Password').on('keyup', function () {
        var password = $(this).val();

        // Si el campo está vacío (en modo edición), no muestra la barra
        if (password.length === 0) {
            $('#strengthBar').css('width', '0%');
            $('#strengthBar').removeClass('bg-danger bg-warning bg-info bg-success');
            $('#strengthText').text('No ingresada').css('color', 'inherit');
            return;
        }

        var strength = 0;

        // Verifica longitud
        if (password.length >= 6) strength += 20;

        // Verifica letras minúsculas
        if (password.match(/[a-z]/)) strength += 20;

        // Verifica letras mayúsculas
        if (password.match(/[A-Z]/)) strength += 20;

        // Verifica números
        if (password.match(/[0-9]/)) strength += 20;

        // Verifica caracteres especiales
        if (password.match(/[^a-zA-Z0-9]/)) strength += 20;

        // Actualiza barra de progreso
        $('#strengthBar').css('width', strength + '%');
        $('#strengthBar').removeClass('bg-danger bg-warning bg-info bg-success');

        // Actualiza color y texto según la fortaleza
        if (strength < 40) {
            $('#strengthBar').addClass('bg-danger');
            $('#strengthText').text('Débil').css('color', '#dc3545');
        } else if (strength < 60) {
            $('#strengthBar').addClass('bg-warning');
            $('#strengthText').text('Regular').css('color', '#ffc107');
        } else if (strength < 100) {
            $('#strengthBar').addClass('bg-info');
            $('#strengthText').text('Buena').css('color', '#0dcaf0');
        } else {
            $('#strengthBar').addClass('bg-success');
            $('#strengthText').text('Fuerte').css('color', '#198754');
        }
    });
});

// Función para confirmar acciones importantes con SweetAlert2
function confirmarAccion(titulo, mensaje, textoConfirmar, callbackExito) {
    Swal.fire({
        title: titulo,
        html: mensaje,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: textoConfirmar,
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed && typeof callbackExito === 'function') {
            callbackExito();
        }
    });
}

// Función global para eliminar elementos con confirmación
function eliminarElemento(entidad, id, url, token, callbackExito) {
    confirmarAccion(
        '¿Eliminar ' + entidad + '?',
        '¿Estás seguro de que deseas eliminar este elemento? Esta acción no se puede deshacer.',
        'Sí, eliminar',
        function () {
            $.ajax({
                url: url,
                type: 'POST',
                data: { id: id, __RequestVerificationToken: token },
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            title: '¡Eliminado!',
                            text: 'El elemento ha sido eliminado correctamente.',
                            icon: 'success',
                            timer: 2000,
                            timerProgressBar: true,
                            showConfirmButton: false
                        });

                        if (typeof callbackExito === 'function') {
                            callbackExito(response);
                        }
                    } else {
                        Swal.fire({
                            title: 'Error',
                            text: response.message || 'No se pudo completar la operación.',
                            icon: 'error'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        title: 'Error',
                        text: 'Ocurrió un error al procesar la solicitud.',
                        icon: 'error'
                    });
                }
            });
        }
    );
}

// Función para cambiar el estado de un usuario con confirmación
function cambiarEstadoUsuario(id, nombre, estadoActual) {
    var nuevoEstado = estadoActual === 1 ? 'Inactivo' : 'Activo';
    var colorEstado = estadoActual === 1 ? '#d33' : '#4CAF50';

    Swal.fire({
        title: 'Cambiar Estado',
        html: `¿Estás seguro de que deseas cambiar el estado del usuario <strong>${nombre}</strong> a <strong>${nuevoEstado}</strong>?`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, cambiar estado',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Usuarios/ToggleEstado',
                type: 'POST',
                data: { id: id },
                success: function (response) {
                    if (response.success) {
                        Swal.fire({
                            title: '¡Estado Actualizado!',
                            html: `El usuario <strong>${response.userName}</strong> ahora está <strong>${response.estado === 1 ? 'Activo' : 'Inactivo'}</strong>`,
                            icon: 'success',
                            timer: 2000,
                            timerProgressBar: true,
                            showConfirmButton: false
                        });

                        // Si hay una tabla de datos, refresca
                        if ($.fn.DataTable) {
                            $('.dataTable').DataTable().ajax.reload();
                        } else {
                            // Si no hay DataTable, redirige o recarga
                            window.location.reload();
                        }
                    } else {
                        Swal.fire({
                            title: 'Error',
                            text: response.message || 'No se pudo cambiar el estado.',
                            icon: 'error'
                        });
                    }
                },
                error: function () {
                    Swal.fire({
                        title: 'Error',
                        text: 'Ocurrió un error al procesar la solicitud.',
                        icon: 'error'
                    });
                }
            });
        }
    });
}

// Esto es para solucionar el problema de los dropdowns de Bootstrap, para que cuando se haga clic sobre ellos se puedan cerrar
document.addEventListener('DOMContentLoaded', function () {
    // Solución para cerrar los dropdowns al hacer clic en el mismo botón que los abre
    const dropdownToggleElements = document.querySelectorAll('.dropdown-toggle');

    dropdownToggleElements.forEach(function (dropdownToggle) {
        // Variable para rastrear si el menú está abierto
        let isOpen = false;

        dropdownToggle.addEventListener('click', function (e) {
            // Obtiene el menú desplegable asociado a este botón
            const dropdownMenu = this.nextElementSibling;

            // Si el menú está abierto y el usuario hace clic en el mismo botón
            if (isOpen) {
                e.preventDefault();
                e.stopPropagation();

                // Fuerza el cierre del menú usando la API de Bootstrap
                const dropdown = bootstrap.Dropdown.getInstance(this);
                if (dropdown) {
                    dropdown.hide();
                }

                // Alternativamente, si la API de Bootstrap no funciona:
                dropdownMenu.classList.remove('show');
                this.setAttribute('aria-expanded', 'false');

                isOpen = false;
            } else {
                // Si el menú no está abierto, permite el comportamiento predeterminado
                // y marcar como abierto para el próximo clic
                setTimeout(() => {
                    if (this.getAttribute('aria-expanded') === 'true') {
                        isOpen = true;
                    }
                }, 10);
            }
        });

        // Restablece el estado cuando el menú se cierra por cualquier motivo
        // (clic fuera, tecla Escape, etc.)
        document.addEventListener('click', function (e) {
            if (!dropdownToggle.contains(e.target) && !dropdownToggle.nextElementSibling?.contains(e.target)) {
                isOpen = false;
            }
        });

        // También restablece el estado cuando se presiona la tecla Escape
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') {
                isOpen = false;
            }
        });
    });

    // Método alternativo: utiliza el evento 'hide.bs.dropdown' de Bootstrap
    // Este método es más limpio pero depende de los eventos del dropdown de Bootstrap
    document.querySelectorAll('.dropdown').forEach(function (dropdown) {
        dropdown.addEventListener('hide.bs.dropdown', function () {
            // Restablece cualquier estado cuando se cierra el dropdown
            const dropdownToggle = this.querySelector('.dropdown-toggle');
            if (dropdownToggle) {
                dropdownToggle.blur(); // Quita el foco del botón
            }
        });
    });
});


// Solución específica para el toggle, esto funciona junto con los estilos para pantallas pequeñas
$(document).ready(function () {
    // Elementos clave
    var navbarToggler = $('.navbar-toggler');
    var navbarCollapse = $('.navbar-collapse');

    // Variable para mantener el estado
    var menuIsOpen = false;

    // Función para abrir el menú
    function openMenu() {
        navbarCollapse.addClass('show');
        menuIsOpen = true;
        $('body').addClass('menu-open');
    }

    // Función para cerrar el menú
    function closeMenu() {
        navbarCollapse.removeClass('show');
        menuIsOpen = false;
        $('body').removeClass('menu-open');
        navbarToggler.attr('aria-expanded', 'false');
        navbarToggler.addClass('collapsed');
    }

    // Sobrescribe el comportamiento del toggler
    navbarToggler.on('click', function (e) {
        // Previene comportamiento por defecto
        e.preventDefault();
        e.stopPropagation();

        // Verificar estado actual
        if (menuIsOpen) {
            // Si está abierto, cerrarlo
            closeMenu();
        } else {
            // Si está cerrado, abrirlo
            openMenu();
        }
    });

    // Cierra al hacer clic en elementos del menú desplegable
    $('.dropdown-item').on('click', function () {
        if (menuIsOpen) {
            closeMenu();
        }
    });

    // Cierra al hacer clic fuera del menú
    $(document).on('click', function (e) {
        if (menuIsOpen &&
            !$(e.target).closest('.navbar-collapse').length &&
            !$(e.target).closest('.navbar-toggler').length) {
            closeMenu();
        }
    });

    // Previene cierre al hacer clic dentro del menú
    navbarCollapse.on('click', function (e) {
        e.stopPropagation();
    });

    // Detecta cambios en la clase para mantener sincronizado el estado
    var observer = new MutationObserver(function (mutations) {
        mutations.forEach(function (mutation) {
            if (mutation.attributeName === 'class') {
                var hasShowClass = navbarCollapse.hasClass('show');
                menuIsOpen = hasShowClass;

                if (hasShowClass) {
                    $('body').addClass('menu-open');
                    navbarToggler.attr('aria-expanded', 'true');
                    navbarToggler.removeClass('collapsed');
                } else {
                    $('body').removeClass('menu-open');
                    navbarToggler.attr('aria-expanded', 'false');
                    navbarToggler.addClass('collapsed');
                }
            }
        });
    });

    // Inicia observación de cambios en el menú
    observer.observe(navbarCollapse[0], { attributes: true });
});