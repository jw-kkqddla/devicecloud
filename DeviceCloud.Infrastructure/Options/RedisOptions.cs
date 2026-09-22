namespace DeviceCloud.Infrastructure.Options;

/// <summary>
/// Redis 配置选项
/// </summary>
public class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// Redis 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Token 键名前缀
    /// </summary>
    public string TokenKeyPrefix { get; set; } = "quectel:token";

    /// <summary>
    /// 默认过期时间（秒）
    /// </summary>
    public int DefaultExpirySeconds { get; set; } = 7200;
}
