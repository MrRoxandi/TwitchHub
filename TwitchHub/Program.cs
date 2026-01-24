using System.Runtime.Versioning;
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
using TwitchHub.Lua;
using TwitchHub.Lua.LuaLibs;
using TwitchHub.Lua.Services;
using TwitchHub.Services.Backends;
using TwitchHub.Services.Backends.Data;
using TwitchHub.Services.Hardware;
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

var dataPath = Path.Combine(builder.Environment.ContentRootPath, "data");
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "keys");

if (!Directory.Exists(dataPath))
{
    _ = Directory.CreateDirectory(dataPath);
}

if (!Directory.Exists(keysPath))
{
    _ = Directory.CreateDirectory(keysPath);
}

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContextFactory<PointsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString(PointsDbContext.ConnectionString)));
builder.Services.AddDbContextFactory<TwitchClipsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString(TwitchClipsDbContext.ConnectionString)));

builder.Services.Configure<TwitchConfiguration>
    (builder.Configuration.GetSection(TwitchConfiguration.SectionName));
builder.Services.Configure<LuaMediaServiceConfiguration>
    (builder.Configuration.GetSection(LuaMediaServiceConfiguration.SectionName));
builder.Services.Configure<LuaStorageContainerConfiguration>
    (builder.Configuration.GetSection(LuaStorageContainerConfiguration.SectionName));
builder.Services.Configure<TextToSpeechEngineConfiguration>
    (builder.Configuration.GetSection(TextToSpeechEngineConfiguration.SectionName));

// ================= TWITCH =================

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

builder.Services.AddTwitchLibEventSubWebsockets();
builder.Services.AddSingleton<TwitchConfigurator>();
builder.Services.AddHostedService<TwitchChatClient>();
builder.Services.AddHostedService<TwitchEventSub>();
builder.Services.AddHostedService<TwitchClipPoller>();

// ================= LUA SERVICES =================

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
builder.Services.AddSingleton<LuaBlockedKeys>();
builder.Services.AddSingleton<LuaReactionsService>();
builder.Services.AddSingleton<LuaScriptsSerivce>();
builder.Services.AddHostedService<LuaHardwareService>();
builder.Services.AddHostedService<LuaSharedManager>();
builder.Services.AddSingleton<TextToSpeechEngine>();

// ================= LUA LIBS =================

builder.Services.AddSingleton<LuaHardwareLib>();
builder.Services.AddSingleton<LuaTwitchLib>();
builder.Services.AddSingleton<LuaUtilsLib>();
builder.Services.AddSingleton<LuaPointsLib>();
builder.Services.AddSingleton<LuaScriptLib>();
builder.Services.AddSingleton<LuaStorageLib>();
builder.Services.AddSingleton<LuaMediaLib>();
builder.Services.AddSingleton<LuaLoggerLib>();
builder.Services.AddSingleton<LuaSpeechLib>();

// Configure Kestrel from appsettings.json
builder.WebHost.ConfigureKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")));

var app = builder.Build();
try
{
    var pointsFactory = app.Services.GetRequiredService<IDbContextFactory<PointsDbContext>>();
    var clipsFactory = app.Services.GetRequiredService<IDbContextFactory<TwitchClipsDbContext>>();
    await using var pointsContext = await pointsFactory.CreateDbContextAsync();
    await using var clipsContext = await clipsFactory.CreateDbContextAsync();
    await pointsContext.Database.MigrateAsync();
    await clipsContext.Database.MigrateAsync();
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

app.Run();
