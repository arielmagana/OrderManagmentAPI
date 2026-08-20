namespace OrderManagement.Application.Common.Pagination;

/// <summary>
/// Generic paginated response wrapper per api.md specifications.
/// Used for all list endpoints to provide pagination metadata and items.
/// </summary>
/// <typeparam name="T">The type of items in the paginated response</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total count of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// The items in the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Creates a new paginated response.
    /// </summary>
    public PaginatedResponse(int pageNumber, int pageSize, int totalCount, List<T> items)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        Items = items;
    }
}
