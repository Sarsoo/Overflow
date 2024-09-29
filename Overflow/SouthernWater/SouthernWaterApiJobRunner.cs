using System.Globalization;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Overflow.SouthernWater;

public class SouthernWaterApiJobRunner(SouthernWaterApi client, ILogger<SouthernWaterApiJobRunner> logger)
{
    protected readonly ILogger<SouthernWaterApiJobRunner> _logger = logger;

    public async Task LoadSpills(int? pageLimit = null)
    {
        var interval = Static.Interval;
        var job = new SouthernWaterApiJob
        {
            StartTime = DateTime.UtcNow,
            Spills = new(),
            Interval = interval
        };

        using var scope = _logger.BeginScope(new Dictionary<string, string>
        {
            {"StartTime", job.StartTime.ToString(CultureInfo.InvariantCulture)},
            {"Interval", job.Interval.ToString()},
        });

        _logger.LogInformation("Starting Southern Water API Pull Job");

        await JobCreated(job);

        var spills = client.GetAllSpills(interval, pageLimit: pageLimit);

        await foreach (var page in spills)
        {
            if (page is null) continue;

            _logger.LogInformation("Processing page [{}/{}]", page.currentPage, page.totalPages);

            page.items.ForEach(s => s.JobId = job._id);

            job.TotalItems = page.totalItems;
            // job.Spills.AddRange(page.items);
            try
            {
                await PageLoaded(job, page);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception while running page loaded callback");
            }
        }

        job.EndTime = DateTime.UtcNow;

        _logger.LogInformation("Finished Southern Water API Pull Job");

        try
        {
            await JobFinished(job);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while running job finished callback");
        }
    }

    protected virtual Task JobCreated(SouthernWaterApiJob job)
    {
        return Task.CompletedTask;
    }

    protected virtual Task PageLoaded(SouthernWaterApiJob job, PagedItems<Spill> page)
    {
        return Task.CompletedTask;
    }

    protected virtual Task JobFinished(SouthernWaterApiJob job)
    {
        return Task.CompletedTask;
    }
}

public class SouthernWaterApiJobRunnerPersisting(
    SouthernWaterApi client,
    ILogger<SouthernWaterApiJobRunner> logger,
    IMongoDatabase mongo)
    : SouthernWaterApiJobRunner(client, logger)
{
    private readonly IMongoCollection<SouthernWaterApiJob> _jobCollection = mongo.GetCollection<SouthernWaterApiJob>(Static.JobCollectionName);
    private readonly IMongoCollection<Spill> _spillCollection = mongo.GetCollection<Spill>(Static.SpillCollectionName);

    protected override async Task JobCreated(SouthernWaterApiJob job)
    {
        await _jobCollection.InsertOneAsync(job);
    }

    protected override async Task PageLoaded(SouthernWaterApiJob job, PagedItems<Spill> page)
    {
        await _spillCollection.InsertManyAsync(page.items);
    }

    protected override async Task JobFinished(SouthernWaterApiJob job)
    {
        var finder = Builders<SouthernWaterApiJob>.Filter
            .Eq(j => j._id, job._id);

        var update = Builders<SouthernWaterApiJob>.Update
            .Set(j => j.EndTime, job.EndTime)
            .Set(j => j.TotalItems, job.TotalItems);

        await _jobCollection.UpdateOneAsync(finder, update);
    }
}