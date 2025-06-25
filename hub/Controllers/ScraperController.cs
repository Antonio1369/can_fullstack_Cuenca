using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace hub.Controllers.Scraping
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScraperController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ScraperController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
    }
}
