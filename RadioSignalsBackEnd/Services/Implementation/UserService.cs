using Microsoft.AspNetCore.Identity;
using Services.Interface;
using Domain.DTO;
using Domain.Domain_Models;
using Repository.Interface;
using System.Collections.Generic;
using System.Linq;

public class UserService : IUserService
{
    private readonly IPasswordHasher<User> _hasher;
    private readonly IRepository<User> _userRepository;

    public UserService(IRepository<User> userRepositoryRepository,
        IPasswordHasher<User> hasher)
    {
        _userRepository = userRepositoryRepository;
        _hasher = hasher;
    }

    // ---------------- Register ----------------
    public RegisterResult Register(RegisterDto dto)
    {
        // Username must be unique – check the DB, not memory
        if (GetAll().Any(u => u.Username == dto.Username))
            return new RegisterResult { Success = false, Message = "Username already exists." };

        if (dto.Password != dto.RepeatPassword)
            return new RegisterResult { Success = false, Message = "Passwords do not match." };

        var user = new User
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Username = dto.Username,
            Email = dto.Email,
            Role = dto.Role,
            Password = _hasher.HashPassword(null!, dto.Password) // hash once
        };

        _userRepository.Insert(user); // IRepository should SaveChanges() inside or expose Save()

        return new RegisterResult { Success = true, Message = "Registration successful." };
    }

    // ---------------- Authenticate ----------------
    public User? Authenticate(string username, string password)
    {
        var user = GetAll().SingleOrDefault(u => u.Username == username);
        if (user is null) return null;

        var result = _hasher.VerifyHashedPassword(user, user.Password, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }

    // ---------------- GetAll ----------------
    public List<User> GetAll() => _userRepository.GetAll(u => u).ToList();
}