using Trim.Models;

namespace Trim.Models.ViewModels;

public class UserDetailsVM
{
    public ApplicationUser User { get; set; } = new();
    public bool IsLocked {get; set;} 
    public string Role {get; set;}
}