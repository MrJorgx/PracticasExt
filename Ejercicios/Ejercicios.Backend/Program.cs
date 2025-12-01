using Microsoft.EntityFrameworkCore;
using Ejercicios.Backend.Data;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Iniciando la aplicación");

    var builder = WebApplication.CreateBuilder(args);

    // Reemplazar el logging por defecto con Serilog
    builder.Host.UseSerilog();

    //Add services to the container.
    builder.Services.AddOpenApi();

    // Agregar soporte para controladores
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Configurar PostgreSQL
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    );

    // Configurar CORS para permitir solicitudes desde el frontend Blazor
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazorApp", policy =>
        {
            policy.WithOrigins(
                    "http://localhost:5088",    // Frontend HTTP
                    "https://localhost:7290"   // Frontend HTTPS
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Agregar middleware de Serilog para logging de requests HTTP
    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    // Habilitar CORS
    app.UseCors("AllowBlazorApp");

    app.UseAuthorization();

    // Mapear los controladores
    app.MapControllers();

    Log.Information("Aplicacion iniciada correctamente.");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó de forma inesperada");
}
finally
{
    Log.CloseAndFlush();
}
