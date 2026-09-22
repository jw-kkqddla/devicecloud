using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs.Products;

namespace DeviceCloud.Application.Interfaces.Products;

/// <summary>
/// 产品应用服务接口
/// </summary>
public interface IProductAppService
{
    /// <summary>
    /// 获取产品列表
    /// </summary>
    Task<ApiResponse<List<ProductListDto>>> GetProductsAsync(string? productName = null, string? productKey = null, int? status = null, int pageNum = 1, int pageSize = 20);

    /// <summary>
    /// 获取产品物模型
    /// </summary>
    Task<ApiResponse<ProductTslDto>> GetProductTslAsync(string productKey);

    /// <summary>
    /// 获取产品详情
    /// </summary>
    Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(string productKey);
}
