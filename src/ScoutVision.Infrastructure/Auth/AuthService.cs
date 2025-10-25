using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using ScoutVision.Infrastructure.Caching;

namespace ScoutVision.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;

    public AuthService(IConfiguration configuration, ICacheService cacheService)
    {
        _configuration = configuration;
        _cacheService = cacheService;
    }

    public async Task<AuthToken> AuthenticateAsync(string username, string password)
    {
        // In production, validate against user database with hashed passwords
        // This is a placeholder implementation
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            throw new UnauthorizedAccessException("Invalid credentials");

        var jwtSecret = _configuration["Auth__JwtSecret"] ?? throw new InvalidOperationException("JWT secret not configured");
        var jwtExpiration = int.Parse(_configuration["Auth__JwtExpiration"] ?? "3600");
        var refreshExpiration = int.Parse(_configuration["Auth__RefreshTokenExpiration"] ?? "604800");

        var key = Encoding.ASCII.GetBytes(jwtSecret);
        // Example: Determine category based on username or other logic
        var userCategory = "Scouting"; // Replace with actual logic (e.g., lookup from DB)

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("tenant_id", "default"), // Multi-tenancy support
                new Claim("Category", userCategory) // Category claim for feature restriction
            }),
            Expires = DateTime.UtcNow.AddSeconds(jwtExpiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);

        var refreshToken = GenerateRefreshToken();
        
        // Cache refresh token for revocation check
        await _cacheService.SetAsync(
            $"refresh_token:{username}:{refreshToken}",
            username,
            TimeSpan.FromSeconds(refreshExpiration)
        );

        return new AuthToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = token.ValidTo,
            TokenType = "Bearer"
        };
    }

    public async Task<AuthToken> RefreshTokenAsync(string refreshToken)
    {
        // In production, validate refresh token against database
        if (string.IsNullOrEmpty(refreshToken))
            throw new UnauthorizedAccessException("Invalid refresh token");

        var principal = GetPrincipalFromExpiredToken("");
        var username = principal?.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(username))
            throw new UnauthorizedAccessException("Invalid token claims");

        // Verify refresh token hasn't been revoked
        var cachedToken = await _cacheService.GetAsync($"refresh_token:{username}:{refreshToken}");
        if (cachedToken == null)
            throw new UnauthorizedAccessException("Refresh token revoked or expired");

        return await AuthenticateAsync(username, "");
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        // Check if token is revoked
        var isRevoked = await _cacheService.GetAsync($"revoked_token:{token}");
        if (isRevoked != null)
            return false;

        try
        {
            var jwtSecret = _configuration["Auth__JwtSecret"] ?? throw new InvalidOperationException("JWT secret not configured");
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            var tokenHandler = new JwtSecurityTokenHandler();
            
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task RevokeTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return;

        try
        {
            var jwtSecurityToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwtSecurityToken != null)
            {
                var timeRemaining = jwtSecurityToken.ValidTo - DateTime.UtcNow;
                if (timeRemaining.TotalSeconds > 0)
                {
                    await _cacheService.SetAsync($"revoked_token:{token}", "true", timeRemaining);
                }
            }
        }
        catch
        {
            // Log error
        }
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var jwtSecret = _configuration["Auth__JwtSecret"] ?? throw new InvalidOperationException("JWT secret not configured");
        var key = Encoding.ASCII.GetBytes(jwtSecret);
        
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}