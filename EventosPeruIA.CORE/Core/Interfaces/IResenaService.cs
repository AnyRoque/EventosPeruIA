using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IResenaService
    {
        Task<ResultadoDTO<List<ResenaDTO>>> ObtenerResenasPorEvento(int idEvento);
        Task<ResultadoDTO<bool>> CrearResena(int idUsuario, CrearResenaDTO dto);
        Task<ResultadoDTO<bool>> EditarResena(int idUsuario, int idResena, EditarResenaDTO dto);
        Task<ResultadoDTO<bool>> EliminarResena(int idUsuario, int idResena);
    }
}