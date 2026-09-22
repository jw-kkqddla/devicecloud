using DeviceCloud.Application.DTOs.Devices;

namespace DeviceCloud.Application.Interfaces.Devices;

/// <summary>
/// 设备详情页查询服务接口。
/// 负责物模型解析、最新值加载（readData 优先 + 历史回退 + 缓存）、字段分组、
/// 机型识别、图表属性映射、历史曲线与月发电量查询。
/// </summary>
public interface IDeviceDetailQueryService
{
    /// <summary>
    /// 解析物模型 JSON，返回属性列表（按名称排序）。
    /// </summary>
    List<TslPropertyViewModel> ParseTslProperties(string tslData);

    /// <summary>
    /// 加载设备最新数据快照（readData 优先，失败回退历史数据），并识别机型、构建字段分组。
    /// </summary>
    Task<DeviceDataSnapshot> LoadSnapshotAsync(string productKey, string deviceKey, List<TslPropertyViewModel> tslProperties);

    /// <summary>
    /// 把 id -> 值 按固定字段清单归组展示（缺失字段空占位）。
    /// </summary>
    List<PropertyFieldGroup> BuildFieldGroups(List<TslPropertyViewModel> tslProperties, Dictionary<int, string> idMap, string? machineType);

    /// <summary>
    /// 图表属性分组（按模块），供前端下拉框按 optgroup 展示。
    /// </summary>
    IReadOnlyList<ChartPropertyGroup> ChartPropertyGroups { get; }

    /// <summary>
    /// 获取图表历史曲线数据。
    /// </summary>
    Task<HistorySeries> GetHistoryAsync(string productKey, string deviceKey, string property, string range);

    /// <summary>
    /// 获取月发电量柱状图数据（按天取当天最后一条「日发电量」）。
    /// </summary>
    Task<DailyEnergySeries> GetDailyEnergyAsync(string productKey, string deviceKey, string month, List<TslPropertyViewModel> tslProperties);

    /// <summary>
    /// 获取数据明细：时间区间内每一条上行（up）记录解析为一行，按时间倒序。
    /// </summary>
    Task<DataDetailResult> GetDataDetailAsync(string productKey, string deviceKey, long start, long end, List<TslPropertyViewModel> tslProperties);

}
