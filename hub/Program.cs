using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using hub.Models;

var builder = WebApplication.CreateBuilder(args);

// Conexión a PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión de la base de datos no está configurada.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Hub API", Version = "v1" });
});

// Autenticación y autorización
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// HTTP Clients
builder.Services.AddHttpClient("ScraperAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:9020");
});

builder.Services.AddHttpClient("RagAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:9030");
});

// Django API (RAG)
builder.Services.AddHttpClient("DjangoAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:8000/");
    client.Timeout = TimeSpan.FromMinutes(2);
});

// CORS (opcional pero recomendado)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hub API v1");
    });
}

// CORS y pipeline HTTP
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
