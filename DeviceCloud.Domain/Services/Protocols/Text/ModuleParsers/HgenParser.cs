namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HGEN（发电量）模块解析器。协议 5.14。
/// 响应格式：(AAAAAA BB:BB CC.CCC DDDD.D EEEE.E FFFFFFFFF.F OOOOOOOOOOOO
/// </summary>
public class HgenParser : IModuleParser
{
    public string Code => "HGEN";

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["系统日期"] = TextFieldParser.Number(v[0].TrimStart('('));
            result["系统时间"] = v[1];
            result["日发电量"] = TextFieldParser.NumberWithUnit(v[2], "KW.h");
            result["月发电量"] = TextFieldParser.NumberWithUnit(v[3], "KW.h");
            result["年发电量"] = TextFieldParser.NumberWithUnit(v[4], "KW.h");
            result["总发电量"] = TextFieldParser.NumberWithUnit(v[5], "KW.h");
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
