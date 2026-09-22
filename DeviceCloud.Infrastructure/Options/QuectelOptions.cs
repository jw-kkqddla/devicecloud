namespace DeviceCloud.Infrastructure.Options;

/// <summary>
/// API 配置选项
/// </summary>
public class QuectelOptions
{
    public const string SectionName = "Quectel";

    public string BaseUrl { get; set; } = "https://iot-api.quectelcn.com";
    public string AccessKey { get; set; } = string.Empty;
    public string AccessSecret { get; set; } = string.Empty;
}
