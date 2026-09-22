using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DeviceCloud.Domain.Services.Subscriptions;
using DeviceCloud.Infrastructure.Options;

namespace DeviceCloud.Infrastructure.Services.Subscriptions;

/// <summary>
/// 消息订阅 API 服务实现
/// </summary>
public class SubscriptionApiService : ISubscriptionService
{
    private readonly HttpClient _httpClient;
    private readonly QuectelOptions _options;
    private readonly ILogger<SubscriptionApiService> _logger;

    public SubscriptionApiService(
        HttpClient httpClient,
        IOptions<QuectelOptions> options,
        ILogger<SubscriptionApiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetQueuesAsync()
    {
        var url = $"{_options.BaseUrl}/v2/quecrule/r1/openapi/queue/list";

        _logger.LogInformation("Getting queue list");

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API Error: {Content}", content);
            throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
        }

        return content;
    }
}
