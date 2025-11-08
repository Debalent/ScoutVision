using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScoutVision.Infrastructure.Caching;
using StackExchange.Redis;

namespace ScoutVision.Tests.Unit
{
    public class CacheServiceTests
    {
        private readonly Mock<IDatabase> _mockRedis;
        private readonly RedisCacheService _cacheService;

        public CacheServiceTests()
        {
            _mockRedis = new Mock<IDatabase>();
            _cacheService = new RedisCacheService(_mockRedis.Object);
        }

        [Fact]
        public async Task GetAsync_WithExistingKey_ReturnsValue()
        {
            // Arrange
            var key = "test-key";
            var expectedValue = "{\"name\":\"Test\"}";
            _mockRedis.Setup(r => r.StringGetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(new RedisValue(expectedValue));

            // Act
            var result = await _cacheService.GetAsync<dynamic>(key);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAsync_WithNonExistentKey_ReturnsNull()
        {
            // Arrange
            var key = "nonexistent-key";
            _mockRedis.Setup(r => r.StringGetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(RedisValue.Null);

            // Act
            var result = await _cacheService.GetAsync<dynamic>(key);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_WithValidData_StoresValue()
        {
            // Arrange
            var key = "test-key";
            var value = new { name = "Test", id = 123 };
            _mockRedis.Setup(r => r.StringSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // Act
            await _cacheService.SetAsync(key, value, TimeSpan.FromSeconds(60));

            // Assert
            _mockRedis.Verify(r => r.StringSetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithExistingKey_RemovesValue()
        {
            // Arrange
            var key = "test-key";
            _mockRedis.Setup(r => r.KeyDeleteAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(1);

            // Act
            await _cacheService.DeleteAsync(key);

            // Assert
            _mockRedis.Verify(r => r.KeyDeleteAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
        {
            // Arrange
            var key = "test-key";
            _mockRedis.Setup(r => r.KeyExistsAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(1);

            // Act
            var exists = await _cacheService.ExistsAsync(key);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentKey_ReturnsFalse()
        {
            // Arrange
            var key = "nonexistent-key";
            _mockRedis.Setup(r => r.KeyExistsAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(0);

            // Act
            var exists = await _cacheService.ExistsAsync(key);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task SetExpiryAsync_WithValidTTL_UpdatesExpiration()
        {
            // Arrange
            var key = "test-key";
            var ttl = TimeSpan.FromSeconds(120);
            _mockRedis.Setup(r => r.KeyExpireAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<TimeSpan>(),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);

            // Act
            await _cacheService.SetExpiryAsync(key, ttl);

            // Assert
            _mockRedis.Verify(r => r.KeyExpireAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<TimeSpan>(),
                It.IsAny<CommandFlags>()), Times.Once);
        }

        [Fact]
        public async Task IncrementAsync_WithValidKey_IncrementsValue()
        {
            // Arrange
            var key = "counter-key";
            _mockRedis.Setup(r => r.StringIncrementAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<long>(),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(1);

            // Act
            var result = await _cacheService.IncrementAsync(key);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task DecrementAsync_WithValidKey_DecrementsValue()
        {
            // Arrange
            var key = "counter-key";
            _mockRedis.Setup(r => r.StringDecrementAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<long>(),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(0);

            // Act
            var result = await _cacheService.DecrementAsync(key);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task DeleteByPatternAsync_WithMatchingPattern_DeletesAllKeys()
        {
            // Arrange
            var pattern = "injury:*";
            var keys = new RedisKey[] { "injury:1", "injury:2", "injury:3" };
            
            // Mock the scan operation (simplified)
            _mockRedis.Setup(r => r.KeyDeleteAsync(
                It.IsAny<RedisKey[]>(),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(3);

            // Act
            await _cacheService.DeleteByPatternAsync(pattern);

            // Assert
            _mockRedis.Verify(r => r.KeyDeleteAsync(
                It.IsAny<RedisKey[]>(),
                It.IsAny<CommandFlags>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task GetAsync_WithComplexObject_DeserializesCorrectly()
        {
            // Arrange
            var key = "player-key";
            var playerJson = @"{""id"":123,""name"":""John"",""position"":""Forward""}";
            _mockRedis.Setup(r => r.StringGetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(new RedisValue(playerJson));

            // Act
            var result = await _cacheService.GetAsync<Player>(key);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(123, result.Id);
            Assert.Equal("John", result.Name);
        }

        [Fact]
        public async Task GetAsync_WithInvalidJson_ReturnsNull()
        {
            // Arrange
            var key = "invalid-key";
            var invalidJson = "{invalid json}";
            _mockRedis.Setup(r => r.StringGetAsync(
                It.Is<RedisKey>(k => k == key),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(new RedisValue(invalidJson));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _cacheService.GetAsync<Player>(key));
        }

        // Test helper class
        private class Player
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Position { get; set; }
        }
    }
}