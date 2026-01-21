using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BtsxWeb.Pages;

public class PrivacyModel : PageModel
{
    public PrivacyModel(ILogger<PrivacyModel> logger)
    {
        this.logger = logger;
    }

    public void OnGet()
    {
    }

    private readonly ILogger<PrivacyModel> logger;
}