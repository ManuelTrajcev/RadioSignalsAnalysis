using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Domain_Models;

public class User : IdentityUser
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public Role Role { get; set; }
}