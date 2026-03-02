using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trim.DbContext;
using Trim.Models;
using Microsoft.EntityFrameworkCore.Design;
using Trim.Helpers;

var builder = WebApplication.CreateBuilder(args);

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ListenAnyIP(5072, listenOptions =>
//     {
//         listenOptions.UseHttps();
//     });
// });

// 1) DbContext MUSI być przed Build()
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MainDb")));

// 2) Identity (na tym samym DbContext)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3) Authorization – UWAGA: role muszą być takie jak w bazie/seedzie
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SalesAdministrator", p => p.RequireRole("SalesAdministrator"));
    options.AddPolicy("Salesperson", p => p.RequireRole("Salesperson"));
    options.AddPolicy("Administrator", p => p.RequireRole("Administrator"));
    options.AddPolicy("AdminOrSalesperson",
        p => p.RequireRole("Administrator", "Salesperson"));
});

// 4) Cookie paths – przed Build()
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddScoped<IOfferCalculator, OfferCalculator>();
// 5) Razor Pages + konwencje (tylko raz)
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");

    options.Conventions.AuthorizeFolder("/Admin/", "Administrator");
    options.Conventions.AuthorizeFolder("/Salesperson", "AdminOrSalesperson");
    options.Conventions.AuthorizeFolder("/SalesAdministrator/", "SalesAdministrator");

    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
});



var app = builder.Build();


await DataSeeder.SeedAsync(app.Services);


app.MapGet("/", () => Results.Redirect("/Home"));
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
