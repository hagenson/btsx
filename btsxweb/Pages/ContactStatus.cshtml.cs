using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BtsxWeb.Pages;

public class ContactStatusModel : PageModel
{
    public ContactStatusModel(ILogger<ContactStatusModel> logger)
    {
        this.logger = logger;
    }

    public string Id { get; set; } = "";
    public void OnGet(string id)
    {
        Id = id;
    }

    private readonly ILogger<ContactStatusModel> logger;
}
