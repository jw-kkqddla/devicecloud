namespace DeviceCloud.Domain.ValueObjects;

/// <summary>
/// AccessKey 凭证值对象
/// </summary>
public record AccessKeyCredential
{
    public string AccessKey { get; }
    public string AccessSecret { get; }

    public AccessKeyCredential(string accessKey, string accessSecret)
    {
        if (string.IsNullOrWhiteSpace(accessKey))
            throw new ArgumentException("AccessKey 不能为空", nameof(accessKey));
        if (string.IsNullOrWhiteSpace(accessSecret))
            throw new ArgumentException("AccessSecret 不能为空", nameof(accessSecret));

        AccessKey = accessKey;
        AccessSecret = accessSecret;
    }

    /// <summary>
    /// 构造签名字符串
    /// </summary>
    public string BuildSignString(long timestamp)
    {
        return $"ver=1&auth_mode=accessKey&sign_method=sha256&access_key={AccessKey}&timestamp={timestamp}";
    }
}
