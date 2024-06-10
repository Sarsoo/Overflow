using Microsoft.Extensions.Logging.Abstractions;
using Overflow.SouthernWater;

namespace Overflow.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Test1()
    {
        var southern = new SouthernWater.SouthernWaterApi(new HttpClient(), NullLogger<SouthernWaterApi>.Instance);
        await southern.LoadApiUrl();
        var spills = await southern.GetSpills();
    }
}