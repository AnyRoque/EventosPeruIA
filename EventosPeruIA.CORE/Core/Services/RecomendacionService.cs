using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using EventosPeruIA.CORE.Core.DTOs;
using EventosPeruIA.CORE.Core.Entities;
using EventosPeruIA.CORE.Core.Interfaces;

namespace EventosPeruIA.CORE.Core.Services
{
    public class RecomendacionService : IRecomendacionService
    {
        private readonly IRecomendacionRepository _recomendacionRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public RecomendacionService(IRecomendacionRepository recomendacionRepository, IUsuarioRepository usuarioRepository)
        {
            _recomendacionRepository = recomendacionRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<IEnumerable<InteresDTO>> GetIntereses()
        {
            return (await _recomendacionRepository.GetInteresesActivos()).Select(MapearInteres).ToList();
        }

        public async Task<ResultadoDTO<List<InteresDTO>>> GetMisIntereses(int idUsuario)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);
            if (usuario == null) return ResultadoDTO<List<InteresDTO>>.Error(404, "Usuario no encontrado.");
            if (usuario.Estado != "Activo") return ResultadoDTO<List<InteresDTO>>.Error(403, "Tu cuenta no está activa.");
            var intereses = await _recomendacionRepository.GetInteresesUsuario(idUsuario);
            return ResultadoDTO<List<InteresDTO>>.Ok(intereses.Select(MapearInteres).ToList());
        }

