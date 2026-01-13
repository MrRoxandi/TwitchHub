using Lua;
using Lua.Standard;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Serilog;
using SharpHook;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using TwitchHub.Components;
using TwitchHub.Configurations;
using TwitchHub.Lua.LuaLibs;
using TwitchHub.Services.Backends;
using TwitchHub.Services.Twitch;
using TwitchLib.Api;
using TwitchLib.Client;
using TwitchLib.EventSub.Websockets.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTwitchLibEventSubWebsockets();

builder.Services.Configure<TwitchConfiguration>(builder.Configuration.GetSection(TwitchConfiguration.SectionName));
builder.Services.Configure<LuaMediaServiceConfiguration>(builder.Configuration.GetSection(LuaMediaServiceConfiguration.SectionName));

builder.Services.AddSingleton<TwitchClient>(sp =>
{
    var lf = sp.GetRequiredService<ILoggerFactory>();
    return new TwitchClient(loggerFactory: lf);
});

builder.Services.AddSingleton<TwitchAPI>(sp =>
{
    var lf = sp.GetRequiredService<ILoggerFactory>();
    var conf = sp.GetRequiredService<IOptions<TwitchConfiguration>>()!.Value;
    var ta = new TwitchAPI(lf)
    {
        Settings =
        {
            ClientId = conf.ClientId,
            Secret = conf.ClientSecret
        }
    };
    return ta;
});

builder.Services.AddSingleton<TwitchConfigurator>();
builder.Services.AddSingleton<LuaDataContainer>();
builder.Services.AddHostedService<ChatClient>();
builder.Services.AddSingleton<LuaMediaService>();

builder.Services.AddSingleton<LuaHardwareLib>();
builder.Services.AddSingleton<LuaTwitchLib>();
builder.Services.AddSingleton<LuaStorageLib>();
builder.Services.AddSingleton<LuaUtilsLib>();

builder.Services.AddSingleton<JsonSerializerOptions>(sp =>
{
    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    options.Converters.Add(new LuaValueJsonConverter());
    options.Converters.Add(new JsonStringEnumConverter());
    return options;
});

// Configure Kestrel from appsettings.json 
builder.WebHost.ConfigureKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    _ = app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var state = LuaState.Create();
state.Environment["TwitchLuaLib"] = app.Services.GetRequiredService<LuaTwitchLib>();
state.Environment["StorageLuaLib"] = app.Services.GetRequiredService<LuaStorageLib>();
state.Environment["HardwareLuaLib"] = app.Services.GetRequiredService<LuaHardwareLib>();
state.Environment["UtilsLuaLib"] = app.Services.GetRequiredService<LuaUtilsLib>();

state.OpenStandardLibraries();

var lms = app.Services.GetRequiredService<LuaMediaService>();
lms.Add(@"https://eu.hitmo-top.com/get/music/20250816/Lady_Gaga_-_Judas_79457310.mp3");
lms.Add(@"https://river-1.rutube.ru/hls-vod/LgkwVS2R6yca-bq5Mxu87g/1768896826/3326/0x5000c500c7c4390b/8b9221c66b10453e876d32b1f6c05553.mp4.m3u8?i=1280x720_3022");
await Task.Delay(TimeSpan.FromSeconds(10));
lms.Skip();
await Task.Delay(TimeSpan.FromSeconds(40));
//var res = await TestScript(state);

//var r = Task.Delay(TimeSpan.FromSeconds(20))
//    .ContinueWith(async _ => await TestScript(state));
//app.Run();

return;

static async Task<LuaValue[]> TestScript(LuaState state) => await state.DoStringAsync(
    @"
        local storage = StorageLuaLib
        local utils = UtilsLuaLib
        local table = storage:Get('cringe')
        local res = utils:TableToJson(table)
        print('Test from lua: ' .. res)
    ");