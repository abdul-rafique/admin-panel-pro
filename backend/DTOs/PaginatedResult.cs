namespace AdminPanel.Api.DTOs;

public record PaginatedResult<T>(
    IEnumerable<T> Data,
    int Total,
    int Page,
    int Limit)
{
    public int Pages => (int)Math.Ceiling(Total / (double)Limit);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < Pages;
}
