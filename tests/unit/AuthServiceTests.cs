using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ScoutVision.Infrastructure.Auth;
using StackExchange.Redis;

namespace ScoutVision.Tests.Unit
{
    public class AuthServiceTests
    {
        private readonly Mock<IDatabase> _mockRedis;
        private readonly AuthService _authService;
        private const string TestSecret = "test-secret-key-for-testing-purposes-minimum-32-chars";

        public AuthServiceTests()
        {
            _mockRedis = new Mock<IDatabase>();
            _authService = new AuthService(_mockRedis.Object, TestSecret);
        }

        [Fact]
        public void GenerateToken_WithValidUser_ReturnsToken()
        {
            // Arrange
            var userId = "user123";
            var email = "test@example.com";
            var roles = new[] { "Admin", "Coach" };

            // Act
            var token = _authService.GenerateToken(userId, email, roles);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.Contains(".", token); // JWT has 3 parts separated by dots
        }

        [Fact]
        public void GenerateToken_WithDifferentUsers_ReturnsDifferentTokens()
        {
            // Arrange
            var token1 = _authService.GenerateToken("user1", "user1@example.com", new[] { "Coach" });
            var token2 = _authService.GenerateToken("user2", "user2@example.com", new[] { "Coach" });

            // Act & Assert
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
        {
            // Arrange
            var userId = "user123";
            var email = "test@example.com";
            var roles = new[] { "Admin" };
            var token = _authService.GenerateToken(userId, email, roles);

            // Act
            var principal = _authService.ValidateToken(token);

            // Assert
            Assert.NotNull(principal);
            Assert.NotNull(principal.Identity);
            Assert.True(principal.Identity.IsAuthenticated);
        }

        [Fact]
        public void ValidateToken_WithInvalidToken_ReturnsNull()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var principal = _authService.ValidateToken(invalidToken);

            // Assert
            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_WithExpiredToken_ReturnsNull()
        {
            // Arrange
            var userId = "user123";
            var email = "test@example.com";
            var roles = new[] { "Coach" };
            
            // Create a token with 1 second expiration (will expire immediately for testing)
            var token = _authService.GenerateToken(userId, email, roles, expirationSeconds: 1);
            System.Threading.Thread.Sleep(2000); // Wait for expiration

            // Act
            var principal = _authService.ValidateToken(token);

            // Assert
            Assert.Null(principal);
        }

        [Fact]
        public async Task RevokeToken_WithValidToken_AddsToBla cklist()
        {
            // Arrange
            var token = _authService.GenerateToken("user123", "test@example.com", new[] { "Admin" });
            _mockRedis.Setup(r => r.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>()))
                .ReturnsAsync(true);

            // Act
            await _authService.RevokeTokenAsync(token);

            // Assert
            _mockRedis.Verify(r => r.StringSetAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>()), Times.Once);
        }

        [Fact]
        public async Task IsTokenRevoked_WithRevokedToken_ReturnsTrue()
        {
            // Arrange
            var token = _authService.GenerateToken("user123", "test@example.com", new[] { "Admin" });
            _mockRedis.Setup(r => r.StringGetAsync(
                It.IsAny<RedisKey>()))
                .ReturnsAsync(new RedisValue("revoked"));

            // Act
            var isRevoked = await _authService.IsTokenRevokedAsync(token);

            // Assert
            Assert.True(isRevoked);
        }

        [Fact]
        public async Task IsTokenRevoked_WithValidToken_ReturnsFalse()
        {
            // Arrange
            var token = _authService.GenerateToken("user123", "test@example.com", new[] { "Admin" });
            _mockRedis.Setup(r => r.StringGetAsync(
                It.IsAny<RedisKey>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var isRevoked = await _authService.IsTokenRevokedAsync(token);

            // Assert
            Assert.False(isRevoked);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsValidToken()
        {
            // Act
            var refreshToken = _authService.GenerateRefreshToken();

            // Assert
            Assert.NotNull(refreshToken);
            Assert.NotEmpty(refreshToken);
            Assert.True(refreshToken.Length > 32);
        }

        [Fact]
        public async Task RefreshAccessToken_WithValidRefreshToken_ReturnsNewAccessToken()
        {
            // Arrange
            var userId = "user123";
            var email = "test@example.com";
            var roles = new[] { "Coach" };
            var refreshToken = _authService.GenerateRefreshToken();

            _mockRedis.Setup(r => r.StringGetAsync(
                It.IsAny<RedisKey>()))
                .ReturnsAsync(new RedisValue(userId));

            // Act
            var newToken = await _authService.RefreshAccessTokenAsync(userId, refreshToken);

            // Assert
            Assert.NotNull(newToken);
            Assert.NotEmpty(newToken);
        }

        [Theory]
        [InlineData("Admin")]
        [InlineData("Coach")]
        [InlineData("Analyst")]
        [InlineData("Viewer")]
        public void GenerateToken_WithDifferentRoles_IncludesRoleInClaims(string role)
        {
            // Arrange
            var token = _authService.GenerateToken("user123", "test@example.com", new[] { role });

            // Act
            var principal = _authService.ValidateToken(token);

            // Assert
            Assert.NotNull(principal);
            Assert.True(principal.IsInRole(role));
        }

        [Fact]
        public void GenerateToken_TokenContainsUserIdClaim()
        {
            // Arrange
            var userId = "user123";
            var token = _authService.GenerateToken(userId, "test@example.com", new[] { "Coach" });

            // Act
            var principal = _authService.ValidateToken(token);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Assert
            Assert.NotNull(userIdClaim);
            Assert.Equal(userId, userIdClaim);
        }
    }
}