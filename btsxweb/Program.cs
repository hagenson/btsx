using AutoMapper;
using Btsx;
using Btsx.Google;
using BtsxWeb;
using BtsxWeb.Hubs;
using BtsxWeb.Models;
using BtsxWeb.Services;
using Microsoft.Extensions.FileProviders;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.Configure<Btsx.Persistence.PersistenceSettings>(builder.Configuration.GetSection("Persistence"));
builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));

builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        // Configure polymorphic serialization for MigrationRequest
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSingleton<IPersistenceService, Btsx.Persistence.PersistenceService>();
builder.Services.AddSingleton<IEncryptionService, Btsx.Persistence.EncryptionService>();
builder.Services.AddKeyedSingleton<IOAuthService, GoogleOAuthService>("Google");
builder.Services.AddSingleton<IMoverFactory, MoverFactory>();
builder.Services.AddSingleton<DataMoverService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<DataMoverService>());

builder.Services.AddSignalR()
.AddJsonProtocol(options =>
{
    // Use camelCase property names to match JavaScript client
    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddScoped<IStatusNotifier, NotifierProxy>();
builder.Services.AddSingleton<IMapper>(sp =>
{
    var cfg = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<AutoMapperConfig>();
    },
    sp.GetRequiredService<ILoggerFactory>());
#if DEBUG
    cfg.AssertConfigurationIsValid();
#endif
    return cfg.CreateMapper();
});
builder.Services.AddHttpClient();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var hugoPublicPath = Path.Combine(app.Environment.ContentRootPath, "..", "docs", "public");
if (Directory.Exists(hugoPublicPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions()
    {
        FileProvider = new PhysicalFileProvider(hugoPublicPath),
        RequestPath = new PathString("/help")
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(hugoPublicPath),
        RequestPath = "/help"        
    });
}

app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();
app.MapHub<MigrationHub>("/migrationHub");
app.MapGet("/{id}", (string id) => Results.Accepted("/Index", new { id }));

app.Run();