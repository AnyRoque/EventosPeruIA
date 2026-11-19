using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;

namespace EventosPeruIA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> EnviarMensaje([FromBody] ChatRequest? request)
    {
        if (string.IsNullOrWhiteSpace(request?.Mensaje))
            return BadRequest(new { mensaje = "El mensaje no puede estar vacío." });

        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { mensaje = "La configuración del servicio de IA no está disponible." });

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.8-flash:generateContent?key={apiKey}";
        var promptSistema = "Eres el asistente virtual oficial de EventosPeruIA, una plataforma en Perú para descubrir, organizar y gestionar entradas a eventos. Responde de forma amable, breve y clara en español.";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = $"{promptSistema}\n\nUsuario: {request.Mensaje.Trim()}" }
                    }
                }
            }
        };

        var client = _httpClientFactory.CreateClient();
        using var content = new StringContent(
            JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorGoogle = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode,
                new { mensaje = "Error al comunicarse con el servicio de IA.", detalle = errorGoogle });
        }

        var jsonString = await response.Content.ReadAsStringAsync();
        var jsonNode = JsonNode.Parse(jsonString);
        var respuestaIA = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString()
            ?? "Lo siento, no pude procesar tu consulta en este momento.";

        return Ok(new { respuesta = respuestaIA });
    }
}

public class ChatRequest
{
    public string Mensaje { get; set; } = string.Empty;
}
