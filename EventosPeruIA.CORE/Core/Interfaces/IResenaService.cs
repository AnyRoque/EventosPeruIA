using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IResenaService
    {
        Task<ResultadoDTO<ResumenResenasEventoDTO>> ObtenerResenasPorEvento(int idEvento);
        Task<ResultadoDTO<ResenaDTO>> CrearResena(int idUsuario, CrearResenaDTO dto);
        Task<ResultadoDTO<ResenaDTO>> EditarResena(int idUsuario, int idResena, EditarResenaDTO dto);
        Task<ResultadoDTO<object>> EliminarResena(int idUsuario, int idResena);
    }
}
