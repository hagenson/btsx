using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BtsxWeb.Pages;

public class LicenseModel : PageModel
{
    public LicenseModel(ILogger<LicenseModel> logger)
    {
        this.logger = logger;
    }

    public void OnGet()
    {
    }

    private readonly ILogger<LicenseModel> logger;
}