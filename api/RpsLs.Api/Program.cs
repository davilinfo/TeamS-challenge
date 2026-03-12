using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RpsLs.ApplicationService.Services;
using RpsLs.Infra.Data;
using RpsLs.Infra.Repositories;

var builder = WebApplication.CreateBuilder(args);

// JSON: camelCase output, ignore nulls
builder.Services.ConfigureHttpJsonOptions(opts =>
{
    opts.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opts.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// HttpClient for the external random number service
builder.Services.AddHttpClient<IRandomService, RandomService>();

// Persistence: SQL Server when running in Docker, in-memory otherwise
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
            || Environment.GetEnvironmentVariable("RUNNING_IN_DOCKER") == "true";

if (isDocker)
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("RpsLsDb"));
}

builder.Services.AddScoped<IScoreRepository, ScoreRepository>();
builder.Services.AddScoped<IRandomService, RandomService>();
builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RPSLS API", Version = "v1" });
});

// CORS: allow the React dev server and any other origin (configurable via appsettings)
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? ["*"];

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()));

var app = builder.Build();

// Ensure database is ready on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    Console.WriteLine($"Is docker: {isDocker}");
    if (isDocker){
        var retries = 10;
        while (true)
        {
            try
            {
                db.Database.Migrate();
                break;
            }
            catch (Exception ex)
            {
                if (retries-- == 0)
                    throw; // let the application fail after exhausting attempts

                // wait and try again
                Console.WriteLine($"Database unavailable, retrying in 5 seconds... ({retries} attempts left)");
                Thread.Sleep(5000);
            }
        }
    }
    else
        db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseRouting();

// Serve the React SPA from wwwroot (used in Docker/production)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback: return index.html for client-side routes
app.MapFallbackToFile("index.html");

app.Run();

// Expose Program for integration tests
public partial class Program { }