        public async Task<ResultadoDTO<List<InteresDTO>>> GuardarIntereses(int idUsuario, UsuarioInteresesUpdateDTO dto)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);
            if (usuario == null) return ResultadoDTO<List<InteresDTO>>.Error(404, "Usuario no encontrado.");
            if (usuario.Estado != "Activo") return ResultadoDTO<List<InteresDTO>>.Error(403, "Tu cuenta no está activa.");

            var ids = dto.IdIntereses;
            if (ids == null || ids.Length < 1 || ids.Length > 5)
                return ResultadoDTO<List<InteresDTO>>.Error(400, "Selecciona entre 1 y 5 intereses.");
            if (ids.Any(id => id <= 0) || ids.Distinct().Count() != ids.Length)
                return ResultadoDTO<List<InteresDTO>>.Error(400, "Los intereses deben ser válidos y no repetirse.");

            var disponibles = await _recomendacionRepository.GetInteresesActivos();
            if (ids.Any(id => disponibles.All(i => i.IdInteres != id)))
                return ResultadoDTO<List<InteresDTO>>.Error(400, "Uno de los intereses no existe o está inactivo.");

            try
            {
                await _recomendacionRepository.GuardarIntereses(idUsuario, ids);
            }
            catch (DbUpdateException)
            {
                return ResultadoDTO<List<InteresDTO>>.Error(409, "No se pudieron guardar tus intereses. Actualiza e inténtalo nuevamente.");
            }

            return ResultadoDTO<List<InteresDTO>>.Ok(disponibles.Where(i => ids.Contains(i.IdInteres))
                .Select(MapearInteres).ToList(), "Intereses actualizados.");
        }

        public async Task<ResultadoDTO<RecomendacionResultadoDTO>> GetRecomendaciones(int idUsuario)
        {
            var usuario = await _usuarioRepository.GetUsuarioById(idUsuario);
            if (usuario == null) return ResultadoDTO<RecomendacionResultadoDTO>.Error(404, "Usuario no encontrado.");
            if (usuario.Estado != "Activo") return ResultadoDTO<RecomendacionResultadoDTO>.Error(403, "Tu cuenta no está activa.");

            var ahora = DateTime.Now;
            var intereses = await _recomendacionRepository.GetInteresesUsuario(idUsuario);
            var compradas = (await _recomendacionRepository.GetCategoriasCompradas(idUsuario)).ToHashSet();
            var visitadas = (await _recomendacionRepository.GetCategoriasVisitadas(idUsuario, ahora.AddDays(-90))).ToHashSet();
            var eventos = await _recomendacionRepository.GetEventosDisponibles(idUsuario, ahora);
            var esPopular = intereses.Count == 0 && compradas.Count == 0 && visitadas.Count == 0;

            List<EventoRecomendadoDTO> recomendaciones;
            if (esPopular)
            {
                // Inicio sin señales: populares del departamento, sin completar con otras regiones.
                recomendaciones = eventos.Where(e => MismoLugar(e.Departamento, usuario.Departamento))
                    .OrderByDescending(e => e.EntradasVendidas).ThenBy(e => e.FechaHoraInicio).ThenBy(e => e.IdEvento)
                    .Take(6).Select(e => MapearEvento(e, e.EntradasVendidas > 0
                        ? $"Entre los más vendidos en {e.Departamento}"
                        : $"Próximo evento en {e.Departamento}")).ToList();
            }
            else
            {
                recomendaciones = eventos.Select(e => Puntuar(e, usuario, intereses, compradas, visitadas))
                    .Where(r => r.Puntos > 0)
                    .OrderByDescending(r => r.Puntos).ThenByDescending(r => r.Evento.EntradasVendidas)
                    .ThenBy(r => r.Evento.FechaHoraInicio).ThenBy(r => r.Evento.IdEvento)
                    .Take(6).Select(r => MapearEvento(r.Evento, r.Motivo)).ToList();
            }

            var datos = new RecomendacionResultadoDTO
            {
                Titulo = esPopular ? "Populares cerca de ti" : "Recomendado para ti",
                EsPopular = esPopular,
                TieneIntereses = intereses.Count > 0,
                Eventos = recomendaciones
            };
            return ResultadoDTO<RecomendacionResultadoDTO>.Ok(datos,
                recomendaciones.Count == 0 ? "Por ahora no hay eventos disponibles para recomendarte." : "");
        }

        private static (Evento Evento, int Puntos, string Motivo) Puntuar(Evento evento, Usuario usuario,
            List<Interes> intereses, HashSet<int> compradas, HashSet<int> visitadas)
        {
            var puntos = 0;
            var motivos = new List<string>();
            var coincidencias = intereses.Where(i => CoincideInteres(i.Nombre, evento)).Select(i => i.Nombre).ToList();
            if (coincidencias.Count > 0)
            {
                puntos += 50;
                motivos.Add($"Porque te interesa {string.Join(", ", coincidencias)}");
            }
            if (compradas.Contains(evento.IdCategoria))
            {
                puntos += 30;
                motivos.Add($"Por tus compras de eventos de {evento.Categoria.Nombre}");
            }
            if (visitadas.Contains(evento.IdCategoria))
            {
                puntos += 15;
                motivos.Add($"Por los eventos de {evento.Categoria.Nombre} que visitaste");
            }
            // Un distrito solo coincide si también coincide el departamento.
            if (MismoLugar(evento.Departamento, usuario.Departamento))
            {
                var mismoDistrito = MismoLugar(evento.Distrito, usuario.Distrito);
                puntos += mismoDistrito ? 20 : 5;
                motivos.Add(mismoDistrito ? $"Cerca de ti: {evento.Distrito}" : $"En tu departamento: {evento.Departamento}");
            }
            return (evento, puntos, string.Join(". ", motivos));
        }

        // Interes (Música, Arte...) y Categoria (Concierto, Taller...) son catálogos distintos.
        // Afinidad por términos completos del título, descripción y categoría, sin depender de IDs.
        private static readonly Dictionary<string, string[]> TerminosInteres = new()
        {
            ["musica"] = new[] { "musica", "musical", "concierto", "rock", "jazz", "salsa", "banda", "bandas", "orquesta" },
            ["tecnologia"] = new[] { "tecnologia", "tecnologico", "programacion", "software", "robotica", "inteligencia artificial", "ia", "digital" },
            ["gastronomia"] = new[] { "gastronomia", "gastronomica", "gastronomico", "cocina", "comida", "reposteria", "sabores" },
            ["arte"] = new[] { "arte", "pintura", "ceramica", "arcilla", "fotografia", "escultura", "teatro", "danza", "artesanal" },
            ["deporte"] = new[] { "deporte", "deportivo", "deportes", "futbol", "running", "maraton", "ciclismo", "yoga" },
            ["negocios"] = new[] { "negocio", "negocios", "emprendedores", "emprendimiento", "empresa", "finanzas", "marketing", "inversion" },
            ["educacion"] = new[] { "educacion", "aprendizaje", "aprende", "curso", "capacitacion", "taller" },
            ["cultura"] = new[] { "cultura", "cultural", "historia", "tradicion", "tradiciones", "museo", "literatura", "artesanal" }
        };

        private static bool CoincideInteres(string interes, Evento evento)
        {
            var nombre = Normalizar(interes);
            var texto = $" {Normalizar($"{evento.Nombre} {evento.Descripcion} {evento.Categoria.Nombre}")} ";
            var terminos = TerminosInteres.GetValueOrDefault(nombre) ?? new[] { nombre };
            return terminos.Any(t => t.Length > 0 && texto.Contains($" {t} ", StringComparison.Ordinal));
        }

        private static bool MismoLugar(string a, string b) =>
            !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b) && Normalizar(a) == Normalizar(b);

        private static string Normalizar(string texto)
        {
            var caracteres = texto.Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return Regex.Replace(new string(caracteres).ToLowerInvariant(), "[^a-z0-9]+", " ").Trim();
        }

        private static InteresDTO MapearInteres(Interes interes) => new()
        {
            IdInteres = interes.IdInteres,
            Nombre = interes.Nombre
        };

        private static EventoRecomendadoDTO MapearEvento(Evento e, string motivo) => new()
        {
            IdEvento = e.IdEvento,
            Nombre = e.Nombre,
            Categoria = e.Categoria.Nombre,
            FechaHoraInicio = e.FechaHoraInicio,
            Departamento = e.Departamento,
            Distrito = e.Distrito,
            Precio = e.Precio,
            PrecioTexto = e.Precio == 0 ? "Gratis" : $"S/ {e.Precio.ToString("0.00", CultureInfo.InvariantCulture)}",
            CuposDisponibles = e.AforoMaximo - e.EntradasVendidas,
            Agotado = false,
            ImagenUrl = e.ImagenUrl,
            Motivo = motivo
        };
    }
}
