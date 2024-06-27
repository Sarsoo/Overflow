using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Overflow.SouthernWater;

public class SouthernWaterSpillCache(IServiceProvider serviceProvider, ILogger<SouthernWaterSpillCache> logger)
{
    private readonly ILogger _logger = logger;

    private SouthernWaterApiJob _currentJob;

    public SouthernWaterApiJob CurrentJob => _currentJob;

    public void ReloadJob()
    {
        _logger.LogInformation("Refreshing Southern Water Spills");

        using var scope = serviceProvider.CreateScope();

        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        var job = database.GetCollection<SouthernWaterApiJob>(Static.JobCollectionName)
            .AsQueryable()
            .OrderByDescending(j => j.EndTime)
            .FirstOrDefault();

        if (job is not null)
        {
            job.Spills = database.GetCollection<Spill>(Static.SpillCollectionName)
                .AsQueryable()
                .Where(s => s.JobId == job._id)
                .ToList();

            _currentJob = job;

            _logger.LogInformation("Southern Water Spills cache refreshed");
        }
        else
        {
            _logger.LogWarning("No Southern Water Spills returned");
        }
    }
}