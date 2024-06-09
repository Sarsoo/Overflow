using MongoDB.Bson;

namespace Overflow.SouthernWater;

public class SouthernWaterApiJob
{
    public ObjectId _id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Interval { get; set; }
    public int TotalItems { get; set; }
    public List<Spill> Spills { get; set; }
}