using Microsoft.Extensions.Logging;
using Quartz;

namespace Overflow;

[DisallowConcurrentExecution]
public class CacheReloadJob(SpillCache cache, ILogger<CacheReloadJob> logger): IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        logger.LogDebug("Refreshing caches");
        cache.Refresh();
        return Task.CompletedTask;
    }
}