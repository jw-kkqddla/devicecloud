namespace DeviceCloud.Domain.ValueObjects;

public record PropertySpecs
{
    public string? Unit { get; }
    public string? Min { get; }
    public string? Max { get; }
    public string? Step { get; }
    public string? Length { get; }
    public IReadOnlyList<BoolSpec>? Specs { get; }

    // 主构造函数
    public PropertySpecs(
        string? unit = null,
        string? min = null,
        string? max = null,
        string? step = null,
        string? length = null,
        IReadOnlyList<BoolSpec>? specs = null)
    {
        Unit = unit;
        Min = min;
        Max = max;
        Step = step;
        Length = length;
        Specs = specs;
    }
}

public record BoolSpec
{
    public string DataType { get; }
    public string Name { get; }
    public string Value { get; }

    public BoolSpec(string dataType, string name, string value)
    {
        DataType = dataType;
        Name = name;
        Value = value;
    }
}