using FruitSysWeb.Extensions;
using Serilog;
using Microsoft.AspNetCore.HttpOverrides;

// Configure Serilog BEFORE creating the builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/odetta-solar-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 10485760, // 10 MB
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Odetta Solar application");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();

    builder.Services.AddOdettaSolarServices(builder.Configuration);

    builder.Services.AddRazorComponents().AddInteractiveServerComponents()
        .AddCircuitOptions(options =>
        {
            options.DetailedErrors = true;
            options.DisconnectedCircuitMaxRetained = 100;
            options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
            options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
        });

    builder.Services.AddHttpClient();
    builder.Services.AddMemoryCache();

    var app = builder.Build();

    // Konfiguracija forwarded headers (za rad iza IIS reverse proxy-ja)
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseStaticFiles();

    app.UseRouting();

    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
    Log.Information("Odetta Solar application stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
