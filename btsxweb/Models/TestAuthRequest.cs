using Btsx;

namespace BtsxWeb.Models;

public class TestAuthRequest
{
    public string Password { get; set; } = "";
    public string Server { get; set; } = "";
    public string User { get; set; } = "";
    public string? Implementer { get; set; }
    public MigrationType MigrationType { get; set; }
}