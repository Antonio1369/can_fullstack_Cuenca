using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using hub.Models;
using BCrypt.Net;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;

namespace hub.Controllers.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context, 
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // Endpoint para registro de usuarios
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            try
            {
                // Validación básica
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Verificar si el usuario o email ya existen
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == registerRequest.Username || u.Email == registerRequest.Email);

                if (existingUser != null)
                {
                    return Conflict(new { message = "El nombre de usuario o email ya están en uso." });
                }

                // Crear el nuevo usuario
                var user = new User
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                    FirstName = registerRequest.FirstName ?? string.Empty,
                    LastName = registerRequest.LastName ?? string.Empty,
                    IsActive = true,
                    DateJoined = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generar token JWT
                var token = GenerateJwtToken(user);
                
                return Ok(new { 
                    message = "Usuario registrado exitosamente", 
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email,
                    token = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar usuario");
                return StatusCode(500, new { message = "Ocurrió un error interno al registrar el usuario." });
            }
        }

        // Endpoint para Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Intento de login para usuario: {Username}", loginRequest.Username);
                
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                {
                    _logger.LogWarning("Credenciales inválidas para usuario: {Username}", loginRequest.Username);
                    return Unauthorized(new { message = "Credenciales inválidas." });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Cuenta desactivada. Contacte al administrador." });
                }

                // Actualizar last login
                user.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                return Ok(new { 
                    token = token,
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return StatusCode(500, new { message = "Ocurrió un error interno durante el login." });
            }
        }

        // Endpoint para obtener información del usuario actual
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }

                return Ok(new {
                    userId = user.Id,
                    username = user.Username,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    isActive = user.IsActive,
                    dateJoined = user.DateJoined
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener información del usuario");
                return StatusCode(500, new { message = "Error al obtener información del usuario." });
            }
        }

        // Método para generar JWT
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IsStaff", user.IsStaff.ToString()),
                new Claim("IsSuperuser", user.IsSuperuser.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is missing")));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(Convert.ToDouble(_configuration["Jwt:ExpireHours"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    // Modelos para las solicitudes
    public class LoginRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
        public string? FirstName { get; set; }

        [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
        public string? LastName { get; set; }
    }
}