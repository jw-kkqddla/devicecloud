namespace DeviceCloud.Application.DTOs.Products;

/// <summary>
/// 产品物模型 DTO
/// </summary>
public record ProductTslDto
{
    /// <summary>
    /// 产品 Key
    /// </summary>
    public string ProductKey { get; init; } = string.Empty;

    /// <summary>
    /// 物模型数据
    /// </summary>
    public string TslData { get; init; } = string.Empty;

    /// <summary>
    /// 获取时间
    /// </summary>
    public DateTime? RetrievedAt { get; init; }
}
