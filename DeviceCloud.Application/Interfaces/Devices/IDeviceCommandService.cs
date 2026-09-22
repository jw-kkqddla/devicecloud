using DeviceCloud.Application.DTOs.Devices;

namespace DeviceCloud.Application.Interfaces.Devices;

/// <summary>
/// 设备指令服务接口。
/// 负责读取设备数据、下发设备指令（Text/Hex/Base64）、下发设置指令（编码 + 透传）。
/// </summary>
public interface IDeviceCommandService
{
    /// <summary>
    /// 读取设备物模型数据（调试用）。
    /// </summary>
    Task<CommandResult> ReadDataAsync(ReadDeviceDataRequest request);

    /// <summary>
    /// 下发设备指令（Text 格式等）。
    /// </summary>
    Task<CommandResult> SendCommandAsync(SendCommandRequest request);

    /// <summary>
    /// 下发 HEEP1 设置指令（枚举/数值 → 指令 → 透传下发）。
    /// </summary>
    Task<CommandResult> SendSettingAsync(SendSettingRequest request);
}
