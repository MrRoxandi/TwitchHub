using Microsoft.Extensions.Options;
using MudBlazor.Services;
using Serilog;
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

builder.Services.Configure<TwitchConfig>(builder.Configuration.GetSection(TwitchConfig.SectionName));

builder.Services.AddSingleton<InputDispatcher>();
builder.Services.AddSingleton<TwitchClient>(sp =>
{
    var lf = sp.GetRequiredService<ILoggerFactory>();
    return new TwitchClient(loggerFactory: lf);
});

builder.Services.AddSingleton<TwitchAPI>(sp =>
{
    var lf = sp.GetRequiredService<ILoggerFactory>();
    var conf = sp.GetRequiredService<IOptions<TwitchConfig>>()!.Value;
    var ta = new TwitchAPI(loggerFactory: lf);
    ta.Settings.ClientId = conf.ClientId;
    ta.Settings.Secret = conf.ClientSecret;
    //ta.Settings.AccessToken = tokenprovider.GetAccessToken(); 
    return ta;
});

builder.Services.AddSingleton<TwitchConfigurator>();
builder.Services.AddHostedService<ChatClient>();
builder.Services.AddSingleton<LuaHadwareLib>();
builder.Services.AddSingleton<LuaTwitchLib>();

// Configure Kestrel from appsettings.json 
builder.WebHost.ConfigureKestrel((context, options) => options.Configure(context.Configuration.GetSection("Kestrel")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    _ = app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();