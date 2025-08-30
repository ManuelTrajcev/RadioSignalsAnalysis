namespace Domain.Domain_Models;

public class User : BaseEntity
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    // public int Id { get; set; }
}