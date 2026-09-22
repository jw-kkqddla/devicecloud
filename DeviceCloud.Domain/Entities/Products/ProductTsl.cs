namespace DeviceCloud.Domain.Entities.Products;

/// <summary>
/// 产品物模型
/// </summary>
public class ProductTsl
{
    /// <summary>
    /// 产品 Key
    /// </summary>
    public string ProductKey { get; private set; } = string.Empty;

    /// <summary>
    /// 物模型数据（JSON 格式）
    /// </summary>
    public string TslData { get; private set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    protected ProductTsl() { }

    public static ProductTsl Create(string productKey, string tslData)
    {
        return new ProductTsl
        {
            ProductKey = productKey,
            TslData = tslData,
            CreatedAt = DateTime.UtcNow
        };
    }
}
