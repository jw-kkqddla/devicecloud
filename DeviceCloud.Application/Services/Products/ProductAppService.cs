using System.Text.Json;
using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs.Products;
using DeviceCloud.Application.Interfaces.Products;
using DeviceCloud.Domain.Services;
using DeviceCloud.Domain.Services.Products;

namespace DeviceCloud.Application.Services.Products;

/// <summary>
/// 产品应用服务实现
/// </summary>
public class ProductAppService : IProductAppService
{
    private readonly IProductService _productService;
    private readonly ICacheService _cacheService;

    public ProductAppService(IProductService productService, ICacheService cacheService)
    {
        _productService = productService;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<List<ProductListDto>>> GetProductsAsync(string? productName = null, string? productKey = null, int? status = null, int pageNum = 1, int pageSize = 20)
    {
        var cacheKey = $"dc:cache:products:{productName ?? ""}:{productKey ?? ""}:{status?.ToString() ?? ""}:{pageNum}:{pageSize}";

        var cached = await _cacheService.GetAsync<ApiResponse<List<ProductListDto>>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        try
        {
            var jsonContent = await _productService.GetProductsAsync(productName, productKey, status, pageNum, pageSize);

            var response = JsonSerializer.Deserialize<ProductListResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ApiResponse<List<ProductListDto>> result;
            if (response == null)
            {
                result = ApiResponse.Fail<List<ProductListDto>>("API 响应解析失败", "PARSE_ERROR");
            }
            else if (response.Code != 200)
            {
                result = ApiResponse.Fail<List<ProductListDto>>(response.Msg ?? "获取产品列表失败", "API_ERROR");
            }
            else
            {
                result = ApiResponse.Ok(response.Data ?? new List<ProductListDto>(), "获取产品列表成功");
            }

            if (result.Success)
            {
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            }

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<List<ProductListDto>>(ex.Message, "API_ERROR");
        }
    }

    public async Task<ApiResponse<ProductTslDto>> GetProductTslAsync(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ApiResponse.Fail<ProductTslDto>("产品 Key 不能为空", "INVALID_PARAM");
        }

        var cacheKey = $"dc:cache:product:tsl:{productKey}";

        var cached = await _cacheService.GetAsync<ApiResponse<ProductTslDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        try
        {
            var productTsl = await _productService.GetProductTslAsync(productKey);

            var data = new ProductTslDto
            {
                ProductKey = productTsl.ProductKey,
                TslData = productTsl.TslData,
                RetrievedAt = productTsl.CreatedAt
            };

            var result = ApiResponse.Ok(data, "获取物模型成功");

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromHours(1));

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<ProductTslDto>(ex.Message, "API_ERROR");
        }
    }

    public async Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(string productKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ApiResponse.Fail<ProductDetailDto>("产品 Key 不能为空", "INVALID_PARAM");
        }

        var cacheKey = $"dc:cache:product:detail:{productKey}";

        var cached = await _cacheService.GetAsync<ApiResponse<ProductDetailDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        try
        {
            var jsonContent = await _productService.GetProductDetailAsync(productKey);

            var response = JsonSerializer.Deserialize<ProductDetailResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ApiResponse<ProductDetailDto> result;
            if (response == null)
            {
                result = ApiResponse.Fail<ProductDetailDto>("API 响应解析失败", "PARSE_ERROR");
            }
            else if (response.Code != 200)
            {
                result = ApiResponse.Fail<ProductDetailDto>(response.Msg ?? "获取产品详情失败", "API_ERROR");
            }
            else
            {
                result = ApiResponse.Ok(response.Data!, "获取产品详情成功");
            }

            if (result.Success)
            {
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
            }

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<ProductDetailDto>(ex.Message, "API_ERROR");
        }
    }
}
