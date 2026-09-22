using DeviceCloud.Application.Interfaces;
using DeviceCloud.Domain.Services.Protocols.Text;

namespace DeviceCloud.Application.Services;

/// <summary>
/// 指令编码应用服务实现。
/// </summary>
public class CommandEncodingService : ICommandEncodingService
{
    public bool SupportsSetting(string setting)
        => HeepCommandEncoder.Supports(setting);

    public string EncodeSetting(string setting, string value, string machineType = "")
    {
        var command = HeepCommandEncoder.Encode(setting, value, machineType);
        var bytes = TqfCrc16.BuildFrame(command);
        return TqfCrc16.ToHexString(bytes);
    }

    public string? ResolveMachineTypeGroup(string rawResponse)
        => MachineTypeResolver.ResolveGroup(rawResponse);

    public string? ResolveMachineModel(string rawResponse)
        => MachineTypeResolver.Resolve(rawResponse);
}
