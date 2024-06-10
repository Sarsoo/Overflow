using Microsoft.Extensions.Logging;
using Quartz;

namespace Overflow.SouthernWater;

public class SouthernWaterJob(ILogger<SouthernWaterJob> logger, SouthernWaterApiJobRunner jobRunner) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await jobRunner.LoadSpills();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while running Southern Water API Job");
        }
    }
}