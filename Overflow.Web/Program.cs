using Overflow.Web.Components;
using Overflow;


using MongoDB.Driver;
using Overflow.SouthernWater;
using Quartz;
using Quartz.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var driver = new MongoClient(builder.Configuration.GetConnectionString("Default"));
builder.Services.AddSingleton(driver);
builder.Services.AddScoped<IMongoDatabase>(s => s.GetRequiredService<MongoClient>().GetDatabase("overflow"));

builder.Services.AddControllers();

// base configuration from appsettings.json
builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));

// if you are using persistent job store, you might want to alter some options
builder.Services.Configure<QuartzOptions>(options =>
{
    options.Scheduling.IgnoreDuplicates = true; // default: false
    options.Scheduling.OverWriteExistingData = true; // default: true
});

builder.Services.AddQuartz(q =>
{
    // base Quartz scheduler, job and trigger configuration
});

// ASP.NET Core hosting
builder.Services.AddQuartzServer(options =>
{
    // when shutting down we want jobs to complete gracefully
    options.WaitForJobsToComplete = false;
});

builder.Services.AddHttpClient();
builder.Services.AddSingleton<SouthernWaterApi>();
builder.Services.AddScoped<SouthernWaterApiJobRunner, SouthernWaterApiJobRunnerPersisting>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();