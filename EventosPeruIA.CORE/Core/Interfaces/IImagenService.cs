using EventosPeruIA.CORE.Core.DTOs;
using Microsoft.AspNetCore.Http;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IImagenService
    {
        Task<ResultadoDTO<string>> SubirImagenEvento(int idOrganizador, int idEvento, IFormFile? imagen, string carpetaWeb);
        Task<ResultadoDTO<object>> EliminarImagenEvento(int idOrganizador, int idEvento, string carpetaWeb);
    }
}
