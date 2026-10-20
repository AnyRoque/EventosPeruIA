namespace EventosPeruIA.CORE.Core.DTOs
{
    // Respuesta de los servicios: indica si salió bien, el mensaje para el usuario
    // y el código HTTP que debe devolver el controller.
    public class ResultadoDTO<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Datos { get; set; }
        public int Codigo { get; set; }

        public static ResultadoDTO<T> Ok(T? datos, string mensaje = "", int codigo = 200)
            => new() { Exito = true, Datos = datos, Mensaje = mensaje, Codigo = codigo };

        public static ResultadoDTO<T> Error(int codigo, string mensaje, T? datos = default)
            => new() { Exito = false, Codigo = codigo, Mensaje = mensaje, Datos = datos };
    }

    public class PaginaDTO<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanoPagina { get; set; }
        public int TotalPaginas { get; set; }
    }
}
