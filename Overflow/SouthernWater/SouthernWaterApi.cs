using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Quartz.Util;

namespace Overflow.SouthernWater;

public partial class SouthernWaterApi
{
    private readonly HttpClient _client;
    private readonly ILogger<SouthernWaterApi> _logger;

    private static readonly string spillsEndpoint = "Spills/GetHistoricSpills";

    private string baseUrl;
    private string apiKey;

    public SouthernWaterApi(HttpClient client, ILogger<SouthernWaterApi> logger)
    {
        _client = client;
        _logger = logger;
    }

    [GeneratedRegex(@".*const APIURL = '(?<APIURL>.*)'.*\n.*const APIGWKEY = '(?<APIKEY>.*)'.*", RegexOptions.IgnoreCase)]
    private static partial Regex ApiUrlAndKey();

    public async Task LoadApiUrl()
    {
        var success = false;

        while (!success)
        {
            try
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri("https://www.southernwater.co.uk/scripts/beachbuoyhistoricspillstable.js"),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        {"Accept", "*/*"},
                        {"Accept-Language", "en-GB,en;q=0.5"},
                        // {"Accept-Encoding", "gzip, deflate, br, zstd"},
                        {"Cache-Control", "no-cache"},
                        {"Connection", "keep-alive"},
                        {"DNT", "1"},
                        {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:126.0) Gecko/20100101 Firefox/126.0"},
                        {"Upgrade-Insecure-Requests", "1"},
                        {"Referer", "https://www.southernwater.co.uk/our-region/clean-rivers-and-seas-task-force/beachbuoy-historic-release-table/"}
                    }
                };

                var content = await _client.SendAsync(request);

                content.EnsureSuccessStatusCode();

                success = true;

                var contentString = await content.Content.ReadAsStringAsync();

                Match m = ApiUrlAndKey().Match(contentString);

                var apiUrlFound = m.Groups.TryGetValue("APIURL", out var apiUrl);
                var apiKeyFound = m.Groups.TryGetValue("APIKEY", out var apiKey);

                if (apiUrlFound)
                {
                    baseUrl = apiUrl.Value;
                }

                if (apiKeyFound)
                {
                    this.apiKey = apiKey.Value;
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "HTTP Exception while API details, waiting {} before retrying", Static.Interval);
                await Task.Delay(Static.Interval);
            }
        }
    }

    public async Task<PagedItems<Spill>?> GetSpills(int page = 1, JsonSerialiser? jsonSerialiser = null)
    {
        if (baseUrl.IsNullOrWhiteSpace()) await LoadApiUrl();

        PagedItems<Spill>? parsedPage = null;
        var success = false;

        while (!success)
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(baseUrl + spillsEndpoint + "?page=" + page),
                    Method = HttpMethod.Get,
                    Headers =
                    {
                        { "Accept", "*/*" },
                        { "Accept-Language", "en-GB,en;q=0.5" },
                        // {"Accept-Encoding", "gzip, deflate, br, zstd"},
                        { "Cache-Control", "no-cache" },
                        { "Connection", "keep-alive" },
                        { "DNT", "1" },
                        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; rv:126.0) Gecko/20100101 Firefox/126.0" },
                        { "Upgrade-Insecure-Requests", "1" },
                        {
                            "Referer",
                            "https://www.southernwater.co.uk/our-region/clean-rivers-and-seas-task-force/beachbuoy-historic-release-table/"
                        },
                        { "x-Gateway-APIKey", apiKey },
                        { "X-Requested-With", "XMLHttpRequest" },
                    }
                };

                var content = await _client.SendAsync(request);

                content.EnsureSuccessStatusCode();

                success = true;

                parsedPage = (PagedItems<Spill>?)await content.Content.ReadFromJsonAsync(typeof(PagedItems<Spill>),
                    jsonSerialiser ?? new JsonSerialiser());

                if (parsedPage is not null)
                {
                    parsedPage.items.ForEach(x =>
                    {
                        x.eventStart = x.eventStart.ToUniversalTime();
                        x.eventStop = x.eventStop.ToUniversalTime();
                    });
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "HTTP Exception while loading page [{}], waiting {} before retrying", page, Static.Interval);
                await Task.Delay(Static.Interval);
            }
            catch (TaskCanceledException e)
            {
                _logger.LogError(e, "HTTP Timeout Exception while loading page [{}], waiting {} before retrying", page, 1.5 * Static.Interval);
                await Task.Delay(1.5 * Static.Interval);
            }
        }

        return parsedPage;
    }

    public async IAsyncEnumerable<PagedItems<Spill>?> GetAllSpills(TimeSpan interval, int? pageLimit = null, JsonSerialiser? jsonSerialiser = null)
    {
        Random rnd = new Random();

        var firstPage = await GetSpills(page: 1, jsonSerialiser);

        yield return firstPage;

        await Task.Delay(interval + TimeSpan.FromSeconds(rnd.Next(-Static.IntervalWiggleSeconds, Static.IntervalWiggleSeconds)));

        var pageCount = Math.Min(pageLimit ?? int.MaxValue, firstPage?.totalPages ?? 1);

        foreach (var pageNum in Enumerable.Range(2, pageCount - 1))
        {
            var nextPage = await GetSpills(page: pageNum, jsonSerialiser);

            if (nextPage is not null)
            {
                yield return nextPage;
            }

            await Task.Delay(interval);
        }
    }
}