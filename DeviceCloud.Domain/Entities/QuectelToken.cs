namespace DeviceCloud.Domain.Entities;

/// <summary>
/// 平台认证令牌聚合根
/// </summary>
public class QuectelToken
{
    public string AccessToken { get; private set; } = string.Empty;
    public string TokenType { get; private set; } = string.Empty;
    public int ExpiresIn { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt => CreatedAt.AddSeconds(ExpiresIn);

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    protected QuectelToken() { }

    public static QuectelToken Create(string accessToken, string tokenType, int expiresIn, string? refreshToken = null)
    {
        return new QuectelToken
        {
            AccessToken = accessToken,
            TokenType = tokenType,
            ExpiresIn = expiresIn,
            RefreshToken = refreshToken,
            CreatedAt = DateTime.UtcNow
        };
    }

    public string GetAuthorizationHeader()
    {
        return AccessToken;
    }
}
