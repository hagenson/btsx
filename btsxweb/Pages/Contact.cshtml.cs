using BtsxWeb.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BtsxWeb.Pages;

public class ContactModel : PageModel
{
    public ContactModel(ILogger<ContactModel> logger, IOptions<AppConfig> appConfig)
    {
        this.logger = logger;
        this.appConfig = appConfig.Value;
    }

    public string SupportEmail => appConfig.SupportEmail;
    public void OnGet()
    {
    }

    private readonly AppConfig appConfig;
    private readonly ILogger<ContactModel> logger;
}