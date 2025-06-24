using Microsoft.AspNetCore.Mvc;
using hub.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System;

namespace hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HubController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;

        public HubController(IHttpClientFactory httpClientFactory, ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User user)
        {
            if (user == null || 
                string.IsNullOrEmpty(user.Username) || 
                string.IsNullOrEmpty(user.Password) || 
                string.IsNullOrEmpty(user.Email))
            {
                return BadRequest(new { error = "Todos los campos del usuario son obligatorios." });
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == user.Username || u.Email == user.Email);
                
            if (existingUser != null)
            {
                return Conflict(new { error = "El nombre de usuario o correo electrónico ya está en uso." });
            }

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Usuario registrado exitosamente", userId = user.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al registrar usuario", details = ex.Message });
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "No se ha seleccionado un archivo válido." });
            }

            try
            {
                // Crear directorio si no existe
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                var filePath = Path.Combine(uploadsDir, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { 
                    message = "Archivo cargado exitosamente",
                    fileName = file.FileName,
                    fileSize = file.Length,
                    filePath = filePath
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error al subir el archivo", details = ex.Message });
            }
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessLinks([FromBody] List<string> links)
        {
            if (links == null || links.Count == 0)
            {
                return BadRequest(new { error = "La lista de enlaces no puede estar vacía." });
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ScraperAPI");
                var response = await client.PostAsJsonAsync("/process", links);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return StatusCode(
                        (int)response.StatusCode, 
                        new { 
                            error = "Error al procesar los enlaces",
                            details = errorContent
                        });
                }

                var result = await response.Content.ReadFromJsonAsync<object>();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    error = "Error interno al procesar enlaces", 
                    details = ex.Message 
                });
            }
        }

        [HttpPost("query")]
        public async Task<IActionResult> Query([FromBody] QueryRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Query))
            {
                return BadRequest(new { error = "El campo 'query' es obligatorio." });
            }

            const string djangoUrl = "http://localhost:8000/api/query-rag/";
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