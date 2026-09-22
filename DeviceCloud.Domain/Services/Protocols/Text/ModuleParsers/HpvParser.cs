namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HPV（PV 电压）模块解析器。协议 5.8。
/// 响应格式：(AAA.A BB.B CCCCC DDDDD.D EEEEE F GGG.G H OOOOOOO
/// </summary>
public class HpvParser : IModuleParser
{
    public string Code => "HPV";

    private static readonly IReadOnlyDictionary<string, string> MpptModeMap = new Dictionary<string, string>
    {
        ["0"] = "停止", ["1"] = "使能",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["PV电压"] = TextFieldParser.NumberWithUnit(v[0].TrimStart('('), "V");
            result["PV电流"] = TextFieldParser.NumberWithUnit(v[1], "A");
            result["PV功率"] = TextFieldParser.NumberWithUnit(v[2], "W");
            result["内部数据1"] = TextFieldParser.Number(v[3]);
            result["内部数据2"] = TextFieldParser.Number(v[4]);
            result["内部数据3"] = TextFieldParser.Number(v[5]);
            result["内部数据4"] = TextFieldParser.Number(v[6]);
            result["MPPT恒温模式"] = TextFieldParser.Map(TextFieldParser.BitAt(v[7], 0), MpptModeMap);
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
