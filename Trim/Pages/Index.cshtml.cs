using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

using Trim.Models;
using System.Text.Json;
using System.Globalization;
using System.Reflection;

namespace Trim.Pages;


[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    public void OnGet()
    {
        //RedirectToPage("/Home");
    }



}