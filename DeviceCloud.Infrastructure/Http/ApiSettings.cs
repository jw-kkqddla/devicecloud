namespace DeviceCloud.Infrastructure.Http;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public int DefaultLimit { get; set; } = 20;
    public int TimeoutSeconds { get; set; } = 30;
}