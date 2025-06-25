using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System;
using hub.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFilesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UserFilesController> _logger;
        private readonly ApplicationDbContext _context;

        public UserFilesController(
            IHttpClientFactory httpClientFactory,
            ILogger<UserFilesController> logger,
            ApplicationDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _context = context;
        }

        // GET: api/userfiles?userId=123&processed=true
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserFileDto>>> GetUserFiles(
            [FromQuery] int? userId,
            [FromQuery] bool? processed,
            [FromQuery] string filenameContains = null,
            [FromQuery] DateTime? uploadedAfter = null,
            [FromQuery] DateTime? uploadedBefore = null)
        {
            try
            {
                _logger.LogInformation("Obteniendo user files con filtros");

                var query = _context.UserFiles.AsQueryable();

                // Aplicar filtros
                if (userId.HasValue)
                {
                    query = query.Where(f => f.UserId == userId.Value);
                }

                if (processed.HasValue)
                {
                    query = query.Where(f => f.Processed == processed.Value);
                }

                if (!string.IsNullOrEmpty(filenameContains))
                {
                    query = query.Where(f => f.Filename.Contains(filenameContains));
                }

                if (uploadedAfter.HasValue)
                {
                    query = query.Where(f => f.UploadedAt >= uploadedAfter.Value);
                }

                if (uploadedBefore.HasValue)
                {
                    query = query.Where(f => f.UploadedAt <= uploadedBefore.Value);
                }

                // Ordenar por fecha de subida descendente
                query = query.OrderByDescending(f => f.UploadedAt);

                var result = await query.Select(f => new UserFileDto
                {
                    Id = f.Id,
                    UserId = f.UserId,
                    Filename = f.Filename,
                    TaskId = f.TaskId,
                    UploadedAt = f.UploadedAt,
                    Processed = f.Processed,
                    //FileExtension = f.FileExtension
                }).ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener user files");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // GET: api/userfiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserFileDto>> GetUserFile(int id)
        {
            try
            {
                var userFile = await _context.UserFiles.FindAsync(id);

                if (userFile == null)
                {
                    return NotFound();
                }

                return Ok(new UserFileDto
                {
                    Id = userFile.Id,
                    UserId = userFile.UserId,
                    Filename = userFile.Filename,
                    TaskId = userFile.TaskId,
                    UploadedAt = userFile.UploadedAt,
                    Processed = userFile.Processed,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener user file con ID {id}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // DELETE: api/userfiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserFile(int id)
        {
            try
            {
                var userFile = await _context.UserFiles.FindAsync(id);
                if (userFile == null)
                {
                    return NotFound();
                }

                _context.UserFiles.Remove(userFile);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar user file con ID {id}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        // GET: api/userfiles/5/content
        [HttpGet("{id}/content")]
        public async Task<IActionResult> GetUserFileContent(int id)
        {
            try
            {
                var userFile = await _context.UserFiles.FindAsync(id);
                if (userFile == null)
                {
                    return NotFound();
                }

                // Aquí implementar la lógica para obtener el contenido del archivo
                // Esto dependerá de cómo estés almacenando los archivos

                return Ok(new { message = "Contenido del archivo" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener contenido del user file con ID {id}");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
    }

    public class UserFileDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Filename { get; set; }
        public string TaskId { get; set; }
        public DateTime UploadedAt { get; set; }
        public bool Processed { get; set; }
        public string FileExtension { get; set; }
    }
}