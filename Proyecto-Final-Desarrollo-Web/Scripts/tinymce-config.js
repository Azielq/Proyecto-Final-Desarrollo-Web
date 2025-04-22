/**
 * Configuración del editor TinyMCE para FarmaU
 */
$(document).ready(function () {
    if (typeof tinymce !== 'undefined') {
        tinymce.init({
            selector: '.tinymce',
            language: 'es_MX',
            height: 300,
            branding: false,
            plugins: 'advlist autolink lists link image charmap print preview hr anchor pagebreak searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking save table contextmenu directionality emoticons template paste textcolor',
            toolbar: [
                'undo redo | styleselect | bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | outdent indent | removeformat | help',
                'fontselect fontsizeselect | forecolor backcolor | bullist numlist | link image media | table hr | charmap emoticons | code fullscreen'
            ],
            content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }',
            branding: false,
            elementpath: false,
            resize: true,
            paste_data_images: true,
            images_upload_url: '/ImagenesProducto/SubirImagenTinyMCE', // Ruta para subir imágenes desde TinyMCE
            images_upload_base_path: '/',
            images_upload_credentials: true,
            image_dimensions: false,
            image_class_list: [
                { title: 'Responsive', value: 'img-fluid' },
                { title: 'Thumbnail', value: 'img-thumbnail' },
                { title: 'Normal', value: '' }
            ]
        });

        // Configuración específica para campos de ingredientes
        tinymce.init({
            selector: '.tinymce-ingredientes',
            language: 'es_MX',
            height: 250,
            branding: false,
            plugins: 'advlist autolink lists link image charmap print preview hr anchor pagebreak searchreplace wordcount visualblocks visualchars code fullscreen insertdatetime media nonbreaking save table contextmenu directionality emoticons template paste textcolor',
            toolbar: [
                'undo redo | styleselect | bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | outdent indent | removeformat | help',
                'fontselect fontsizeselect | forecolor backcolor | bullist numlist | link image media | table hr | charmap emoticons | code fullscreen'
            ],
            toolbar: 'undo redo | formatselect | ' +
                'bold italic | bullist numlist | ' +
                'removeformat | help',
            content_style: 'body { font-family:Helvetica,Arial,sans-serif; font-size:14px }',
            branding: false,
            elementpath: false,
            resize: true
        });
    }
});