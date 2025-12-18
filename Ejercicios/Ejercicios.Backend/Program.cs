using Microsoft.EntityFrameworkCore;
using Ejercicios.Backend.Data;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Ejercicios.Backend")
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt", 
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Seq("http://localhost:5341", apiKey: null, controlLevelSwitch: null)
    .CreateLogger();

try
{
    Log.Information("=== INICIANDO APLICACIÓN EJERCICIOS.BACKEND ===");
    Log.Information("Configurando Seq en http://localhost:5341");

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

    builder.Services.AddScoped<Ejercicios.Backend.Services.IEmailService, Ejercicios.Backend.Services.EmailService>();

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
    app.UseSerilogRequestLogging(configure =>
    {
        configure.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        configure.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
            diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
        };
    });

    app.UseHttpsRedirection();

    // Habilitar CORS
    app.UseCors("AllowBlazorApp");

    app.UseAuthorization();

    // Mapear los controladores
    app.MapControllers();

    Log.Information("=== APLICACIÓN INICIADA CORRECTAMENTE ===");
    Log.Information("Swagger disponible en: http://localhost:5231/swagger");
    Log.Information("Seq dashboard disponible en: http://localhost:5341");
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "=== LA APLICACIÓN TERMINÓ DE FORMA INESPERADA ===");
}
finally
{
    Log.Information("=== CERRANDO APLICACIÓN ===");
    Log.CloseAndFlush();
}
