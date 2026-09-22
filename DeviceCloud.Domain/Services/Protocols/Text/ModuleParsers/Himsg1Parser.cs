namespace DeviceCloud.Domain.Services.Protocols.Text.ModuleParsers;

/// <summary>
/// HIMSG1 模块解析器（软件版本号与发行日期）。协议 5.4。
/// 响应格式：(NNNN.NN AAAABBCC DD
/// 示例：(0000.03 20230220 00 → 版本 0000.03，日期 2023-02-20。
/// </summary>
public class Himsg1Parser : IModuleParser
{
    public string Code => "HIMSG1";

    public IReadOnlyDictionary<string, string> Parse(string rawValue)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(rawValue) || TextFieldParser.IsCrcError(rawValue))
            return result;

        var v = TextFieldParser.SplitFields(rawValue);

        try
        {
            // 软件版本：Values[0] 去掉起始位 '('
            result["软件版本"] = TextFieldParser.Number(v[0].TrimStart('('));

            // 发行日期：Values[1] = AAAABBCC（年/月/日）
            var date = v[1];
            if (date.Length >= 8)
            {
                result["发行日期"] = $"{date.Substring(0, 4)}-{date.Substring(4, 2)}-{date.Substring(6, 2)}";
            }
            else
            {
                result["发行日期"] = date;
            }
        }
        catch (Exception)
        {
            // 忽略解析异常
        }

        return result;
    }
}
