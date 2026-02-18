using Btsx;
using BtsxWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BtsxWeb.Pages;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    public IndexModel(
        IServiceProvider serviceProvider,
        IMoverFactory moverFactory,
        ILogger<IndexModel> logger)
    {
        this.serviceProvider = serviceProvider;
        this.moverFactory = moverFactory;
        this.logger = logger;
    }

    public void OnGet()
    {
    }

    public IActionResult OnGetOAuthUrl(string implementer, MigrationDirection direction)
    {
        var state = $"{direction}_{Guid.NewGuid():N}";
        TempData["OAuthState"] = state;
        TempData["OAuthType"] = direction;

        var authUrl = serviceProvider.GetRequiredKeyedService<IOAuthService>(implementer)
            .GetAuthUrl(MigrationType.Mail, MigrationDirection.Source, state);
        return new JsonResult(new { authUrl });
    }

    public async Task<IActionResult> OnPostTestAuthAsync([FromBody] TestAuthRequest request)
    {
        if (string.IsNullOrEmpty(request.Server) || string.IsNullOrEmpty(request.User) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { success = false, message = "Server, username, and password are required" });
        }

        var creds = new Creds
        {
            Server = request.Server,
            User = request.User,
            Password = request.Password,
            UseOAuth = false,
            Implementer = request.Implementer ?? ""
        };

        try
        {

            var mover = moverFactory.CreateAuthenticator(request.MigrationType);
            var success = await mover.TestAuthenticationAsync(creds, HttpContext.RequestAborted);
            if (success)
            {
                return new JsonResult(new { success = true, message = "Authentication successful" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Authentication failed" });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error testing authentication");
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    private readonly IServiceProvider serviceProvider;
    private readonly IMoverFactory moverFactory;
    private readonly ILogger<IndexModel> logger;
}
