namespace DeviceCloud.Application.DTOs.Devices;

/// <summary>
/// 设备详情页查询结果快照：最新值 + 更新时间 + 识别出的机型 + 字段分组。
/// </summary>
public class DeviceDataSnapshot
{
    /// <summary>
    /// 物模型属性数字 id -> 最新值
    /// </summary>
    public Dictionary<int, string> Values { get; set; } = new();

    /// <summary>
    /// 最近一次上行时间（毫秒）
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 识别出的具体机型（如 "HPVINV04"）
    /// </summary>
    public string? MachineType { get; set; }

    /// <summary>
    /// 识别出的机型分组（"A"=A/C 组、"B"=B/D 组）
    /// </summary>
    public string? MachineTypeGroup { get; set; }

    /// <summary>
    /// 按固定字段清单归组后的展示字段
    /// </summary>
    public List<PropertyFieldGroup> FieldGroups { get; set; } = new();
}

/// <summary>
/// 历史曲线数据点（时间 + 数值）
/// </summary>
public class HistoryPoint
{
    public long Time { get; set; }
    public double Value { get; set; }
}

/// <summary>
/// 历史曲线查询结果
/// </summary>
public class HistorySeries
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<HistoryPoint> Points { get; set; } = new();
    public string PropertyName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    public static HistorySeries Fail(string message) => new() { Success = false, Message = message };
}

/// <summary>
/// 月发电量柱状图查询结果
/// </summary>
public class DailyEnergySeries
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string> Labels { get; set; } = new();
    public List<double> Values { get; set; } = new();
    public string Unit { get; set; } = "kWh";

    public static DailyEnergySeries Fail(string message) => new() { Success = false, Message = message };
}

/// <summary>
/// 前端下拉框展示的单个图表属性。
/// </summary>
public class ChartPropertyOption
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// 前端下拉框展示的属性分组（一个模块一组）。
/// </summary>
public class ChartPropertyGroup
{
    public string Module { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<ChartPropertyOption> Items { get; set; } = new();
}

/// <summary>
/// 数据明细表头列定义。
/// </summary>
public class DataDetailColumn
{
    public string Label { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// 数据明细单行（对应一条设备上行记录）。
/// </summary>
public class DataDetailRow
{
    /// <summary>上报时间（毫秒时间戳）</summary>
    public long Time { get; set; }

    /// <summary>与 Columns 顺序对齐的单元格值（缺失用空串占位）</summary>
    public List<string> Cells { get; set; } = new();
}

/// <summary>
/// 数据明细查询结果：每一条上报记录解析为一行。
/// </summary>
public class DataDetailResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<DataDetailColumn> Columns { get; set; } = new();
    public List<DataDetailRow> Rows { get; set; } = new();

    public static DataDetailResult Fail(string message) => new() { Success = false, Message = message };
}
