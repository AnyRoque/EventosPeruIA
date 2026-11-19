namespace EventosPeruIA.CORE.Core.Settings
{
    // Imágenes de portada de los eventos (US-03): el archivo se guarda en la carpeta
    // wwwroot/imagenes/eventos del proyecto API y en la BD solo se guarda su nombre.
    public static class Imagenes
    {
        public const string CarpetaEventos = "imagenes/eventos";
        public const long TamanoMaximo = 2 * 1024 * 1024;   // 2 MB

        // Ruta que recibe el frontend: la concatena con el host de la API
        // Ej.: "/imagenes/eventos/evento1_20261118173500_a1b2c3.jpg"
        public static string? Ruta(string? archivo)
        {
            if (string.IsNullOrWhiteSpace(archivo)) return null;
            if (archivo.StartsWith("http", StringComparison.OrdinalIgnoreCase) || archivo.StartsWith('/')) return archivo;
            return $"/{CarpetaEventos}/{archivo}";
        }
    }
}
