namespace DeviceCloud.Domain.Services.Devices;

/// <summary>
/// 设备领域服务接口
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// 获取设备列表
    /// </summary>
    Task<string> GetDevicesAsync(string productKey, string? deviceKey = null, string? deviceName = null, int pageNum = 1, int pageSize = 20);

    /// <summary>
    /// 获取设备详情
    /// </summary>
    Task<string> GetDeviceDetailAsync(string productKey, string deviceKey);

    /// <summary>
    /// 创建设备
    /// </summary>
    Task<string> CreateDeviceAsync(string productKey, string deviceKey, string? deviceName = null, string? sn = null, int? authMode = null, string? psk = null, string? fingerPrint = null);

    /// <summary>
    /// 删除设备
    /// </summary>
    Task<string> DeleteDeviceAsync(string productKey, string deviceKey);

    /// <summary>
    /// 更新设备信息
    /// </summary>
    Task<string> UpdateDeviceAsync(string productKey, string deviceKey, string? deviceName = null, string? sn = null);

    /// <summary>
    /// 读取设备物模型数据
    /// </summary>
    /// <param name="productKey">产品Key</param>
    /// <param name="devices">设备Key列表</param>
    /// <param name="data">物模型标识符列表（JSON数组字符串）</param>
    Task<string> ReadDeviceDataAsync(string productKey, List<string> devices, string data);

    Task<string> GetDeviceHistoryDataAsync(string productKey, string deviceKey, long startTime, long endTime, List<string>? identifiers = null, int? limit = null, int? offset = null);
 
    Task<string> GetDevicePropertyHistoryAsync(string productKey, string deviceKey, string code, long startTime, long endTime, int? pageSize = null);

    Task<string> SendDeviceDataAsync(string productKey, string deviceKey, string data, string encode = "Text",bool isCache = false, bool isCover = false, int? qos = null, int? cacheTime = null);

    Task<string> WriteDeviceDataAsync(string productKey, List<string> deviceKeys, string data, bool isCache = false,bool isCover = false, int? qos = null, int? cacheTime = null);
}
