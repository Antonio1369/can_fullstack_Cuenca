using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace hub.Controllers.Files
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<FileController> _logger;
        private readonly string _uploadsPath;

        public FileController(
            IConfiguration config, 
            IHttpClientFactory httpClientFactory,
            ILogger<FileController> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), 
                config["FileSettings:UploadPath"] ?? "Uploads");
            
            Directory.CreateDirectory(_uploadsPath);
            
            // Print de inicialización
            Console.WriteLine($"FileController inicializado. Ruta de uploads: {_uploadsPath}");
            Debug.WriteLine($"Configuración cargada: {_config["FileSettings:AllowedExtensions"]}");
            _logger.LogInformation("FileController creado");
        }

        [HttpPost("upload")]
        [RequestSizeLimit(long.MaxValue)]
        public async Task<IActionResult> UploadFile(
            [Required][FromForm] IFormFile file, 
            [Required][FromForm] int userId)
        {
            // Print de inicio de método
            Console.WriteLine($"\n===== INICIO UploadFile =====");
            Console.WriteLine($"Usuario: {userId}, Archivo: {file?.FileName ?? "null"}");
            Debug.WriteLine($"Tamaño archivo: {file?.Length ?? 0} bytes");
            _logger.LogDebug($"Inicio UploadFile - User: {userId}, File: {file?.FileName}");

            try
            {
                // Validación básica
                if (file == null || file.Length == 0)
                {
                    Console.WriteLine("ERROR: Archivo vacío o nulo");
                    _logger.LogWarning("Archivo vacío recibido");
                    return BadRequest("El archivo está vacío");
                }

                // Print de metadatos del archivo
                Console.WriteLine($"Metadatos archivo:");
                Console.WriteLine($"- Nombre: {file.FileName}");
                Console.WriteLine($"- Tamaño: {file.Length} bytes");
                Console.WriteLine($"- ContentType: {file.ContentType}");
                Console.WriteLine($"- Headers: {string.Join(", ", file.Headers)}");

                // Validar extensión
                var allowedExtensions = _config["FileSettings:AllowedExtensions"]?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries) 
                    ?? new[] { ".pdf", ".doc", ".docx", ".jpg", ".png", ".jpeg",".txt" };
                
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                Console.WriteLine($"\nValidando extensión: {fileExtension}");
                Console.WriteLine($"Extensiones permitidas: {string.Join(", ", allowedExtensions)}");

                if (!allowedExtensions.Contains(fileExtension))
                {
                    Console.WriteLine("ERROR: Extensión no permitida");
                    _logger.LogWarning($"Extensión no permitida: {fileExtension}");
                    return BadRequest($"Tipo de archivo no permitido. Use: {string.Join(", ", allowedExtensions)}");
                }

                // Validar tamaño máximo
                var maxFileSize = _config.GetValue<long>("FileSettings:MaxFileSizeBytes", 100 * 1024 * 1024);
                Console.WriteLine($"\nValidando tamaño (Max: {maxFileSize} bytes, Actual: {file.Length} bytes)");

                if (file.Length > maxFileSize)
                {
                    Console.WriteLine("ERROR: Tamaño excedido");
                    _logger.LogWarning($"Tamaño excedido: {file.Length} > {maxFileSize}");
                    return BadRequest($"Tamaño máximo excedido ({maxFileSize / (1024 * 1024)}MB)");
                }

                // Generar nombre único
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadsPath, uniqueFileName);
                
                Console.WriteLine($"\nGuardando archivo:");
                Console.WriteLine($"- Nombre original: {file.FileName}");
                Console.WriteLine($"- Nombre único: {uniqueFileName}");
                Console.WriteLine($"- Ruta destino: {filePath}");

                // Guardar archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    Console.WriteLine("Iniciando copia del archivo...");
                    await file.CopyToAsync(stream);
                    Console.WriteLine("Archivo copiado exitosamente");
                }

                _logger.LogInformation($"Archivo guardado: {uniqueFileName}");

                // Procesar con FastAPI
                Console.WriteLine("\nEnviando a FastAPI...");
                var apiResponse = await ProcessWithFastAPI(file, userId, uniqueFileName);
                Console.WriteLine($"Respuesta de FastAPI: {apiResponse}");

                // Print de éxito
                Console.WriteLine("\n===== UPLOAD EXITOSO =====");
                _logger.LogInformation($"Upload exitoso - User: {userId}, FileId: {uniqueFileName}");

                return Ok(new 
                {
                    OriginalName = file.FileName,
                    StoredName = uniqueFileName,
                    Size = file.Length,
                    ContentType = file.ContentType,
                    ApiResponse = apiResponse
                });
            }
            catch (Exception ex)
            {
                // Print de error
                Console.WriteLine("\n===== ERROR =====");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                _logger.LogError(ex, "Error en UploadFile");
                return StatusCode(500, new 
                {
                    Error = "Error al procesar el archivo",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        private async Task<string> ProcessWithFastAPI(IFormFile file, int userId, string uniqueFileName)
        {
            var client = _httpClientFactory.CreateClient("FastAPI");
            
            try
            {
                using var form = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                
                // Asegúrate que los nombres coincidan con lo que espera FastAPI
                form.Add(new StreamContent(fileStream), "file", file.FileName); // "file" es clave
                form.Add(new StringContent(userId.ToString()), "userId"); // "userId" es clave

                // Debug: Verifica lo que se está enviando
                _logger.LogDebug($"Enviando a FastAPI - File: {file.FileName}, UserId: {userId}");
                Console.WriteLine($"Content Headers: {form.Headers}");

                var response = await client.PostAsync("/process", form);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"FastAPI error: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {errorContent}");
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ProcessWithFastAPI");
                throw;
            }
        }

        [HttpGet("download/{fileId}")]
        public IActionResult DownloadFile(string fileId)
        {
            Console.WriteLine($"\n===== INICIO DownloadFile =====");
            Console.WriteLine($"Solicitando fileId: {fileId}");
            _logger.LogDebug($"DownloadFile - FileId: {fileId}");

            try
            {
                if (string.IsNullOrWhiteSpace(fileId))
                {
                    Console.WriteLine("ERROR: fileId vacío");
                    return BadRequest("ID de archivo no válido");
                }

                var filePath = Path.Combine(_uploadsPath, fileId);
                Console.WriteLine($"Buscando archivo en: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("ERROR: Archivo no encontrado");
                    _logger.LogWarning($"Archivo no encontrado: {filePath}");
                    return NotFound("Archivo no encontrado");
                }

                Console.WriteLine("Archivo encontrado, preparando descarga...");
                var fileStream = System.IO.File.OpenRead(filePath);
                
                Console.WriteLine("\n===== DESCARGA EXITOSA =====");
                _logger.LogInformation($"Archivo descargado: {fileId}");
                
                return File(fileStream, "application/octet-stream", fileId);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n===== ERROR EN DESCARGA =====");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                _logger.LogError(ex, $"Error al descargar {fileId}");
                return StatusCode(500, new 
                {
                    Error = "Error al descargar archivo",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        [HttpGet("info/{fileId}")]
        public IActionResult GetFileInfo(string fileId)
        {
            Console.WriteLine($"\n===== INICIO GetFileInfo =====");
            Console.WriteLine($"Consultando info de: {fileId}");

            try
            {
                if (string.IsNullOrWhiteSpace(fileId))
                {
                    Console.WriteLine("ERROR: fileId vacío");
                    return BadRequest("ID de archivo no válido");
                }

                var filePath = Path.Combine(_uploadsPath, fileId);
                Console.WriteLine($"Ruta completa: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("ERROR: Archivo no existe");
                    return NotFound();
                }

                var fileInfo = new FileInfo(filePath);
                
                Console.WriteLine("\nInformación del archivo:");
                Console.WriteLine($"- Tamaño: {fileInfo.Length} bytes");
                Console.WriteLine($"- Última modificación: {fileInfo.LastWriteTimeUtc}");
                Console.WriteLine($"- Atributos: {fileInfo.Attributes}");

                Console.WriteLine("\n===== CONSULTA EXITOSA =====");
                
                return Ok(new
                {
                    FileName = fileId,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTimeUtc,
                    Path = filePath
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n===== ERROR =====");
                Console.WriteLine($"Mensaje: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                _logger.LogError(ex, $"Error en GetFileInfo para {fileId}");
                return StatusCode(500, new 
                {
                    Error = "Error interno",
                    Details = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }
    }
}