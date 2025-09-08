using Domain.Domain_Models;
using Domain.DTO;
using Microsoft.AspNetCore.Identity;
using Repository.Interface;
using Services.Interface;
using System.Collections.Concurrent;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterDto dto)
    {
        var existing = await _userRepository.FindByNameAsync(dto.Username);
        if (existing != null)
            return new RegisterResult { Success = false, Message = "Username already exists." };

        if (dto.Password != dto.RepeatPassword)
            return new RegisterResult { Success = false, Message = "Passwords do not match." };

        var user = new User
        {
            UserName = dto.Username,
            Email = dto.Email,
            Name = dto.Name,
            Surname = dto.Surname,
            Role = dto.Role
        };

        var result = await _userRepository.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return new RegisterResult
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new RegisterResult { Success = true, Message = "Registration successful." };
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.FindByNameAsync(username);
        if (user == null)
            return null;

        var isValid = await _userRepository.CheckPasswordAsync(user, password);
        return isValid ? user : null;
    }
}