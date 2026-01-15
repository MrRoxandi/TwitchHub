using Lua;
using Lua.Standard;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Serilog;
using SharpHook;
using System.Text.Json;
using System.Text.Json.Serialization;
using TwitchHub.Components;
using TwitchHub.Configurations;
using TwitchHub.Lua.LuaLibs;
using TwitchHub.Services;
using TwitchHub.Services.Backends;
using TwitchHub.Services.Backends.Data;
using TwitchHub.Services.LuaMedia;
using TwitchHub.Services.Twitch;
using TwitchHub.Services.Twitch.Data;
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

builder.Services.Configure<TwitchConfiguration>(builder.Configuration.GetSection(TwitchConfiguration.SectionName));
builder.Services.Configure<LuaMediaServiceConfiguration>(builder.Configuration.GetSection(LuaMediaServiceConfiguration.SectionName));

// -------------- TWITCH --------------

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(
        Path.Combine(builder.Environment.ContentRootPath, "keys")
        )
    )
    .SetApplicationName("TwitchHub");

builder.Services.AddSingleton<FileTwitchTokenStorage>();

builder.Services.AddSingleton<TwitchAPI>(sp =>
{
    var lf = sp.GetRequiredService<ILoggerFactory>();
    var conf = sp.GetRequiredService<IOptions<TwitchConfiguration>>()!.Value;
    return new TwitchAPI(lf)
    {
        Settings = { ClientId = conf.ClientId, Secret = conf.ClientSecret }
    };
});

builder.Services.AddSingleton<TwitchTokenProvider>();

builder.Services.AddSingleton<TwitchClient>(sp =>
{
    var lf = sp.GetRequiredService<ILoggerFactory>();
    return new TwitchClient(loggerFactory: lf);
});

builder.Services.AddHostedService<TwitchChatClient>();

builder.Services.AddTwitchLibEventSubWebsockets();
builder.Services.AddHostedService<TwitchEventSub>();
builder.Services.AddSingleton<TwitchConfigurator>();

// -------------- LUA SERVICES --------------

builder.Services.AddDbContextFactory<PointsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString(PointsDbContext.ConnectionString)));

builder.Services.AddSingleton<LuaDataContainer>();
builder.Services.AddSingleton<LuaMediaService>();
builder.Services.AddSingleton<LuaPointsService>();
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

// -------------- LUA LIBS --------------

builder.Services.AddSingleton<LuaHardwareLib>();
builder.Services.AddSingleton<LuaTwitchLib>();
builder.Services.AddSingleton<LuaStorageLib>();
builder.Services.AddSingleton<LuaUtilsLib>();
builder.Services.AddSingleton<LuaPointsLib>();
builder.Services.AddSingleton<LuaMediaLib>();

// Configure Kestrel from appsettings.json 
builder.WebHost.ConfigureKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")));

var app = builder.Build();
try
{
    var factory = app.Services.GetRequiredService<IDbContextFactory<PointsDbContext>>();
    using var context = await factory.CreateDbContextAsync();
    await context.Database.MigrateAsync();
    app.Logger.LogInformation("Database migration/check completed successfully.");
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    return;
}
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
state.Environment["MediaLuaLib"] = app.Services.GetRequiredService<LuaMediaLib>();
state.OpenStandardLibraries();

var lms = app.Services.GetRequiredService<LuaMediaService>();
var channels = string.Join(", ", lms.Channels);
app.Logger.LogInformation("All channesl: {channels}", channels);
var res = await TestScript(state);

await Task.Delay(TimeSpan.FromSeconds(30));

//var r = Task.Delay(TimeSpan.FromSeconds(20))
//    .ContinueWith(async _ => await TestScript(state));
//app.Run();

return;

static async Task<LuaValue[]> TestScript(LuaState state) => await state.DoStringAsync(
    @"
        local media = MediaLuaLib
        local utils = UtilsLuaLib
        local channel = 'Main'
        media:Add(channel, 'C:/Users/lyamcev/Downloads/Lady_Gaga_-_Judas_79457310.mp3') 
        media:SetVolume(channel, 30)
        utils:Delay(4 * 1000)
        media:Add('stream', 'C:/Users/lyamcev/Downloads/Britney Manson - FASHION (Audio) [7af0d1a10b8d35e2453784bc215ab6ea].mp4')
    ");