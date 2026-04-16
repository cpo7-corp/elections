namespace Elections.Api.Models;

public class Party
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Leader { get; set; }
    public string? ImageUrl { get; set; }
    public int Order { get; set; }
}
