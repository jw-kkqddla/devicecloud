using DeviceCloud.Domain.Services.Devices;
using DeviceCloud.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DeviceCloud.Infrastructure.Services.Devices;

    /// <summary>
    /// 设备 API 服务实现
    /// </summary>
    public class DeviceApiService : IDeviceService
    {
        private readonly HttpClient _httpClient;
        private readonly QuectelOptions _options;
        private readonly ILogger<DeviceApiService> _logger;

        public DeviceApiService(
            HttpClient httpClient,
            IOptions<QuectelOptions> options,
            ILogger<DeviceApiService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> GetDevicesAsync(string productKey, string? deviceKey = null, string? deviceName = null, int pageNum = 1, int pageSize = 20)
        {
            var url = $"{_options.BaseUrl}/v2/devicemgr/r3/openapi/product/device/overview?productKey={Uri.EscapeDataString(productKey)}&pageNum={pageNum}&pageSize={pageSize}";

            if (!string.IsNullOrEmpty(deviceKey))
            {
                url += $"&deviceKey={Uri.EscapeDataString(deviceKey)}";
            }

            if (!string.IsNullOrEmpty(deviceName))
            {
                url += $"&deviceName={Uri.EscapeDataString(deviceName)}";
            }

            _logger.LogInformation("Getting devices for product: {ProductKey}, deviceKey: {DeviceKey}, deviceName: {DeviceName}, pageNum: {PageNum}, pageSize: {PageSize}", productKey, deviceKey, deviceName, pageNum, pageSize);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API Error: {Content}", content);
                throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
            }

            return content;
        }

        public async Task<string> GetDeviceDetailAsync(string productKey, string deviceKey)
        {
            var url = $"{_options.BaseUrl}/v2/devicemgr/r3/openapi/device/detail?productKey={Uri.EscapeDataString(productKey)}&deviceKey={Uri.EscapeDataString(deviceKey)}";

            _logger.LogInformation("Getting device detail for product: {ProductKey}, deviceKey: {DeviceKey}", productKey, deviceKey);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API Error: {Content}", content);
                throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
            }

            return content;
        }

        public async Task<string> CreateDeviceAsync(string productKey, string deviceKey, string? deviceName = null, string? sn = null, int? authMode = null, string? psk = null, string? fingerPrint = null)
        {
            var url = $"{_options.BaseUrl}/v2/devicemgr/r3/openapi/device/create";

            _logger.LogInformation("Creating device: productKey={ProductKey}, deviceKey={DeviceKey}", productKey, deviceKey);

            var requestBody = new
            {
                productKey,
                deviceKey,
                deviceName = deviceName ?? string.Empty,
                sn = sn ?? string.Empty,
                authMode,
                psk = psk ?? string.Empty,
                fingerPrint = fingerPrint ?? string.Empty
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API Error: {Content}", content);
                throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
            }

            return content;
        }

        public async Task<string> DeleteDeviceAsync(string productKey, string deviceKey)
        {
            var url = $"{_options.BaseUrl}/v2/devicemgr/r3/openapi/device/delete";

            _logger.LogInformation("Deleting device: productKey={ProductKey}, deviceKey={DeviceKey}", productKey, deviceKey);

            var requestBody = new
            {
                productKey,
                deviceKey
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API Error: {Content}", content);
                throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
            }

            return content;
        }

        public async Task<string> UpdateDeviceAsync(string productKey, string deviceKey, string? deviceName = null, string? sn = null)
        {
            var url = $"{_options.BaseUrl}/v2/devicemgr/r3/openapi/device/update";

            _logger.LogInformation("Updating device: productKey={ProductKey}, deviceKey={DeviceKey}", productKey, deviceKey);

            var requestBody = new
            {
                productKey,
                deviceKey,
                deviceName = deviceName ?? string.Empty,
                sn = sn ?? string.Empty
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API Error: {Content}", content);
                throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
            }

            return content;
        }

        public async Task<string> ReadDeviceDataAsync(string productKey, List<string> devices, string data)
        {
            var url = $"{_options.BaseUrl}/v2/deviceshadow/r3/openapi/dm/readData";

            _logger.LogInformation("Reading device data: productKey={ProductKey}, devices={Devices}, data={Data}",
                productKey, string.Join(",", devices), data);

            // data 参数已经是 JSON 字符串，需要作为字符串值嵌入
            // API 要求: "data": "[\"load_a_voltage\"]" (字符串，不是数组)
            var devicesJson = JsonSerializer.Serialize(devices);
            var dataJson = JsonSerializer.Serialize(data); // 再次序列化为字符串值
            var jsonContent = $"{{\"productKey\":\"{productKey}\",\"devices\":{devicesJson},\"data\":{dataJson}}}";

            _logger.LogDebug("Request body: {RequestBody}", jsonContent);

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Accept", "application/json");

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("API Error: {Content}", content);
                throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
            }

            return content;
        }

        public async Task<string> GetDeviceHistoryDataAsync(string productKey, string deviceKey, long startTime, long endTime, List<string>? identifiers = null, int? limit = null, int? offset = null)
        {
        var url = $"{_options.BaseUrl}/v2/quecdatastorage/r2/openapi/device/data/history";
            var payload = new Dictionary<string, object?>
            {
                ["productKey"] = productKey,
                ["deviceKey"] = deviceKey,
                ["startTime"] = startTime,
                ["endTime"] = endTime,
                ["identifierList"] = identifiers,
                ["limit"] = limit,
                ["offset"] = offset
            };

            // remove nulls
            var filtered = payload.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value);
            var content = new StringContent(JsonSerializer.Serialize(filtered), System.Text.Encoding.UTF8, "application/json");

            var res = await _httpClient.PostAsync(url, content);
            var txt = await res.Content.ReadAsStringAsync();
            return txt;
        }

        /// <summary>
        /// 读取设备单个物模型属性的历史数据
        /// </summary>
        public async Task<string> GetDevicePropertyHistoryAsync(string productKey, string deviceKey, string code, long startTime, long endTime, int? pageSize = null)
        {
            var url = $"{_options.BaseUrl}/v2/quecdatastorage/r2/openapi/device/property/history";

            var payload = new Dictionary<string, object?>
            {
                ["productKey"] = productKey,
                ["deviceKey"] = deviceKey,
                ["code"] = code,
                ["startTime"] = startTime,
                ["endTime"] = endTime,
                ["pageSize"] = pageSize
            };

            var filtered = payload.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value);
            var content = new StringContent(JsonSerializer.Serialize(filtered), System.Text.Encoding.UTF8, "application/json");

            var res = await _httpClient.PostAsync(url, content);
            var txt = await res.Content.ReadAsStringAsync();
            return txt;
        }

    public async Task<string> SendDeviceDataAsync(string productKey,string deviceKey,string data,string encode = "Text",bool isCache = false,bool isCover = false,int? qos = null,int? cacheTime = null)
    {
        var url = $"{_options.BaseUrl}/v2/deviceshadow/r3/openapi/raw/sendData";

        _logger.LogInformation("Sending data to device: productKey={ProductKey}, deviceKey={DeviceKey}",productKey, deviceKey);

        var requestBody = new
        {
            cacheTime = cacheTime,
            data = data,
            devices = new[]
            {
            new { deviceKey = deviceKey, productKey = productKey }
        },
            encode = encode,
            isCache = isCache,
            isCover = isCover,
            qos = qos
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        _logger.LogDebug("Request body: {RequestBody}", jsonContent);

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API Error: {Content}", content);
            throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
        }

        return content;
    }

    /// <summary>
    /// 写物模型数据（下发设置指令）。POST /v2/deviceshadow/r3/openapi/dm/writeData
    /// </summary>
    /// <param name="data">JSON 字符串，形如 [{"HEEP1":"POP01"}]</param>
    public async Task<string> WriteDeviceDataAsync(string productKey, List<string> deviceKeys, string data, bool isCache = false, bool isCover = false, int? qos = null, int? cacheTime = null)
    {
        var url = $"{_options.BaseUrl}/v2/deviceshadow/r3/openapi/dm/writeData";

        _logger.LogInformation("Writing device data: productKey={ProductKey}, devices={Devices}, data={Data}",
            productKey, string.Join(",", deviceKeys), data);

        var requestBody = new
        {
            cacheTime = cacheTime,
            data = data,
            devices = deviceKeys,
            extendPara = new { },
            isCache = isCache,
            isCover = isCover,
            productKey = productKey,
            qos = qos
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        _logger.LogDebug("Request body: {RequestBody}", jsonContent);

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Accept", "application/json");

        var response = await _httpClient.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        _logger.LogDebug("API Response: {StatusCode}", response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("API Error: {Content}", content);
            throw new HttpRequestException($"API 请求失败: {response.StatusCode} - {content}");
        }

        return content;
    }

    /// <summary>
    /// 获取当前时间的毫秒级时间戳 long
    /// </summary>
    public static long NowTimestampLong()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
