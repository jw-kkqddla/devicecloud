namespace DeviceCloud.Application.DTOs;

public record MessageDisplayDto(
    int Index,
    string MsgTypeLabel,
    int PropertyId,
    string PropertyName,
    string PropertyCode,
    string? RawValue,
    string DisplayValue,
    DateTime CreateTime,
    string Ticket,
    bool IsUp
);