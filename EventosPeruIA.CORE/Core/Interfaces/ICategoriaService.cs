using EventosPeruIA.CORE.Core.DTOs;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaListDTO>> GetCategoriasActivas();
        Task<IEnumerable<CategoriaAdminDTO>> GetCategorias(string? buscar);
        Task<ResultadoDTO<CategoriaAdminDTO>> CreateCategoria(CategoriaCreateDTO dto);
        Task<ResultadoDTO<CategoriaAdminDTO>> UpdateCategoria(int id, CategoriaUpdateDTO dto);
        Task<ResultadoDTO<CategoriaAdminDTO>> DesactivarCategoria(int id, bool confirmar);
        Task<ResultadoDTO<CategoriaAdminDTO>> ReactivarCategoria(int id);
    }
}
