using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    public class CategoriaListDTO
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }

    // Vista del administrador (US-04)
    public class CategoriaAdminDTO : CategoriaListDTO
    {
        public string Estado { get; set; } = string.Empty;   // Activa / Inactiva
        public int CantidadEventos { get; set; }
    }

    public class CategoriaCreateDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "La descripción puede tener como máximo 250 caracteres.")]
        public string? Descripcion { get; set; }
    }

    public class CategoriaUpdateDTO : CategoriaCreateDTO
    {
    }
}
