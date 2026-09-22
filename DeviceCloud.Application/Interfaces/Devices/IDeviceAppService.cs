using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs.Devices;

namespace DeviceCloud.Application.Interfaces.Devices;

/// <summary>
/// 设备应用服务接口
/// </summary>
public interface IDeviceAppService
{
    /// <summary>
    /// 获取设备列表
    /// </summary>
    Task<ApiResponse<List<DeviceListDto>>> GetDevicesAsync(string productKey, string? deviceKey = null, string? deviceName = null, int pageNum = 1, int pageSize = 20);

    /// <summary>
    /// 获取设备详情
    /// </summary>
    Task<ApiResponse<DeviceDetailDto>> GetDeviceDetailAsync(string productKey, string deviceKey);

    /// <summary>
    /// 创建设备
    /// </summary>
    Task<ApiResponse<DeviceCreateData>> CreateDeviceAsync(string productKey, string deviceKey, string? deviceName = null, string? sn = null, int? authMode = null, string? psk = null, string? fingerPrint = null);

    /// <summary>
    /// 删除设备
    /// </summary>
    Task<ApiResponse<object>> DeleteDeviceAsync(string productKey, string deviceKey);

    /// <summary>
    /// 更新设备信息
    /// </summary>
    Task<ApiResponse<object>> UpdateDeviceAsync(string productKey, string deviceKey, string? deviceName = null, string? sn = null);

    /// <summary>
    /// 读取设备物模型数据
    /// </summary>
    /// <param name="productKey">产品Key</param>
    /// <param name="devices">设备Key列表</param>
    /// <param name="data">物模型标识符列表（JSON数组字符串）</param>
    Task<ApiResponse<List<DeviceDataItem>>> ReadDeviceDataAsync(string productKey, List<string> devices, string data);
}
