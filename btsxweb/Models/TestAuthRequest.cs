using Btsx;

namespace BtsxWeb.Models;

/// <summary>
/// Encapsulates a request to test login credentials.
/// </summary>
public class TestAuthRequest
{
    /// <summary>
    /// The service being authenticated.
    /// </summary>
    public string? Implementer { get; set; }
    /// <summary>
    /// The type of migration begin attempted.
    /// </summary>
    public MigrationType MigrationType { get; set; }
    /// <summary>
    /// Account password.
    /// </summary>
    public string Password { get; set; } = "";
    /// <summary>
    /// Service hostname or URI.
    /// </summary>
    public string Server { get; set; } = "";
    /// <summary>
    /// Username on the service.
    /// </summary>
    public string User { get; set; } = "";
}