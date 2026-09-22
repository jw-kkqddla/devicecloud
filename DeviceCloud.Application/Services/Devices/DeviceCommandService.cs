using System.Text.Json;
using DeviceCloud.Application.DTOs.Devices;
using DeviceCloud.Application.Interfaces;
using DeviceCloud.Application.Interfaces.Devices;
using DeviceCloud.Domain.Services.Devices;
using Microsoft.Extensions.Logging;

namespace DeviceCloud.Application.Services.Devices;

/// <summary>
/// 设备指令服务实现。
/// 从 DetailModel 抽取：读取设备数据、下发设备指令、下发设置指令。
/// </summary>
public class DeviceCommandService : IDeviceCommandService
{
    private readonly IDeviceAppService _deviceAppService;
    private readonly IDeviceService _deviceService;
    private readonly ICommandEncodingService _commandEncodingService;
    private readonly ILogger<DeviceCommandService> _logger;

    public DeviceCommandService(
        IDeviceAppService deviceAppService,
        IDeviceService deviceService,
        ICommandEncodingService commandEncodingService,
        ILogger<DeviceCommandService> logger)
    {
        _deviceAppService = deviceAppService;
        _deviceService = deviceService;
        _commandEncodingService = commandEncodingService;
        _logger = logger;
    }

    public async Task<CommandResult> ReadDataAsync(ReadDeviceDataRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductKey))
            return CommandResult.Fail("产品 Key 不能为空");

        if (request.Devices == null || request.Devices.Count == 0)
            return CommandResult.Fail("设备列表不能为空");

        if (request.Data == null || request.Data.Count == 0)
            return CommandResult.Fail("物模型标识符列表不能为空");

        var result = await _deviceAppService.ReadDeviceDataAsync(request.ProductKey, request.Devices, request.DataJson);

        if (result.Success)
            return CommandResult.Ok(result.Message, result.Data);

        return new CommandResult { Success = false, Message = result.Message, ErrorCode = result.ErrorCode };
    }

    public async Task<CommandResult> SendCommandAsync(SendCommandRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductKey))
            return CommandResult.Fail("产品Key不能为空");

        if (string.IsNullOrWhiteSpace(request.DeviceKey))
            return CommandResult.Fail("设备Key不能为空");

        if (string.IsNullOrWhiteSpace(request.Data))
            return CommandResult.Fail("指令内容不能为空");

        try
        {
            // 调用设备服务下发指令
            var resultJson = await _deviceService.SendDeviceDataAsync(
                request.ProductKey,
                request.DeviceKey,
                request.Data,
                request.Encode ?? "Text",
                request.IsCache ?? false,
                request.IsCover ?? false,
                request.Qos,
                request.CacheTime
            );

            // 如果返回为空，视为失败
            if (string.IsNullOrWhiteSpace(resultJson))
                return CommandResult.Fail("指令下发失败：服务返回空");

            // 尝试解析返回的 JSON，判断是否成功
            try
            {
                using var doc = JsonDocument.Parse(resultJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("code", out var codeEl))
                {
                    var code = codeEl.GetInt32();
                    if (code == 0 || code == 200)
                    {
                        var msg = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "指令下发成功";
                        return new CommandResult { Success = true, Message = msg, Data = resultJson };
                    }

                    var errMsg = root.TryGetProperty("message", out var m) ? m.GetString() : $"指令下发失败，错误码: {code}";
                    return new CommandResult { Success = false, Message = errMsg, Code = code };
                }
                else if (root.TryGetProperty("success", out var successEl) && successEl.GetBoolean())
                {
                    return new CommandResult { Success = true, Message = "指令下发成功", Data = resultJson };
                }
                else
                {
                    // 没有明确的状态码，但返回了内容，视为成功
                    return new CommandResult { Success = true, Message = "指令已下发", Data = resultJson };
                }
            }
            catch (JsonException)
            {
                // 返回的不是 JSON，但字符串不为空，视为成功
                return new CommandResult { Success = true, Message = "指令已下发", Data = resultJson };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设备指令下发失败, ProductKey={ProductKey}, DeviceKey={DeviceKey}, Data={Data}",
                request.ProductKey, request.DeviceKey, request.Data);
            return CommandResult.Fail($"指令下发失败: {ex.Message}");
        }
    }

    public async Task<CommandResult> SendSettingAsync(SendSettingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProductKey))
            return CommandResult.Fail("产品Key不能为空");

        if (string.IsNullOrWhiteSpace(request.DeviceKey))
            return CommandResult.Fail("设备Key不能为空");

        if (!_commandEncodingService.SupportsSetting(request.Setting))
            return CommandResult.Fail($"不支持的设置项: {request.Setting}");

        try
        {
            // 编码成含 CRC + 回车的 Hex 指令，透传下发
            var hex = _commandEncodingService.EncodeSetting(request.Setting, request.Value, request.MachineType ?? "");
            _logger.LogInformation("下发设置指令: {Setting}={Value} -> {Hex}", request.Setting, request.Value, hex);

            var resultJson = await _deviceService.SendDeviceDataAsync(
                request.ProductKey, request.DeviceKey, hex, "Hex",
                request.IsCache ?? false, request.IsCover ?? false, request.Qos, request.CacheTime);

            if (string.IsNullOrWhiteSpace(resultJson))
                return CommandResult.Fail("指令下发失败：服务返回空");

            try
            {
                using var doc = JsonDocument.Parse(resultJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("code", out var codeEl))
                {
                    var code = codeEl.GetInt32();
                    if (code == 0 || code == 200)
                    {
                        var msg = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "设置指令下发成功";
                        return new CommandResult { Success = true, Message = msg, Hex = hex };
                    }

                    var errMsg = root.TryGetProperty("message", out var m2) ? m2.GetString() : $"下发失败，错误码: {code}";
                    return new CommandResult { Success = false, Message = errMsg, Code = code };
                }

                return new CommandResult { Success = true, Message = "指令已下发", Hex = hex };
            }
            catch (JsonException)
            {
                return new CommandResult { Success = true, Message = "指令已下发", Hex = hex };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置指令下发失败: {Setting}={Value}", request.Setting, request.Value);
            return CommandResult.Fail($"设置指令下发失败: {ex.Message}");
        }
    }
}
