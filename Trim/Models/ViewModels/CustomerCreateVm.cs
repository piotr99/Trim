using Trim.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Trim.Models.ViewModels;

public class CustomerCreateVm
{
    public Customer Customer { get; set; } = new();
    public List<SelectListItem> TransportCompanies { get; set; } = new();
    public List<SelectListItem> Salespeople { get; set; } = new();
}
