using DeviceCloud.Domain.ValueObjects;

namespace DeviceCloud.Domain.Entities;

public class PropertyDefinition
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string SubType { get; set; } = string.Empty;
    public PropertySpecs? Specs { get; set; }
    public int Sort { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Desc { get; set; }

    // 领域行为：格式化显示值
    public string FormatDisplayValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "—";

        return DataType switch
        {
            "FLOAT" when float.TryParse(value, out var fVal)
                => $"{fVal:F1}{(string.IsNullOrEmpty(Specs?.Unit) ? "" : $" {Specs.Unit}")}",
            "INT" => $"{value}{(string.IsNullOrEmpty(Specs?.Unit) ? "" : $" {Specs.Unit}")}",
            "BOOL" => value is "true" or "1" ? "开启" : "关闭",
            "TEXT" => string.IsNullOrEmpty(value) ? "（空文本）" : value,
            _ => value
        };
    }
}