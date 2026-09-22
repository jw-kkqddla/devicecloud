using DeviceCloud.Domain.Entities.Products;

namespace DeviceCloud.Domain.Services.Products;

/// <summary>
/// 产品领域服务接口
/// </summary>
public interface IProductService
{
    /// <summary>
    /// 获取产品列表
    /// </summary>
    Task<string> GetProductsAsync(string? productName = null, string? productKey = null, int? status = null, int pageNum = 1, int pageSize = 20);

    /// <summary>
    /// 获取产品物模型数据
    /// </summary>
    Task<ProductTsl> GetProductTslAsync(string productKey);

    /// <summary>
    /// 获取产品详情
    /// </summary>
    Task<string> GetProductDetailAsync(string productKey);
}
