using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface ICategoriaRepository
    {
        Task<IEnumerable<Categoria>> GetCategoriasActivas();
        Task<IEnumerable<Categoria>> GetCategorias(string? buscar);
        Task<Categoria?> GetCategoriaById(int id);
        Task<bool> ExisteNombre(string nombre, int? excluirId = null);
        Task<int> ContarActivas();
        Task<int> ContarEventos(int idCategoria);
        Task<bool> CreateCategoria(Categoria categoria);
        Task<bool> UpdateCategoria(Categoria categoria);
    }
}
