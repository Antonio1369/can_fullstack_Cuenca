using Microsoft.AspNetCore.Mvc;
using hub.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using System;


namespace hub.Controllers.RAG
{
    [Route("api/[controller]")]
    [ApiController]
    public class RAGController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;

        public RAGController(IHttpClientFactory httpClientFactory, ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] QueryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Query))
            {
                return BadRequest(new { error = "El campo 'query' es obligatorio." });
            }

            const string djangoUrl = "http://localhost:9030/api/query-rag/";
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(2);

            try
            {
                var payload = new { query = request.Query };
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(djangoUrl, jsonContent);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode, 
                        new { 
                            error = "Error en el servidor Django",
                            status = response.StatusCode,
                            details = content
                        });
                }

                try
                {
                    var result = JsonSerializer.Deserialize<JsonDocument>(content);
                    return Ok(result);
                }
                catch (JsonException)
                {
                    return Ok(content);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                return StatusCode(504, new { error = "Timeout al conectar con el servidor Django" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Error interno al procesar la consulta",
                    details = ex.Message 
                });
            }
        }

        [HttpPost("user-query")]
        public async Task<IActionResult> CreateUserQuery([FromBody] UserQueryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.QueryText))
            {
                return BadRequest(new { error = "La consulta no puede estar vacía." });
            }

            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return NotFound(new { error = "Usuario no encontrado." });
                }

                var userQuery = new UserQuery
                {
                    UserId = user.Id,
                    QueryText = request.QueryText,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserQueries.Add(userQuery);
                await _context.SaveChangesAsync();

                return Ok(new { 
                    message = "Consulta registrada exitosamente",
                    queryId = userQuery.Id,
                    userId = user.Id,
                    createdAt = userQuery.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Error al registrar la consulta",
                    details = ex.Message 
                });
            }
        }
    }

    public class QueryRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    public class UserQueryRequest
    {
        public int UserId { get; set; }
        public string QueryText { get; set; } = string.Empty;
    }
}
