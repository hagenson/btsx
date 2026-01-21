using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BtsxWeb.Pages;

public class TosModel : PageModel
{
    public TosModel(ILogger<TosModel> logger)
    {
        this.logger = logger;
    }

    public void OnGet()
    {
    }

    private readonly ILogger<TosModel> logger;
}