using System.ComponentModel.DataAnnotations;

namespace EventosPeruIA.CORE.Core.DTOs
{
    // US-01 Registro de cuenta
    public class RegistroRequestDTO
    {
        [Required(ErrorMessage = "Selecciona qué deseas hacer.")]
        [RegularExpression("^(Comprar|Publicar|Ambas)$", ErrorMessage = "La opción debe ser Comprar, Publicar o Ambas.")]
        public string Perfil { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los nombres deben tener entre 2 y 50 caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+$", ErrorMessage = "Los nombres solo aceptan letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Los apellidos deben tener entre 2 y 50 caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñÜü ]+$", ErrorMessage = "Los apellidos solo aceptan letras y espacios.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [StringLength(100, ErrorMessage = "El correo puede tener como máximo 100 caracteres.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido (ejemplo: usuario@dominio.com).")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El celular es obligatorio.")]
        [RegularExpression(@"^9\d{8}$", ErrorMessage = "El celular debe tener 9 dígitos y empezar con 9.")]
        public string Celular { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        public DateOnly? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El departamento es obligatorio.")]
        [StringLength(50)]
        public string Departamento { get; set; } = string.Empty;

        [Required(ErrorMessage = "El distrito es obligatorio.")]
        [StringLength(50)]
        public string Distrito { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma tu contraseña.")]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;

        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre del organizador debe tener entre 3 y 80 caracteres.")]
        public string? NombreOrganizador { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar los términos y condiciones.")]
        public bool AceptaTerminos { get; set; }
    }

    // US-05 Inicio de sesión
    public class LoginRequestDTO
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido (ejemplo: usuario@dominio.com).")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? NombreOrganizador { get; set; }
        public List<string> Roles { get; set; } = new();
        public string Token { get; set; } = string.Empty;
    }

    // US-01: activar "Quiero publicar eventos" desde el perfil
    public class ActivarOrganizadorDTO
    {
        [Required(ErrorMessage = "El nombre del organizador es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre del organizador debe tener entre 3 y 80 caracteres.")]
        public string NombreOrganizador { get; set; } = string.Empty;
    }

    public class PerfilDTO
    {
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public string Departamento { get; set; } = string.Empty;
        public string Distrito { get; set; } = string.Empty;
        public string? NombreOrganizador { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
