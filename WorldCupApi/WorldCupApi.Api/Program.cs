using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Swashbuckle.AspNetCore.Filters;
using WorldCupApi.Api.Data;
using WorldCupApi.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ── MongoDB ──────────────────────────────────────────────────────────────
// Connection string lives in appsettings.{Environment}.json — never hardcode it.
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MongoDbSettings>>().Value;
    return client.GetDatabase(settings.DatabaseName);
});

// ── Repositories (Repository pattern — one generic implementation per collection) ──
builder.Services.AddSingleton<IRepository<Team>>(sp =>
    new MongoRepository<Team>(sp.GetRequiredService<IMongoDatabase>(), "teams"));
builder.Services.AddSingleton<IRepository<Match>>(sp =>
    new MongoRepository<Match>(sp.GetRequiredService<IMongoDatabase>(), "matches"));
builder.Services.AddSingleton<IRepository<PredictionResponse>>(sp =>
    new MongoRepository<PredictionResponse>(sp.GetRequiredService<IMongoDatabase>(), "predictions"));
builder.Services.AddSingleton<IRepository<Country>>(sp =>
    new MongoRepository<Country>(sp.GetRequiredService<IMongoDatabase>(), "countries"));
builder.Services.AddSingleton<IRepository<QuizQuestion>>(sp =>
    new MongoRepository<QuizQuestion>(sp.GetRequiredService<IMongoDatabase>(), "quizQuestions"));
builder.Services.AddSingleton<IRepository<BotStats>>(sp =>
    new MongoRepository<BotStats>(sp.GetRequiredService<IMongoDatabase>(), "botStats"));

builder.Services.AddSingleton<IWorldCupDataStore, WorldCupDataStore>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WorldCup API",
        Version = "v1",
        Description = "World Cup API (teams, matches, predictions, countries, quiz questions, bot stats) backed by MongoDB and seeded on first run — every endpoint is testable directly from this Swagger UI via \"Try it out\"."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

var app = builder.Build();

// Seed MongoDB with sample data on first run (no-op if collections already have data),
// then make sure required indexes exist (unique constraints, lookup indexes). Both are
// idempotent — safe to run on every startup.
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
    await IndexInitializer.EnsureIndexesAsync(scope.ServiceProvider);
}

// Safety net: translate a MongoDB duplicate-key write (e.g. a race that slips past a
// controller's proactive existence/uniqueness check) into a clean 409 instead of a raw 500.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { error = "Duplicate key violation.", detail = ex.WriteError.Message });
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WorldCup API v1");
        options.EnableTryItOutByDefault();
        options.DocumentTitle = "WorldCup API";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var url = app.Urls.FirstOrDefault(u => u.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                  ?? app.Urls.FirstOrDefault();
        if (url is not null)
        {
            LaunchChrome($"{url}/swagger", app.Logger);
        }
        else
        {
            app.Logger.LogWarning("Could not determine the app URL to open Chrome automatically.");
        }
    });
}

app.Run();

static void LaunchChrome(string url, ILogger logger)
{
    string[] candidatePaths =
    {
        Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Google\Chrome\Application\chrome.exe"),
        Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe"),
        Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Google\Chrome\Application\chrome.exe")
    };

    var chromePath = candidatePaths.FirstOrDefault(File.Exists);

    try
    {
        var startInfo = chromePath is not null
            ? new ProcessStartInfo(chromePath, url) { UseShellExecute = true }
            : new ProcessStartInfo("chrome", url) { UseShellExecute = true };

        Process.Start(startInfo);
        logger.LogInformation("Opened {Url} in Chrome ({ChromePath}).", url, chromePath ?? "chrome on PATH");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not launch Chrome automatically. Open {Url} manually.", url);
    }
}
