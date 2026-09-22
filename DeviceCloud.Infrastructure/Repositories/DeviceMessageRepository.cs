namespace DeviceCloud.Domain.ValueObjects;

public class PropertySpecs
{
    public string? Unit { get; set; }
    public string? Min { get; set; }
    public string? Max { get; set; }
    public string? Step { get; set; }
    public string? Length { get; set; }
    public List<BoolSpec>? Specs { get; set; }
}

public class BoolSpec
{
    public string DataType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}