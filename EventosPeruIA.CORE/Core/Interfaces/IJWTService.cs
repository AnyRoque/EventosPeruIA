using EventosPeruIA.CORE.Core.Entities;

namespace EventosPeruIA.CORE.Core.Interfaces
{
    public interface IJWTService
    {
        string GenerateJWToken(Usuario usuario, IEnumerable<string> roles);
    }
}
