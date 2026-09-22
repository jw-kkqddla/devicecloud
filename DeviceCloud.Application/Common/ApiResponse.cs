namespace DeviceCloud.Application.Common;

/// <summary>
/// 统一 API 响应类
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 错误代码
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 成功响应
    /// </summary>
    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse { Success = true, Message = message };
    }

    /// <summary>
    /// 成功响应（带数据）
    /// </summary>
    public static ApiResponse<T> Ok<T>(T data, string? message = null)
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    /// <summary>
    /// 失败响应
    /// </summary>
    public static ApiResponse Fail(string message, string? errorCode = null)
    {
        return new ApiResponse { Success = false, Message = message, ErrorCode = errorCode };
    }

    /// <summary>
    /// 失败响应（带数据）
    /// </summary>
    public static ApiResponse<T> Fail<T>(string message, string? errorCode = null, T? data = default)
    {
        return new ApiResponse<T> { Success = false, Message = message, ErrorCode = errorCode, Data = data };
    }
}

/// <summary>
/// 统一 API 响应类（带数据）
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }
}

/// <summary>
/// 分页响应数据
/// </summary>
public class PagedData<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious => Page > 1;
}
