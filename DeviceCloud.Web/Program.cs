using DeviceCloud.Application.Interfaces;
using DeviceCloud.Application.Interfaces.Devices;
using DeviceCloud.Application.Interfaces.Products;
using DeviceCloud.Application.Interfaces.Subscriptions;
using DeviceCloud.Application.Services;
using DeviceCloud.Application.Services.Devices;
using DeviceCloud.Application.Services.Products;
using DeviceCloud.Application.Services.Subscriptions;
using DeviceCloud.Infrastructure.Extensions;
using DeviceCloud.Infrastructure.Http;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 添加 Razor Pages
builder.Services.AddRazorPages();

// 添加 Infrastructure 层服务
builder.Services.AddInfrastructure(builder.Configuration);

// 添加 Application 层服务
builder.Services.AddScoped<IQuectelAppService, QuectelAppService>();
builder.Services.AddScoped<IProductAppService, ProductAppService>();
builder.Services.AddScoped<IDeviceAppService, DeviceAppService>();
builder.Services.AddScoped<ISubscriptionAppService, SubscriptionAppService>();
builder.Services.AddScoped<IProtocolParsingService, ProtocolParsingService>();
builder.Services.AddScoped<ICommandEncodingService, CommandEncodingService>();
builder.Services.AddScoped<IDeviceDetailQueryService, DeviceDetailQueryService>();
builder.Services.AddScoped<IDeviceCommandService, DeviceCommandService>();

var app = builder.Build();

// 开发环境配置
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

// 测试接口（Minimal API）
app.MapGet("/api/test", () =>
{
    return Results.Ok(new
    {
        code = 200,
        message = "OK"
    });
});

app.Run();