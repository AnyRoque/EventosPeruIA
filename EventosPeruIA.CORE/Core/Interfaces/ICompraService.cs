using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface ICompraService
    {
        Task<ResultadoDTO<CompraResultadoDTO>> ComprarEntradas(int idUsuario, CompraCreateDTO dto);
        Task<ResultadoDTO<CompraResultadoDTO>> GetCompra(int idUsuario, int idCompra);
        Task<IEnumerable<EntradaDTO>> GetMisEntradas(int idUsuario);
    }
}
