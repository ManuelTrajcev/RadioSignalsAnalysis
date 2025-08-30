using Microsoft.AspNetCore.Identity;
using Services.Interface;
using Domain.DTO;
using Domain.Domain_Models;
using System.Collections.Concurrent;

public class UserService : IUserService
{
    private readonly PasswordHasher<User> _hasher = new();
    private static ConcurrentDictionary<string, string> _users = new();

    public RegisterResult Register(RegisterDto dto)
    {
        if (_users.ContainsKey(dto.Username))
            return new RegisterResult { Success = false, Message = "Username already exists." };

        var user = new User { Username = dto.Username };
        string hashedPassword = _hasher.HashPassword(user, dto.Password);

        _users[dto.Username] = hashedPassword;

        return new RegisterResult { Success = true, Message = "Registration successful." };
    }

    public User Authenticate(string username, string password)
    {
        if (!_users.TryGetValue(username, out var hashedPassword))
            return null;

        var user = new User { Username = username };
        var result = _hasher.VerifyHashedPassword(user, hashedPassword, password);
        if (result == PasswordVerificationResult.Success)
            return user;

        return null;
    }
}