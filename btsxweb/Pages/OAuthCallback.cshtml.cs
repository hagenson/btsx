using BtsxWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BtsxWeb.Pages;

public class OAuthCallbackModel : PageModel
{
    public OAuthCallbackModel(
        GoogleOAuthService googleOAuth,
        ILogger<OAuthCallbackModel> logger)
    {
        this.googleOAuth = googleOAuth;
        this.logger = logger;
    }

    public string? Email { get; set; }
    public string? Error { get; set; }
    public string? ServerType { get; set; }
    public bool Success { get; set; }
    public string? Token { get; set; }

    public async Task<IActionResult> OnGetAsync(string? code, string? state, string? error,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Success = false;
            Error = error;
            return Page();
        }

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            Success = false;
            Error = "Missing authorization code or state";
            return Page();
        }

        var savedState = TempData["OAuthState"]?.ToString();
        var serverType = TempData["OAuthType"]?.ToString();

        if (savedState != state)
        {
            Success = false;
            Error = "Invalid state parameter";
            return Page();
        }

        try
        {
            var tokenResponse = await googleOAuth.RequestTokenAsync(code, cancellationToken);
            if (tokenResponse.access_token == null)
            {
                Success = false;
                Error = "No access token received";
                return Page();
            }

            // Get user's email from Google
            Email = tokenResponse.user_id;

            Success = true;
            Token = tokenResponse.access_token;
            ServerType = serverType ?? "source";
        }
        catch (Exception ex)
        {
            Success = false;
            Error = $"Error during OAuth: {ex.Message}";
            logger.LogError(ex, "Error during OAuth callback");
        }

        return Page();
    }

    private readonly GoogleOAuthService googleOAuth;
    private readonly ILogger<OAuthCallbackModel> logger;
}