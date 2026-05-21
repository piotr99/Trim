using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.AI;
using OllamaSharp;
using Serilog;
using Serilog.Events;
using Trim.DbContext;
using Trim.Helpers;
using Trim.Models;
using Trim.Services;
using Trim.Services.AI;

var builder = WebApplication.CreateBuilder(args);

//Logowanie
builder.Host.UseSerilog((context, services, configuration) => configuration
.ReadFrom.Configuration(context.Configuration)
.ReadFrom.Services(services)
.Enrich.FromLogContext());

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MainDb")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var ollamaUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
var ollamaModel = builder.Configuration["Ollama:Model"] ?? "glm-5.1:cloud";

builder.Services.AddSingleton<IChatClient>(sp =>
{
    return new Microsoft.Extensions.AI.OllamaChatClient(new Uri(ollamaUrl), ollamaModel);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SalesAdministrator", p => p.RequireRole("SalesAdministrator"));
    options.AddPolicy("Salesperson", p => p.RequireRole("Salesperson"));
    options.AddPolicy("Administrator", p => p.RequireRole("Administrator"));
    options.AddPolicy("AdminOrSalesperson",
        p => p.RequireRole("Administrator", "Salesperson"));
    options.AddPolicy("Customer", p => p.RequireRole("Customer"));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});
builder.Services.AddScoped<IOfferCalculator, OfferCalculator>();
builder.Services.AddScoped<ICallFactoryForVinHelper, CallFactoryForVinHelper>();
builder.Services.AddScoped<IUpdateOfferPricing, UpdateOfferPricing>();
builder.Services.AddScoped<ISalespeopleHelper, SalespeopleHelper>();
builder.Services.AddScoped<IOrderHelper, OrderHelper>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<ICaseService, CaseService>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");

    options.Conventions.AuthorizeFolder("/Customer/", "Customer");

    options.Conventions.AuthorizeFolder("/Admin/", "Administrator");
    options.Conventions.AuthorizeFolder("/NewUI", "AdminOrSalesperson");
    options.Conventions.AuthorizeFolder("/SalesAdministrator/", "SalesAdministrator");

    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
});

builder.Services.AddControllers();
var app = builder.Build();
await DataSeeder.SeedAsync(app.Services);

app.UseSerilogRequestLogging();

app.MapGet("/", () => Results.Redirect("/Home"));
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//Przy każdym żądaniu logujemy informacje o użytkowniku, co pozwala nam później analizować logi pod kątem aktywności konkretnych użytkowników.
app.UseMiddleware<Trim.Middleware.UserLoggingMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null || httpContext.Response.StatusCode > 499)
        {
            return LogEventLevel.Error;
        }    
        if (httpContext.Request.Path.StartsWithSegments("/NewUI/Case", StringComparison.OrdinalIgnoreCase))
        {
            return LogEventLevel.Debug;
        }
        return LogEventLevel.Information;
    };
});

app.MapControllers();
app.MapRazorPages();
app.Run();
