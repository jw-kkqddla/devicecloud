namespace DeviceCloud.Application.DTOs.Devices;

/// <summary>
/// 属性分组
/// </summary>
public class PropertyFieldGroup
{
    public string GroupName { get; set; } = string.Empty;
    public List<PropertyFieldItem> Fields { get; set; } = new();
}

/// <summary>
/// 属性字段项（字段名 → 值）
/// </summary>
public class PropertyFieldItem
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// 物模型属性视图模型
/// </summary>
public class TslPropertyViewModel
{
    public string Identifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    public int? PropId { get; set; }
}

/// <summary>
/// 设备最新值的解析后缓存（物模型 id -> 值 + 最近上行时间），命中后无需重复解析接口原始数据
/// </summary>
public class LatestValuesCache
{
    /// <summary>
    /// 物模型属性数字 id -> 值
    /// </summary>
    public Dictionary<int, string> Values { get; set; } = new();

    /// <summary>
    /// 最近一次上行时间（毫秒）
    /// </summary>
    public long? UpdateTime { get; set; }
}
