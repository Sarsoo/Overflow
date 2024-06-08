namespace Overflow.SouthernWater;

public class Spill
{
    public int id { get; set; }
    public int eventId { get; set; }
    public int siteUnitNumber { get; set; }
    public string bathingSite { get; set; }
    public DateTime eventStart { get; set; }
    public DateTime eventStop { get; set; }
    public int duration { get; set; }
    public string status { get; set; }
    public int associatedSiteId { get; set; }
    public string outfallName { get; set; }
    public bool isImpacting { get; set; }
    public int overFlowSiteId { get; set; }
    public List<Spill> historicSpillsList { get; set; }
}