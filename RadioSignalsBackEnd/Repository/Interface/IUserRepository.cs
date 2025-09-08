using Domain.Domain_Models;
using Microsoft.AspNetCore.Identity;

namespace Repository.Interface;

public interface IUserRepository
{
    Task<IdentityResult> CreateAsync(User user, string password);
    Task<User?> FindByNameAsync(string username);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<IdentityResult> AddToRoleAsync(User user, string role);
    Task<IList<string>> GetRolesAsync(User user);
}