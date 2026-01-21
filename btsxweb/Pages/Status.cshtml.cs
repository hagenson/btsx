using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BtsxWeb.Pages;

public class StatusModel : PageModel
{
    public StatusModel(ILogger<StatusModel> logger)
    {
        this.logger = logger;
    }

    public string Id { get; set; } = "";
    public void OnGet(string id)
    {
        Id = id;
    }

    private readonly ILogger<StatusModel> logger;
}