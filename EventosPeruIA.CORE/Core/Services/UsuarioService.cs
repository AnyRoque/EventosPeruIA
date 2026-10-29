using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.CORE.Core.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IJWTService _jwtService;

        public UsuarioService(IUsuarioRepository usuarioRepository, IJWTService jwtService)
        {
            _usuarioRepository = usuarioRepository;
            _jwtService = jwtService;
        }

        // US-01 Registro de cuenta
        public async Task<ResultadoDTO<object>> Registrar(RegistroRequestDTO dto)
        {
            var correo = dto.Correo.Trim().ToLower();

            // Mayor de 18 años
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            if (dto.FechaNacimiento!.Value.AddYears(18) > hoy)
                return ResultadoDTO<object>.Error(400, "Debes ser mayor de edad para registrarte.");

            if (await _usuarioRepository.ExisteCorreo(correo))
                return ResultadoDTO<object>.Error(400, "El correo ya está registrado.");

            var publica = dto.Perfil == "Publicar" || dto.Perfil == "Ambas";
            string? nombreOrganizador = null;

            if (publica)
            {
                if (string.IsNullOrWhiteSpace(dto.NombreOrganizador))
                    return ResultadoDTO<object>.Error(400, "El nombre del organizador es obligatorio.");

                nombreOrganizador = dto.NombreOrganizador.Trim();
                if (await _usuarioRepository.ExisteNombreOrganizador(nombreOrganizador))
                    return ResultadoDTO<object>.Error(400, "El nombre del organizador ya está en uso.");
            }

            var usuario = new Usuario
            {
                Nombre = dto.Nombre.Trim(),
                Apellido = dto.Apellido.Trim(),
                Correo = correo,
                Celular = dto.Celular,
                FechaNacimiento = dto.FechaNacimiento.Value,
                Departamento = dto.Departamento.Trim(),
                Distrito = dto.Distrito.Trim(),
                PasswordHash = dto.Password, // Texto plano, según el alcance visto en clase
                NombreOrganizador = nombreOrganizador,
                Estado = "Activo",
                FechaRegistro = DateTime.Now
            };

            // Toda cuenta es Asistente; si publica eventos, también Organizador
            var roles = new List<int> { Roles.IdAsistente };
            if (publica) roles.Add(Roles.IdOrganizador);

            var ok = await _usuarioRepository.CreateUsuario(usuario, roles);
            if (!ok) return ResultadoDTO<object>.Error(500, "No se pudo crear la cuenta. Inténtalo nuevamente.");

            return ResultadoDTO<object>.Ok(null, "Cuenta creada correctamente.", 201);
        }

        // US-02 Inicio de sesión
        public async Task<ResultadoDTO<LoginResponseDTO>> Login(LoginRequestDTO dto)
        {
            var usuario = await _usuarioRepository.GetUsuarioByCorreo(dto.Correo.Trim().ToLower());

            // Mensaje genérico: no se indica qué dato falló
            if (usuario == null || usuario.PasswordHash != dto.Password)
                return ResultadoDTO<LoginResponseDTO>.Error(401, "Correo o contraseña incorrectos.");

            if (usuario.Estado == "Suspendido")
                return ResultadoDTO<LoginResponseDTO>.Error(403, "Tu cuenta está suspendida. Comunícate con el administrador.");

            return ResultadoDTO<LoginResponseDTO>.Ok(CrearRespuestaLogin(usuario), "Inicio de sesión correcto.");
        }

        public async Task<ResultadoDTO<PerfilDTO>> GetPerfil(int idUsuario)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);
            if (usuario == null) return ResultadoDTO<PerfilDTO>.Error(404, "Usuario no encontrado.");

            var perfil = new PerfilDTO
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                Celular = usuario.Celular,
                FechaNacimiento = usuario.FechaNacimiento,
                Departamento = usuario.Departamento,
                Distrito = usuario.Distrito,
                NombreOrganizador = usuario.NombreOrganizador,
                Roles = usuario.UsuarioRol.Select(ur => ur.Rol.Nombre).ToList()
            };
            return ResultadoDTO<PerfilDTO>.Ok(perfil);
        }

        // US-01: un comprador activa "Quiero publicar eventos" sin crear otra cuenta.
        // Devuelve un token nuevo que ya incluye el rol Organizador.
        public async Task<ResultadoDTO<LoginResponseDTO>> ActivarOrganizador(int idUsuario, ActivarOrganizadorDTO dto)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);
            if (usuario == null) return ResultadoDTO<LoginResponseDTO>.Error(404, "Usuario no encontrado.");

            if (usuario.UsuarioRol.Any(ur => ur.IdRol == Roles.IdOrganizador))
                return ResultadoDTO<LoginResponseDTO>.Error(400, "Tu cuenta ya puede publicar eventos.");

            var nombre = dto.NombreOrganizador.Trim();
            if (await _usuarioRepository.ExisteNombreOrganizador(nombre))
                return ResultadoDTO<LoginResponseDTO>.Error(400, "El nombre del organizador ya está en uso.");

            await _usuarioRepository.ActivarOrganizador(idUsuario, nombre);

            usuario = await _usuarioRepository.GetUsuarioById(idUsuario);
            return ResultadoDTO<LoginResponseDTO>.Ok(CrearRespuestaLogin(usuario!), "Ahora también puedes publicar eventos.");
        }

        private LoginResponseDTO CrearRespuestaLogin(Usuario usuario)
        {
            var roles = usuario.UsuarioRol.Select(ur => ur.Rol.Nombre).ToList();
            return new LoginResponseDTO
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                NombreOrganizador = usuario.NombreOrganizador,
                Roles = roles,
                Token = _jwtService.GenerateJWToken(usuario, roles)
            };
        }
    }
}
