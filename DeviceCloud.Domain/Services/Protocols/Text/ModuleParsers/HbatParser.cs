namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HBAT（电池）模块解析器。协议 5.7。
/// 响应格式：(AA BBB.B CCC DDD EEEEE FFF GHIJKLMOOOOOOOOOOOOOO
/// </summary>
public class HbatParser : IModuleParser
{
    public string Code => "HBAT";

    private static readonly IReadOnlyDictionary<string, string> OnOffMap = new Dictionary<string, string>
    {
        ["0"] = "关闭", ["1"] = "打开",
    };

    private static readonly IReadOnlyDictionary<string, string> ActiveMap = new Dictionary<string, string>
    {
        ["0"] = "停止", ["1"] = "运行中",
    };

    private static readonly IReadOnlyDictionary<string, string> BatStateMap = new Dictionary<string, string>
    {
        ["0"] = "无", ["1"] = "放电", ["2"] = "充电",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["电池节数"] = TextFieldParser.Number(TextFieldParser.Substring(v[0], 1, 2));
            result["电池电压"] = TextFieldParser.NumberWithUnit(v[1], "V");
            result["电池容量"] = TextFieldParser.NumberWithUnit(v[2], "%");
            result["电池充电电流"] = TextFieldParser.NumberWithUnit(v[3], "A");
            result["电池放电电流"] = TextFieldParser.NumberWithUnit(v[4], "A");
            result["BUS电压"] = TextFieldParser.NumberWithUnit(v[5], "V");

            // Values[6]：GHIJKLM + 其他标志
            result["充电总开关"] = TextFieldParser.Map(Bit(v, 6, 0), OnOffMap);
            result["太阳能充电开关"] = TextFieldParser.Map(Bit(v, 6, 1), OnOffMap);
            result["AC充电开关"] = TextFieldParser.Map(Bit(v, 6, 2), OnOffMap);
            result["锂电激活功能开关"] = TextFieldParser.Map(Bit(v, 6, 3), OnOffMap);
            result["锂电激活过程"] = TextFieldParser.Map(Bit(v, 6, 4), ActiveMap);
            result["内部数据"] = Bit(v, 6, 5);
            result["电池状态"] = TextFieldParser.Map(Bit(v, 6, 6), BatStateMap);
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }

    private static string Bit(string[] fields, int fieldIndex, int bitIndex)
        => fieldIndex < fields.Length ? TextFieldParser.BitAt(fields[fieldIndex], bitIndex) : string.Empty;
}
