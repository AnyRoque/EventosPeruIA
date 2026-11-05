using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IRecomendacionService
    {
        Task<IEnumerable<InteresDTO>> GetIntereses();
        Task<ResultadoDTO<List<InteresDTO>>> GetMisIntereses(int idUsuario);
        Task<ResultadoDTO<List<InteresDTO>>> GuardarIntereses(int idUsuario, UsuarioInteresesUpdateDTO dto);
        Task<ResultadoDTO<RecomendacionResultadoDTO>> GetRecomendaciones(int idUsuario);
    }
}
