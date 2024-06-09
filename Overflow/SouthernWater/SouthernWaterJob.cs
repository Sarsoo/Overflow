using Quartz;

namespace Overflow.SouthernWater;

public class SouthernWaterJob(SouthernWaterApiJobRunner jobRunner) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await jobRunner.LoadSpills();
    }
}