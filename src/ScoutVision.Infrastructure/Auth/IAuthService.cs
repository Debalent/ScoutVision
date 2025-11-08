using System.Security.Claims;

namespace ScoutVision.Infrastructure.Auth;

public interface IAuthService
{
    Task<AuthToken> AuthenticateAsync(string username, string password);
    Task<AuthToken> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    Task RevokeTokenAsync(string token);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}

public class AuthToken
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
}

public class AuthRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public AuthToken Data { get; set; }
}