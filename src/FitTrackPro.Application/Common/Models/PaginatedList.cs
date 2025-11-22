using System.Text.Json.Serialization;

namespace FitTrackPro.Application.Common.Models;

public class PaginatedList<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set;}
    public int TotalPages { get; set;}
    public int TotalCount { get; set; }

    [JsonIgnore]
    public bool HasPreviousPage => PageNumber > 1;

    [JsonIgnore]
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList() { }

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }
}
