using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IUsuarioService
    {
        Task<ResultadoDTO<object>> Registrar(RegistroRequestDTO dto);
        Task<ResultadoDTO<LoginResponseDTO>> Login(LoginRequestDTO dto);
        Task<ResultadoDTO<PerfilDTO>> GetPerfil(int idUsuario);
        Task<ResultadoDTO<LoginResponseDTO>> ActivarOrganizador(int idUsuario, ActivarOrganizadorDTO dto);
    }
}
