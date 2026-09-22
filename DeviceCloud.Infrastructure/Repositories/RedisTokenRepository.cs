using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DeviceCloud.Domain.Entities;
using DeviceCloud.Domain.Repositories;
using DeviceCloud.Infrastructure.Options;
using StackExchange.Redis;

namespace DeviceCloud.Infrastructure.Repositories;

/// <summary>
/// Redis 令牌仓储实现
/// </summary>
public class RedisTokenRepository : ITokenRepository
{
    private readonly IDatabase _database;
    private readonly RedisOptions _options;
    private readonly ILogger<RedisTokenRepository> _logger;
    private readonly string _tokenKey;
    private readonly string _refreshLockKey;

    public RedisTokenRepository(
        IConnectionMultiplexer redis,
        IOptions<RedisOptions> options,
        ILogger<RedisTokenRepository> logger)
    {
        _database = redis.GetDatabase();
        _options = options.Value;
        _logger = logger;
        _tokenKey = $"{_options.TokenKeyPrefix}:current";
        _refreshLockKey = $"{_options.TokenKeyPrefix}:refresh_lock";
    }

    public async Task<QuectelToken?> GetAsync()
    {
        try
        {
            var value = await _database.StringGetAsync(_tokenKey);

            if (!value.HasValue)
            {
                _logger.LogDebug("Token not found in Redis");
                return null;
            }

            var data = JsonSerializer.Deserialize<TokenData>(value!);
            if (data == null)
            {
                _logger.LogWarning("Failed to deserialize token from Redis");
                return null;
            }

            // 重新构造 Token 实体
            return QuectelToken.Create(
                data.AccessToken,
                data.TokenType,
                data.ExpiresIn,
                data.RefreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token from Redis");
            return null;
        }
    }

    public async Task SaveAsync(QuectelToken token)
    {
        try
        {
            var data = new TokenData
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType,
                ExpiresIn = token.ExpiresIn,
                RefreshToken = token.RefreshToken,
                CreatedAt = token.CreatedAt
            };

            var value = JsonSerializer.Serialize(data);

            // 设置过期时间比 Token 有效期稍长，留出刷新时间
            var expiry = TimeSpan.FromSeconds(token.ExpiresIn + 300);

            await _database.StringSetAsync(_tokenKey, value, expiry);

            _logger.LogInformation("Token saved to Redis, expires in {Seconds}s", token.ExpiresIn);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving token to Redis");
            throw;
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            await _database.KeyDeleteAsync(_tokenKey);
            _logger.LogInformation("Token cleared from Redis");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing token from Redis");
        }
    }

    public async Task<string?> TryAcquireRefreshLockAsync(TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        try
        {
            var lockToken = Guid.NewGuid().ToString("N");
            var acquired = await _database.LockTakeAsync(_refreshLockKey, lockToken, expiry);
            return acquired ? lockToken : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring refresh lock");
            return null;
        }
    }

    public async Task ReleaseRefreshLockAsync(string lockToken)
    {
        try
        {
            await _database.LockReleaseAsync(_refreshLockKey, lockToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing refresh lock");
        }
    }

    /// <summary>
    /// Token 数据结构（用于序列化）
    /// </summary>
    private class TokenData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
