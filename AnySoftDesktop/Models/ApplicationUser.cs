namespace AnySoftDesktop.Models;

/// <summary>
/// User object that is returned when requested
/// </summary>
public class ApplicationUser
{
    /// <summary>
    /// Identifier
    /// </summary>
    public int? Id { get; set; }
    /// <summary>
    /// Login
    /// </summary>
    public string? Login { get; set; }
    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }
    /// <summary>
    /// Password
    /// </summary>
    public string? Password { get; set; }
    /// <summary>
    /// Image Path
    /// </summary>
    public string? Image { get; set; }
    /// <summary>
    /// User Jwt
    /// </summary>
    public string? JwtToken { get; set; }
}