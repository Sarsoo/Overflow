using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Overflow.SouthernWater;

public partial class SouthernWater
{
    private readonly HttpClient _client;

    private static readonly string spillsEndpoint = "Spills/GetHistoricSpills";

    private string baseUrl;
    private string apiKey;

    public SouthernWater(HttpClient client)
    {
        _client = client;
    }

    [GeneratedRegex(@".*const APIURL = '(?<APIURL>.*)'.*\n.*const APIGWKEY = '(?<APIKEY>.*)'.*", RegexOptions.IgnoreCase)]
    private static partial Regex ApiUrlAndKey();

    public async Task LoadApiUrl()
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

    public async Task<PagedItems<Spill>?> GetSpills(int page = 1, JsonSerialiser? jsonSerialiser = null)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri(baseUrl + spillsEndpoint),
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
                {"Referer", "https://www.southernwater.co.uk/our-region/clean-rivers-and-seas-task-force/beachbuoy-historic-release-table/"},
                {"x-Gateway-APIKey", apiKey},
                {"X-Requested-With", "XMLHttpRequest"},
            }
        };

        request.Options.TryAdd("page", page);

        var content = await _client.SendAsync(request);

        content.EnsureSuccessStatusCode();

        return (PagedItems<Spill>?) await content.Content.ReadFromJsonAsync(typeof(PagedItems<Spill>), jsonSerialiser ?? new JsonSerialiser());
    }
}