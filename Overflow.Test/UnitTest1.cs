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
        var southern = new SouthernWater.SouthernWater(new HttpClient());
        await southern.LoadApiUrl();
        var spills = await southern.GetSpills();
    }
}