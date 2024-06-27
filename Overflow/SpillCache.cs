using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Overflow.SouthernWater;
using Quartz;

namespace Overflow;

public class SpillCache(SouthernWaterSpillCache southernWaterSpillCache, ILogger<SpillCache> logger)
{
    public SouthernWaterApiJob CurrentSouthernWaterApiJob => southernWaterSpillCache.CurrentJob;

    public void Refresh()
    {
        logger.LogDebug("Refreshing caches");

        southernWaterSpillCache.ReloadJob();
    }
}

public class LoadCacheOnStart(ISchedulerFactory scheduler, SpillCache cache, ILogger<LoadCacheOnStart> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Loading caches for startup");

        await (await scheduler.GetScheduler()).TriggerJob(new JobKey("cache-refresh", "cache"));
        // cache.Refresh();

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}