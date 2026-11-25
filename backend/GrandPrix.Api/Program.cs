using GrandPrix.Api.Configuration;
using GrandPrix.Api.Interfaces;
using GrandPrix.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();

builder.Services.Configure<AisSettings>(builder.Configuration.GetSection(AisSettings.SectionName));
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IAisClient, AisClient>((serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<AisSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(settings.HttpTimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(settings.UserAgent);
});
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
builder.Services.AddSingleton<IAisProcessingService, AisProcessingService>();
builder.Services.AddSingleton<IAisSnapshotStore, AisSnapshotStore>();
builder.Services.AddSingleton<IShipQueryService, ShipQueryService>();
builder.Services.AddHostedService<AisPollingService>();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
