using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;
using EventosPeruIA.CORE.Core.Settings;

namespace EventosPeruIA.CORE.Infrastructure.Shared
{
    public class JWTService : IJWTService
    {
        public JWTSettings _settings { get; }

        public JWTService(IOptions<JWTSettings> settings)
        {
            _settings = settings.Value;
        }

        public string GenerateJWToken(Usuario usuario, IEnumerable<string> roles)
        {
            var ssk = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var sc = new SigningCredentials(ssk, SecurityAlgorithms.HmacSha256);
            var header = new JwtHeader(sc);

            var claims = new List<Claim>
            {
                new Claim("UserId", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre + " " + usuario.Apellido),
                new Claim(ClaimTypes.GivenName, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
            };

            // Doble rol: se agrega un claim por cada rol del usuario
            foreach (var rol in roles)
                claims.Add(new Claim(ClaimTypes.Role, rol));

            var payload = new JwtPayload(
                _settings.Issuer,
                _settings.Audience,
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(_settings.ExpirationInMinutes));

            var token = new JwtSecurityToken(header, payload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
