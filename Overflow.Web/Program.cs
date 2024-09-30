using Microsoft.AspNetCore.Components.Web;
using Overflow;

using MongoDB.Driver;
using NLog.Extensions.Logging;
using Overflow.SouthernWater;
using Overflow.Web.Client;
using Quartz;
using Quartz.AspNetCore;
using Radzen;
using App = Overflow.Web.Components.App;

RenderModeSettings.InteractiveRenderMode = RenderMode.InteractiveAuto;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents()
                .AddInteractiveWebAssemblyComponents();

builder.Logging.ClearProviders();
builder.Logging.AddNLog(builder.Configuration);

var driver = new MongoClient(builder.Configuration.GetConnectionString("Default"));
builder.Services.AddSingleton(driver)
                .AddScoped<IMongoDatabase>(s => s.GetRequiredService<MongoClient>().GetDatabase(Static.DatabaseName));

builder.Services.AddControllers();

// base configuration from appsettings.json
builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"))

// if you are using persistent job store, you might want to alter some options
                .Configure<QuartzOptions>(options =>
{
    options.Scheduling.IgnoreDuplicates = true; // default: false
    options.Scheduling.OverWriteExistingData = true; // default: true
});

builder.Services.AddQuartz(q =>
{
    // base Quartz scheduler, job and trigger configuration

    q.UseSimpleTypeLoader();
    q.UseInMemoryStore();
    q.UseDefaultThreadPool(tp =>
    {
        tp.MaxConcurrency = 5;
    });

    var swKey = new JobKey("southern-water-api", "southern-water");

    q.AddJob<SouthernWaterJob>(j => j
        .WithDescription("Pull spills data from Southern Water API")
        .WithIdentity(swKey)
        .UsingJobData("IsFull", false)
    );

    q.AddTrigger(t => t
        .WithIdentity("southern-water-api-trigger")
        .ForJob(swKey)
        .StartNow()
        .WithCronSchedule(builder.Configuration.GetSection("SouthernWater").GetValue<string>("Cron") ?? "0 0 8 * * ?")
        .WithDescription("Periodic trigger for Southern Water API pulling")
    );

    var cacheKey = new JobKey("cache-refresh", "cache");

    q.AddJob<CacheReloadJob>(j => j
        .WithDescription("Refresh caches")
        .WithIdentity(cacheKey)
        .UsingJobData("IsFull", false)
    );

    q.AddTrigger(t => t
        .WithIdentity("cache-refresh-trigger")
        .ForJob(cacheKey)
        .StartNow()
        .WithCronSchedule(builder.Configuration.GetSection("Cache").GetValue<string>("Cron") ?? "0 0 8 * * ?")
        .WithDescription("Periodic trigger for cache refreshing")
    );
});

// ASP.NET Core hosting
builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = false;
});

builder.Services.AddHttpClient()
                .AddSingleton<SouthernWaterApi>()
                .AddScoped<SouthernWaterApiJobRunner, SouthernWaterApiJobRunnerPersisting>()
                .AddTransient<SouthernWaterJob>();

builder.Services.AddSingleton<SpillCache>()
                .AddSingleton<SouthernWaterSpillCache>()
                .AddHostedService<LoadCacheOnStart>();

builder.Services.AddRadzenComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles()
    .UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddAdditionalAssemblies(typeof(Overflow.Web.Client.Components.SpillsCalendar).Assembly)
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();