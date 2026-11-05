using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IModeracionService
    {
        Task<ResultadoDTO<PaginaDTO<EventoModeracionDTO>>> GetEventos(ModeracionFiltroDTO filtro);
        Task<ResultadoDTO<PaginaDTO<UsuarioModeracionDTO>>> GetUsuarios(ModeracionFiltroDTO filtro);
        Task<ResultadoDTO<PaginaDTO<ResenaModeracionDTO>>> GetResenas(ModeracionFiltroDTO filtro);
        Task<ResultadoDTO<PaginaDTO<HistorialModeracionDTO>>> GetHistorial(HistorialFiltroDTO filtro);

        Task<ResultadoDTO<object>> SuspenderEvento(int idAdmin, int idEvento, ModeracionMotivoDTO dto);
        Task<ResultadoDTO<object>> ReactivarEvento(int idAdmin, int idEvento, ModeracionReactivarDTO dto);
        Task<ResultadoDTO<object>> SuspenderUsuario(int idAdmin, int idUsuario, ModeracionMotivoDTO dto);
        Task<ResultadoDTO<object>> ReactivarUsuario(int idAdmin, int idUsuario, ModeracionReactivarDTO dto);
        Task<ResultadoDTO<object>> OcultarResena(int idAdmin, int idResena, ModeracionMotivoDTO dto);
    }
}
