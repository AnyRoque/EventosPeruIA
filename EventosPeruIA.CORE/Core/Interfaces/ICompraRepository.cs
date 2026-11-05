using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface ICompraRepository
    {
        Task<Compra?> GetCompraByCodigoOperacion(Guid codigoOperacion);
        Task<Compra?> GetCompraById(int idCompra);
        Task<IEnumerable<Entrada>> GetEntradasByUsuario(int idUsuario);
        Task<Entrada?> GetEntradaByUsuario(int idUsuario, int idEntrada);
        Task<bool> CreateCompra(Compra compra);
    }
}
