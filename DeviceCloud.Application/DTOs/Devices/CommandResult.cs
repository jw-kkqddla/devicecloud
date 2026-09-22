namespace DeviceCloud.Application.DTOs.Devices;

/// <summary>
/// 设备指令/读取操作结果。PageModel 据此还原原始 JSON 响应结构。
/// </summary>
public class CommandResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    /// <summary>
    /// 读取结果 / 指令下发原始响应（用于 readData 的 data、sendCommand 的 data）。
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 字符串错误码（readData 失败时的 ApiResponse.ErrorCode）。
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 上游返回的整数错误码（sendCommand / sendSetting 失败时）。
    /// </summary>
    public int? Code { get; set; }

    /// <summary>
    /// sendSetting 编码后的 Hex 指令。
    /// </summary>
    public string? Hex { get; set; }

    public static CommandResult Fail(string message) => new() { Success = false, Message = message };

    public static CommandResult Ok(string? message = null, object? data = null) =>
        new() { Success = true, Message = message, Data = data };
}
