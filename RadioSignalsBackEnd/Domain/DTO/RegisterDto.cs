using Domain.Enums;

namespace Domain.DTO;

public class RegisterDto
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string RepeatPassword { get; set; }
    public string Email { get; set; }
    public Role Role { get; set; }
}