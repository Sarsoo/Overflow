using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Overflow.SouthernWater;

public class SouthernWaterApiJob
{
    [BsonId]
    public ObjectId _id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Interval { get; set; }
    public int TotalItems { get; set; }
    public List<Spill> Spills { get; set; }
}