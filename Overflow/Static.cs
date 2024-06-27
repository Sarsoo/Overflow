namespace Overflow;

public static class Static
{
    public static readonly string DatabaseName = "overflow";
    public static readonly string JobCollectionName = "southern_water_api_job";
    public static readonly string SpillCollectionName = "southern_water_spills";

    public static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);
    public static readonly int IntervalWiggleSeconds = 10;
}