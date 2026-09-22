using DeviceCloud.Application.Interfaces;
using DeviceCloud.Domain.Services.Protocols.Text;
using DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

namespace DeviceCloud.Application.Services;

/// <summary>
/// 协议解析应用服务实现。
/// 维护「模块 code → 解析器」的路由表，把物模型属性的原始响应解析成具体字段。
/// </summary>
public class ProtocolParsingService : IProtocolParsingService
{
    private readonly Dictionary<string, IModuleParser> _parsers;

    public ProtocolParsingService()
    {
        _parsers = new IModuleParser[]
        {
            new HstsParser(),
            new Hsts2Parser(),
            new Himsg1Parser(),
            new HgridParser(),
            new HopParser(),
            new HbatParser(),
            new HpvParser(),
            new HpvbParser(),
            new HtempParser(),
            new Heep1Parser(),
            new Heep2Parser(),
            new Hbms1Parser(),
            new Hbms2Parser(),
            new Hbms3Parser(),
            new HgenParser(),
        }.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);
    }

    public bool Supports(string code)
        => !string.IsNullOrEmpty(code) && _parsers.ContainsKey(code);

    public IReadOnlyDictionary<string, string> Parse(string code, string rawValue)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(rawValue))
            return new Dictionary<string, string>();

        return _parsers.TryGetValue(code, out var parser)
            ? parser.Parse(rawValue)
            : new Dictionary<string, string>();
    }
}
