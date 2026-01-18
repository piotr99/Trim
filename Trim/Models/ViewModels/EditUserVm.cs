using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Trim.Models.ViewModels;

public class EditUserVm
{
    public string UserId { get; set; } = "";

    [Required] public string FirstName { get; set; } = "";
    [Required] public string LastName  { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";

    public string? SelectedRole { get; set; }
    public List<SelectListItem> Roles { get; set; } = new();
}

