using BtsxWeb;
using BtsxWeb.Hubs;
using BtsxWeb.Models;
using BtsxWeb.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.Configure<PersistenceSettings>(builder.Configuration.GetSection("Persistence"));
builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));

builder.Services.AddRazorPages();
builder.Services.AddSingleton<EncryptionService>();
builder.Services.AddSingleton<JobPersistenceService>();
builder.Services.AddSingleton<GoogleOAuthService>();
builder.Services.AddSingleton<MailMoverService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<MailMoverService>());
builder.Services.AddSingleton<ContactMoverService>();
builder.Services.AddSingleton<ContactMapper>();
builder.Services.AddSingleton<ContactJobPersistenceService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ContactMoverService>());
builder.Services.AddSignalR();
builder.Services.AddScoped<IStatusNotifier, NotifierProxy>();
builder.Services.AddSingleton<Mapper>();
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
app.MapHub<ContactHub>("/contactHub");

app.Run();