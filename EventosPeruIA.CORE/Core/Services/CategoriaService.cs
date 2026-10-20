using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.CORE.Core.Services
{
    // US-04 Gestión de categorías de eventos
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaService(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public async Task<IEnumerable<CategoriaListDTO>> GetCategoriasActivas()
        {
            var categorias = await _categoriaRepository.GetCategoriasActivas();
            return categorias.Select(c => new CategoriaListDTO
            {
                IdCategoria = c.IdCategoria,
                Nombre = c.Nombre,
                Descripcion = c.Descripcion
            });
        }

        public async Task<IEnumerable<CategoriaAdminDTO>> GetCategorias(string? buscar)
        {
            var categorias = await _categoriaRepository.GetCategorias(buscar);
            return categorias.Select(c => Mapear(c, c.Evento.Count));
        }

        public async Task<ResultadoDTO<CategoriaAdminDTO>> CreateCategoria(CategoriaCreateDTO dto)
        {
            var nombre = dto.Nombre.Trim();
            if (await _categoriaRepository.ExisteNombre(nombre))
                return ResultadoDTO<CategoriaAdminDTO>.Error(400, "Ya existe una categoría con ese nombre.");

            var categoria = new Categoria
            {
                Nombre = nombre,
                Descripcion = dto.Descripcion?.Trim(),
                Estado = true,
                FechaCreacion = DateTime.Now
            };
            await _categoriaRepository.CreateCategoria(categoria);
            return ResultadoDTO<CategoriaAdminDTO>.Ok(Mapear(categoria, 0), "Categoría creada correctamente.", 201);
        }

        public async Task<ResultadoDTO<CategoriaAdminDTO>> UpdateCategoria(int id, CategoriaUpdateDTO dto)
        {
            var categoria = await _categoriaRepository.GetCategoriaById(id);
            if (categoria == null) return ResultadoDTO<CategoriaAdminDTO>.Error(404, "Categoría no encontrada.");

            var nombre = dto.Nombre.Trim();
            if (await _categoriaRepository.ExisteNombre(nombre, id))
                return ResultadoDTO<CategoriaAdminDTO>.Error(400, "Ya existe una categoría con ese nombre.");

            categoria.Nombre = nombre;
            categoria.Descripcion = dto.Descripcion?.Trim();
            await _categoriaRepository.UpdateCategoria(categoria);

            var eventos = await _categoriaRepository.ContarEventos(id);
            return ResultadoDTO<CategoriaAdminDTO>.Ok(Mapear(categoria, eventos), "Categoría actualizada correctamente.");
        }

        // Las categorías no se eliminan, se desactivan
        public async Task<ResultadoDTO<CategoriaAdminDTO>> DesactivarCategoria(int id, bool confirmar)
        {
            var categoria = await _categoriaRepository.GetCategoriaById(id);
            if (categoria == null) return ResultadoDTO<CategoriaAdminDTO>.Error(404, "Categoría no encontrada.");
            if (!categoria.Estado) return ResultadoDTO<CategoriaAdminDTO>.Error(400, "La categoría ya está inactiva.");

            if (await _categoriaRepository.ContarActivas() <= 1)
                return ResultadoDTO<CategoriaAdminDTO>.Error(400, "Debe existir al menos una categoría activa.");

            var eventos = await _categoriaRepository.ContarEventos(id);
            if (!confirmar)
                return ResultadoDTO<CategoriaAdminDTO>.Error(409,
                    $"Esta categoría la usan {eventos} eventos. ¿Deseas desactivarla? Vuelve a enviar con confirmar=true.");

            categoria.Estado = false;
            await _categoriaRepository.UpdateCategoria(categoria);
            return ResultadoDTO<CategoriaAdminDTO>.Ok(Mapear(categoria, eventos), "Categoría desactivada.");
        }

        public async Task<ResultadoDTO<CategoriaAdminDTO>> ReactivarCategoria(int id)
        {
            var categoria = await _categoriaRepository.GetCategoriaById(id);
            if (categoria == null) return ResultadoDTO<CategoriaAdminDTO>.Error(404, "Categoría no encontrada.");
            if (categoria.Estado) return ResultadoDTO<CategoriaAdminDTO>.Error(400, "La categoría ya está activa.");

            categoria.Estado = true;
            await _categoriaRepository.UpdateCategoria(categoria);
            var eventos = await _categoriaRepository.ContarEventos(id);
            return ResultadoDTO<CategoriaAdminDTO>.Ok(Mapear(categoria, eventos), "Categoría reactivada.");
        }

        private static CategoriaAdminDTO Mapear(Categoria c, int cantidadEventos) => new()
        {
            IdCategoria = c.IdCategoria,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            Estado = c.Estado ? "Activa" : "Inactiva",
            CantidadEventos = cantidadEventos
        };
    }
}
