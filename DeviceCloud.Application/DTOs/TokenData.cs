namespace DeviceCloud.Application.DTOs;

/// <summary>
/// 令牌数据
/// </summary>
public record TokenData
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// 令牌类型
    /// </summary>
    public string TokenType { get; init; } = string.Empty;

    /// <summary>
    /// 有效期（秒）
    /// </summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string? RefreshToken { get; init; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime? ExpiresAt { get; init; }
}
