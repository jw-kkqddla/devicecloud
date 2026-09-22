using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DeviceCloud.Domain.Entities.Products;
using DeviceCloud.Domain.Services.Products;
using DeviceCloud.Infrastructure.Options;

namespace DeviceCloud.Infrastructure.Services.Products;

/// <summary>
/// 产品 API 服务实现
/// </summary>
public class ProductApiService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly QuectelOptions _options;
    private readonly ILogger<ProductApiService> _logger;

    public ProductApiService(
        HttpClient httpClient,
        IOptions<QuectelOptions> options,
        ILogger<ProductApiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetProductsAsync(string? productName = null, string? productKey = null, int? status = null, int pageNum = 1, int pageSize = 20)
    {
        var url = $"{_options.BaseUrl}/v2/quecproductmgr/r3/openapi/products?pageNum={pageNum}&pageSize={pageSize}";

        if (!string.IsNullOrEmpty(productName))
        {
            url += $"&productName={Uri.EscapeDataString(productName)}";
        }

        if (!string.IsNullOrEmpty(productKey))
        {
            url += $"&productKey={Uri.EscapeDataString(productKey)}";
        }

        if (status.HasValue)
        {
            url += $"&status={status.Value}";
        }

        _logger.LogInformation("Getting products list, pageNum: {PageNum}, pageSize: {PageSize}", pageNum, pageSize);

        var request = new HttpRequestMessage(HttpMethod.Get, url);
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

    public async Task<ProductTsl> GetProductTslAsync(string productKey)
    {
        var url = $"{_options.BaseUrl}/v2/quectsl/openapi/product/export/tslFile?productKey={Uri.EscapeDataString(productKey)}";

        _logger.LogInformation("Getting product TSL for: {ProductKey}", productKey);

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API Error: {Content}", content);
            throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
        }

        // 直接返回原始 JSON 数据
        return ProductTsl.Create(productKey, content);
    }

    public async Task<string> GetProductDetailAsync(string productKey)
    {
        var url = $"{_options.BaseUrl}/v2/quecproductmgr/r3/openapi/product/detail?productKey={Uri.EscapeDataString(productKey)}";

        _logger.LogInformation("Getting product detail for: {ProductKey}", productKey);

        var request = new HttpRequestMessage(HttpMethod.Get, url);
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
