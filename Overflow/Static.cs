namespace Overflow;

public static class Static
{
    public static readonly string DatabaseName = "overflow";
    public static readonly string CollectionName = "southern_water_api_job";

    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    public static readonly int IntervalWiggleSeconds = 10;
}