namespace Overflow.SouthernWater;

public class PagedItems<T>
{
    public int currentPage { get; set; }
    public int totalItems { get; set; }
    public int totalPages { get; set; }
    public List<T> items { get; set; }
}