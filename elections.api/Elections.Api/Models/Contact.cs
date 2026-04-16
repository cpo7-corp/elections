namespace Elections.Api.Models;

public class ContactReq
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Phone { get; set; }
    public required string Email { get; set; }
    public required string Message { get; set; }
}
