using System.Text.Json.Serialization;

namespace DeviceCloud.Application.DTOs.Devices;

/// <summary>
/// 设备详情视图模型
/// </summary>
public class DeviceDetailViewModel
{
    /// <summary>
    /// 产品Key
    /// </summary>
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// 设备Key
    /// </summary>
    public string DeviceKey { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 序列号
    /// </summary>
    public string? Sn { get; set; }

    /// <summary>
    /// 设备二维码
    /// </summary>
    public string? DeviceQrCode { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public int IsActived { get; set; }

    /// <summary>
    /// 激活状态显示
    /// </summary>
    public string IsActivedDisplay => IsActived switch
    {
        1 => "已激活",
        _ => "未激活"
    };

    /// <summary>
    /// 激活时间
    /// </summary>
    public long? ActivedTime { get; set; }

    /// <summary>
    /// 激活时间显示
    /// </summary>
    public string? ActivedTimeDisplay => ActivedTime.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(ActivedTime.Value).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
        : null;

    /// <summary>
    /// 是否认证
    /// </summary>
    public int IsVerified { get; set; }

    /// <summary>
    /// 认证状态显示
    /// </summary>
    public string IsVerifiedDisplay => IsVerified switch
    {
        1 => "已认证",
        _ => "未认证"
    };

    /// <summary>
    /// 是否虚拟设备
    /// </summary>
    public int IsVirtual { get; set; }

    /// <summary>
    /// 虚拟设备显示
    /// </summary>
    public string IsVirtualDisplay => IsVirtual switch
    {
        1 => "是",
        _ => "否"
    };

    /// <summary>
    /// 数据格式
    /// </summary>
    public int DataFmt { get; set; }

    /// <summary>
    /// 数据格式显示
    /// </summary>
    public string DataFmtDisplay => DataFmt switch
    {
        1 => "自定义",
        2 => "透传",
        3 => "JSON",
        _ => "未知"
    };

    /// <summary>
    /// 认证模式
    /// </summary>
    public int AuthMode { get; set; }

    /// <summary>
    /// 认证模式显示
    /// </summary>
    public string AuthModeDisplay => AuthMode switch
    {
        1 => "一机一密",
        2 => "一型一密",
        _ => "未知"
    };

    /// <summary>
    /// 设备状态 (0: 离线, 1: 在线)
    /// </summary>
    public int DeviceStatus { get; set; }

    /// <summary>
    /// 设备状态显示
    /// </summary>
    public string DeviceStatusDisplay => DeviceStatus switch
    {
        1 => "在线",
        _ => "离线"
    };

    /// <summary>
    /// 设备状态徽章样式
    /// </summary>
    public string DeviceStatusBadge => DeviceStatus switch
    {
        1 => "success",
        _ => "secondary"
    };

    /// <summary>
    /// 首次连接时间
    /// </summary>
    public long? FirstConnTime { get; set; }

    /// <summary>
    /// 首次连接时间显示
    /// </summary>
    public string? FirstConnTimeDisplay => FirstConnTime.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(FirstConnTime.Value).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
        : null;

    /// <summary>
    /// 最后连接时间
    /// </summary>
    public long? LastConnTime { get; set; }

    /// <summary>
    /// 最后连接时间显示
    /// </summary>
    public string? LastConnTimeDisplay => LastConnTime.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(LastConnTime.Value).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
        : null;

    /// <summary>
    /// 最后离线时间
    /// </summary>
    public long? LastOfflineTime { get; set; }

    /// <summary>
    /// 最后离线时间显示
    /// </summary>
    public string? LastOfflineTimeDisplay => LastOfflineTime.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(LastOfflineTime.Value).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
        : null;

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreateTime { get; set; }

    /// <summary>
    /// 创建时间显示
    /// </summary>
    public string CreateTimeDisplay =>
        DateTimeOffset.FromUnixTimeMilliseconds(CreateTime).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新时间显示
    /// </summary>
    public string? UpdateTimeDisplay => UpdateTime.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(UpdateTime.Value).LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")
        : null;

    /// <summary>
    /// 时间偏移
    /// </summary>
    public string? TimeOffset { get; set; }

    /// <summary>
    /// 时区ID
    /// </summary>
    public string? TimeZoneId { get; set; }

    /// <summary>
    /// 从 DTO 转换为 ViewModel
    /// </summary>
    public static DeviceDetailViewModel FromDto(DeviceDetailDto dto)
    {
        return new DeviceDetailViewModel
        {
            ProductKey = dto.ProductKey,
            DeviceKey = dto.DeviceKey,
            DeviceName = dto.DeviceName,
            Sn = dto.Sn,
            DeviceQrCode = dto.DeviceQrCode,
            IsActived = dto.IsActived,
            ActivedTime = dto.ActivedTime,
            IsVerified = dto.IsVerified,
            IsVirtual = dto.IsVirtual,
            DataFmt = dto.DataFmt,
            AuthMode = dto.AuthMode,
            DeviceStatus = dto.DeviceStatus,
            FirstConnTime = dto.FirstConnTime,
            LastConnTime = dto.LastConnTime,
            LastOfflineTime = dto.LastOfflineTime,
            CreateTime = dto.CreateTime,
            UpdateTime = dto.UpdateTime,
            TimeOffset = dto.TimeOffset,
            TimeZoneId = dto.TimeZoneId
        };
    }
}
