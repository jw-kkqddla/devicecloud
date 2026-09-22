namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HEEP1（主控）模块解析器。协议 5.10。
/// 响应格式：(A BBB CCC DEFGHIJeKKK LMN P Q R S T UUU VVV WWW XXX YYY.Y ZZZ.Z aaa.a bbb c d
/// 注：工作模式 A、充电优先顺序 N 等枚举与机型相关（A/C 与 B/D/E 机型不同），此处采用 A/C 机型约定。
/// </summary>
public class Heep1Parser : IModuleParser
{
    public string Code => "HEEP1";

    private static readonly IReadOnlyDictionary<string, string> WorkModeMap = new Dictionary<string, string>
    {
        ["0"] = "UTI", ["1"] = "SUB", ["2"] = "SBU",
    };

    private static readonly IReadOnlyDictionary<string, string> AcRangeMap = new Dictionary<string, string>
    {
        ["0"] = "APL", ["1"] = "UPS",
    };

    private static readonly IReadOnlyDictionary<string, string> GridProtoMap = new Dictionary<string, string>
    {
        ["0"] = "India", ["1"] = "German", ["2"] = "SouthAmerica",
    };

    private static readonly IReadOnlyDictionary<string, string> BattTypeMap = new Dictionary<string, string>
    {
        ["0"] = "AGM", ["1"] = "FLD", ["2"] = "USER",
    };

    private static readonly IReadOnlyDictionary<string, string> PvPriorityMap = new Dictionary<string, string>
    {
        ["0"] = "BLU", ["1"] = "LBU",
    };

    private static readonly IReadOnlyDictionary<string, string> OnOffMap = new Dictionary<string, string>
    {
        ["0"] = "关闭", ["1"] = "打开",
    };

    private static readonly IReadOnlyDictionary<string, string> EnableMap = new Dictionary<string, string>
    {
        ["0"] = "失能", ["1"] = "使能",
    };

    private static readonly IReadOnlyDictionary<string, string> FreqMap = new Dictionary<string, string>
    {
        ["0"] = "50Hz", ["1"] = "60Hz",
    };

    private static readonly IReadOnlyDictionary<string, string> ChargePriorityMap = new Dictionary<string, string>
    {
        ["0"] = "CUT", ["1"] = "CSO", ["2"] = "SNU", ["3"] = "OSO",
    };

    private static readonly IReadOnlyDictionary<string, string> OutputModeMap = new Dictionary<string, string>
    {
        ["0"] = "SIG", ["1"] = "PAL", ["2"] = "3P1", ["3"] = "3P2", ["4"] = "3P3",
    };

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            result["工作模式"] = TextFieldParser.Map(TextFieldParser.BitAt(v[0], 1), WorkModeMap);
            result["最大总充电电流"] = TextFieldParser.NumberWithUnit(v[1], "A");
            result["市电最大充电电流"] = TextFieldParser.NumberWithUnit(v[2], "A");

            // Values[3]：DEFGHIJeKKK
            result["市电输入范围"] = TextFieldParser.Map(Bit(v, 3, 0), AcRangeMap);
            result["PV并网协议"] = TextFieldParser.Map(Bit(v, 3, 1), GridProtoMap);
            result["电池类型"] = TextFieldParser.Map(Bit(v, 3, 2), BattTypeMap);
            result["PV馈能优先级"] = TextFieldParser.Map(Bit(v, 3, 3), PvPriorityMap);
            result["过载重启"] = TextFieldParser.Map(Bit(v, 3, 4), OnOffMap);
            result["输入源提示功能"] = TextFieldParser.Map(Bit(v, 3, 5), OnOffMap);
            result["过温重启功能"] = TextFieldParser.Map(Bit(v, 3, 6), OnOffMap);
            result["CT功能开关"] = TextFieldParser.Map(Bit(v, 3, 7), OnOffMap);
            result["输出设定电压"] = TextFieldParser.NumberWithUnit(TextFieldParser.Substring(v[3], 8, 3), "V");

            // Values[4]：LMN
            result["输出设定频率"] = TextFieldParser.Map(Bit(v, 4, 0), FreqMap);
            result["自动返回第一页功能"] = TextFieldParser.Map(Bit(v, 4, 1), OnOffMap);
            result["充电优先顺序"] = TextFieldParser.Map(Bit(v, 4, 2), ChargePriorityMap);

            result["蜂鸣器功能"] = TextFieldParser.Map(TextFieldParser.BitAt(v[5], 0), EnableMap);
            result["LCD背光"] = TextFieldParser.Map(TextFieldParser.BitAt(v[6], 0), EnableMap);
            result["过载转旁路功能"] = TextFieldParser.Map(TextFieldParser.BitAt(v[7], 0), OnOffMap);
            result["输出模式"] = TextFieldParser.Map(TextFieldParser.BitAt(v[8], 0), OutputModeMap);
            result["BMS通信控制功能"] = TextFieldParser.Map(TextFieldParser.BitAt(v[9], 0), OnOffMap);

            result["BMS低电SOC"] = TextFieldParser.NumberWithUnit(v[10], "%");
            result["BMS返回市电模式SOC"] = TextFieldParser.NumberWithUnit(v[11], "%");
            result["BMS返回电池模式SOC"] = TextFieldParser.NumberWithUnit(v[12], "%");
            result["BMS低电后自动开机SOC"] = TextFieldParser.NumberWithUnit(v[13], "%");
            result["强充电压"] = TextFieldParser.NumberWithUnit(v[14], "V");
            result["浮充电压"] = TextFieldParser.NumberWithUnit(v[15], "V");
            result["低电锁机电压"] = TextFieldParser.NumberWithUnit(v[16], "V");
            result["并网电流"] = TextFieldParser.NumberWithUnit(v[17], "A");
            result["并网功能"] = TextFieldParser.Map(TextFieldParser.BitAt(v[18], 0), EnableMap);
            result["ECO"] = TextFieldParser.Map(TextFieldParser.BitAt(v[19], 0), EnableMap);
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
