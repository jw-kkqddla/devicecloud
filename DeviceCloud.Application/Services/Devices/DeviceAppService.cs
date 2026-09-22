using System.Text.Json;
using DeviceCloud.Application.Common;
using DeviceCloud.Application.DTOs.Devices;
using DeviceCloud.Application.Interfaces.Devices;
using DeviceCloud.Domain.Services;
using DeviceCloud.Domain.Services.Devices;

namespace DeviceCloud.Application.Services.Devices;

/// <summary>
/// 设备应用服务实现
/// </summary>
public class DeviceAppService : IDeviceAppService
{
    private readonly IDeviceService _deviceService;
    private readonly ICacheService _cacheService;

    public DeviceAppService(IDeviceService deviceService, ICacheService cacheService)
    {
        _deviceService = deviceService;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<List<DeviceListDto>>> GetDevicesAsync(string productKey, string? deviceKey = null, string? deviceName = null, int pageNum = 1, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ApiResponse.Fail<List<DeviceListDto>>("产品key不能为空", "INVALID_PARAM");
        }

        var cacheKey = $"dc:cache:devices:{productKey}:{deviceKey ?? ""}:{deviceName ?? ""}:{pageNum}:{pageSize}";

        var cached = await _cacheService.GetAsync<ApiResponse<List<DeviceListDto>>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        try
        {
            var jsonContent = await _deviceService.GetDevicesAsync(productKey, deviceKey, deviceName, pageNum, pageSize);

            var response = JsonSerializer.Deserialize<DeviceListResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ApiResponse<List<DeviceListDto>> result;
            if (response == null)
            {
                result = ApiResponse.Fail<List<DeviceListDto>>("API响应解析失败", "PARSE_ERROR");
            }
            else if (response.Code != 200)
            {
                result = ApiResponse.Fail<List<DeviceListDto>>(response.Msg ?? "获取设备列表成功", "API_ERROR");
            }
            else
            {
                result = ApiResponse.Ok(response.Data ?? new List<DeviceListDto>(), "获取设备列表失败");
            }

            if (result.Success)
            {
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromSeconds(30));
            }

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<List<DeviceListDto>>(ex.Message, "API_ERROR");
        }
    }

    public async Task<ApiResponse<DeviceDetailDto>> GetDeviceDetailAsync(string productKey, string deviceKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ApiResponse.Fail<DeviceDetailDto>("产品Key不能为空", "INVALID_PARAM");
        }

        if (string.IsNullOrWhiteSpace(deviceKey))
        {
            return ApiResponse.Fail<DeviceDetailDto>("设备Key不能为空", "INVALID_PARAM");
        }

        var cacheKey = $"dc:cache:device:detail:{productKey}:{deviceKey}";

        var cached = await _cacheService.GetAsync<ApiResponse<DeviceDetailDto>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        try
        {
            var jsonContent = await _deviceService.GetDeviceDetailAsync(productKey, deviceKey);

            var response = JsonSerializer.Deserialize<DeviceDetailResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            ApiResponse<DeviceDetailDto> result;
            if (response == null)
            {
                result = ApiResponse.Fail<DeviceDetailDto>("API响应解析失败", "PARSE_ERROR");
            }
            else if (response.Code != 200)
            {
                result = ApiResponse.Fail<DeviceDetailDto>(response.Msg ?? "获取设备详情失败", "API_ERROR");
            }
            else
            {
                result = ApiResponse.Ok(response.Data!, "获取设备详情成功");
            }

            if (result.Success)
            {
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromSeconds(30));
            }

            return result;
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<DeviceDetailDto>(ex.Message, "API_ERROR");
        }
    }

    public async Task<ApiResponse<DeviceCreateData>> CreateDeviceAsync(string productKey, string deviceKey, string? deviceName = null, string? sn = null, int? authMode = null, string? psk = null, string? fingerPrint = null)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ApiResponse.Fail<DeviceCreateData>("产品Key不能为空", "INVALID_PARAM");
        }

        if (string.IsNullOrWhiteSpace(deviceKey))
        {
            return ApiResponse.Fail<DeviceCreateData>("设备Key不能为空", "INVALID_PARAM");
        }

        try
        {
            var jsonContent = await _deviceService.CreateDeviceAsync(productKey, deviceKey, deviceName, sn, authMode, psk, fingerPrint);

            var response = JsonSerializer.Deserialize<DeviceCreateResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response == null)
            {
                return ApiResponse.Fail<DeviceCreateData>("API响应解析失败", "PARSE_ERROR");
            }

            if (response.Code != 200)
            {
                return ApiResponse.Fail<DeviceCreateData>(response.Msg ?? "创建设备失败", "API_ERROR");
            }

            await _cacheService.RemoveByPrefixAsync($"dc:cache:devices:{productKey}:");

            return ApiResponse.Ok(response.Data ?? new DeviceCreateData(), "创建设备成功");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<DeviceCreateData>(ex.Message, "API_ERROR");
        }
    }

    public async Task<ApiResponse<object>> DeleteDeviceAsync(string productKey, string deviceKey)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ApiResponse.Fail<object>("产品Key不能为空", "INVALID_PARAM");
        }

        if (string.IsNullOrWhiteSpace(deviceKey))
        {
            return ApiResponse.Fail<object>("设备Key不能为空", "INVALID_PARAM");
        }

        try
        {
            var jsonContent = await _deviceService.DeleteDeviceAsync(productKey, deviceKey);

            var response = JsonSerializer.Deserialize<DeviceDeleteResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response == null)
            {
                return ApiResponse.Fail<object>("API 响应解析失败", "PARSE_ERROR");
            }

            if (response.Code != 200)
            {
                return ApiResponse.Fail<object>(response.Msg ?? "删除设备失败", "API_ERROR");
            }

            await _cacheService.RemoveByPrefixAsync($"dc:cache:devices:{productKey}:");

            return ApiResponse.Ok<object>(new { }, "删除设备成功");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<object>(ex.Message, "API_ERROR");
        }
    }

    public async Task<ApiResponse<object>> UpdateDeviceAsync(string productKey, string deviceKey, string? deviceName = null, string? sn = null)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ApiResponse.Fail<object>("产品Key不能为空", "INVALID_PARAM");
        }

        if (string.IsNullOrWhiteSpace(deviceKey))
        {
            return ApiResponse.Fail<object>("设备Key不能为空", "INVALID_PARAM");
        }

        try
        {
            var jsonContent = await _deviceService.UpdateDeviceAsync(productKey, deviceKey, deviceName, sn);

            var response = JsonSerializer.Deserialize<DeviceUpdateResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response == null)
            {
                return ApiResponse.Fail<object>("API 响应解析失败", "PARSE_ERROR");
            }

            if (response.Code != 200)
            {
                return ApiResponse.Fail<object>(response.Msg ?? "更新设备失败", "API_ERROR");
            }

            await _cacheService.RemoveByPrefixAsync($"dc:cache:devices:{productKey}:");

            return ApiResponse.Ok<object>(new { }, "更新设备成功");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<object>(ex.Message, "API_ERROR");
        }
    }

    public async Task<ApiResponse<List<DeviceDataItem>>> ReadDeviceDataAsync(string productKey, List<string> devices, string data)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ApiResponse.Fail<List<DeviceDataItem>>("产品Key不能为空", "INVALID_PARAM");
        }

        if (devices == null || devices.Count == 0)
        {
            return ApiResponse.Fail<List<DeviceDataItem>>("设备列表不能为空", "INVALID_PARAM");
        }

        if (string.IsNullOrWhiteSpace(data))
        {
            return ApiResponse.Fail<List<DeviceDataItem>>("物模型数据标识不能为空", "INVALID_PARAM");
        }

        try
        {
            var jsonContent = await _deviceService.ReadDeviceDataAsync(productKey, devices, data);

            var response = JsonSerializer.Deserialize<ReadDeviceDataResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (response == null)
            {
                return ApiResponse.Fail<List<DeviceDataItem>>("API 响应解析失败", "PARSE_ERROR");
            }

            if (response.Code != 200)
            {
                return ApiResponse.Fail<List<DeviceDataItem>>(response.Msg ?? "读取设备数据失败", "API_ERROR");
            }

            return ApiResponse.Ok(response.Data ?? new List<DeviceDataItem>(), "读取设备数据成功");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<List<DeviceDataItem>>(ex.Message, "API_ERROR");
        }
    }
}
