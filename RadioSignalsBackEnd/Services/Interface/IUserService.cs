using Domain.Domain_Models;
using Domain.DTO;

namespace Services.Interface;

public interface IUserService
{
    RegisterResult Register(RegisterDto dto);
    User Authenticate(string username, string password);
    List<User> GetAll();
}

public class RegisterResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}