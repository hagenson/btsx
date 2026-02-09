using Btsx;
using BtsxWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BtsxWeb.Pages;

[IgnoreAntiforgeryToken]
public class ContactsModel : PageModel
{
    public ContactsModel(ILogger<ContactsModel> logger, IOptions<GoogleOAuthSettings> googleOAuthSettings)
    {
        this.logger = logger;
        this.googleOAuthSettings = googleOAuthSettings.Value;
    }

    public void OnGet()
    {
    }

    public IActionResult OnGetGoogleAuth(string type)
    {
        var clientId = googleOAuthSettings.ClientId;
        var redirectUri = googleOAuthSettings.RedirectUri;

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
        {
            return BadRequest("Google OAuth is not configured. Please set ClientId and RedirectUri in appsettings.json");
        }

        var state = $"{type}_{Guid.NewGuid():N}";
        TempData["OAuthState"] = state;
        TempData["OAuthType"] = type;

        var scope = Uri.EscapeDataString("https://www.googleapis.com/auth/contacts https://www.googleapis.com/auth/userinfo.email");
        var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?client_id={Uri.EscapeDataString(clientId)}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope={scope}&access_type=offline&prompt=consent&state={state}";

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
            Implementor = request.Implementor,
        };

        try
        {
            var success = await ContactMover.TestAuthenticationAsync(creds, HttpContext.RequestAborted);
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

    private readonly GoogleOAuthSettings googleOAuthSettings;
    private readonly ILogger<ContactsModel> logger;
}
