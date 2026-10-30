using System.ComponentModel.DataAnnotations;

namespace GroceryList.Infrastructure.Security;

/// <summary>
/// JWT settings bound from the "Jwt" configuration section and validated on startup.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    // 32 characters is at least 256 bits, the minimum key size for HMAC-SHA256.
    public const int SigningKeyMinLength = 32;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(SigningKeyMinLength)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 24 * 60)]
    public int ExpirationMinutes { get; set; } = 60;
}
