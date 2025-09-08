using Domain.Domain_Models;
using Domain.DTO;

namespace Services.Interface;

public interface IUserService
{
    Task<RegisterResult> RegisterAsync(RegisterDto dto);
    Task<User?> AuthenticateAsync(string username, string password);
}

public class RegisterResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}