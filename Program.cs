using Blazored.LocalStorage;
using BTECH_APP;
using BTECH_APP.Components;
using BTECH_APP.Models;
using BTECH_APP.Models.Email;
using BTECH_APP.Services.Admin;
using BTECH_APP.Services.Admin.Interfaces;
using BTECH_APP.Services.Applicant;
using BTECH_APP.Services.Applicant.Interfaces;
using BTECH_APP.Services.Auth;
using BTECH_APP.Services.Auth.Interfaces;
using BTECH_APP.Services.Email;
using BTECH_APP.Services.Guest;
using BTECH_APP.Services.Guest.Interfaces;
using BTECH_APP.Services.StaticData;
using BTECH_APP.Services.StaticData.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Load environment variables for MySQL connection
var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

// ✅ 1.1 Log Environment Variables (for Render diagnostics)
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("🔍 [ENV CHECK] Loading MySQL Environment Variables...");
Console.WriteLine($"DB_HOST: {(string.IsNullOrWhiteSpace(dbHost) ? "[MISSING]" : dbHost)}");
Console.WriteLine($"DB_PORT: {(string.IsNullOrWhiteSpace(dbPort) ? "[MISSING]" : dbPort)}");
Console.WriteLine($"DB_NAME: {(string.IsNullOrWhiteSpace(dbName) ? "[MISSING]" : dbName)}");
Console.WriteLine($"DB_USER: {(string.IsNullOrWhiteSpace(dbUser) ? "[MISSING]" : dbUser)}");
Console.WriteLine($"DB_PASSWORD: {(string.IsNullOrWhiteSpace(dbPassword) ? "[MISSING]" : "[LOADED]")}");
Console.ResetColor();

// ✅ 2. Build the connection string securely
var connectionString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPassword};SslMode=Required;";

// ✅ 3. Register EF Core DbContext
builder.Services.AddDbContext<BTECHDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAutoMapper(typeof(MappingProfile));

// ✅ 4. Register HTTP client for components
builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});

// ✅ 5. Configure Authentication & Authorization
builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/";
        options.AccessDeniedPath = "/Not-Authorized";
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// ✅ 6. Add external packages
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<UserContext>();

// ✅ 7. Configure Email Settings using environment variables
builder.Services.Configure<EmailSettingsModel>(options =>
{
    options.SmtpServer = Environment.GetEnvironmentVariable("EMAIL_SMTP") ?? "smtp.gmail.com";
    options.Port = int.TryParse(Environment.GetEnvironmentVariable("EMAIL_PORT"), out int port) ? port : 587;
    options.SenderName = Environment.GetEnvironmentVariable("EMAIL_SENDER_NAME") ?? "BTECH Admission";
    options.Username = Environment.GetEnvironmentVariable("EMAIL_USER");
    options.Password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");
});

builder.Services.AddTransient<IEmailService, EmailService>();

#region Guest Services
builder.Services.AddScoped<IGuestService, GuestService>();
#endregion

#region Admin Services
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<BTECH_APP.Services.Admin.Interfaces.IApplicantService, BTECH_APP.Services.Admin.ApplicantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInstituteService, InstituteService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IRequirementService, RequirementService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
#endregion

#region Applicant Services
builder.Services.AddScoped<BTECH_APP.Services.Applicant.Interfaces.IApplicantService, BTECH_APP.Services.Applicant.ApplicantService>();
builder.Services.AddScoped<IStep1ApplicantService, Step1ApplicantService>();
builder.Services.AddScoped<IStep2ApplicantService, Step2ApplicantService>();
builder.Services.AddScoped<IStep5ApplicantService, Step5ApplicantService>();
#endregion

#region Static Data Services
builder.Services.AddScoped<IStaticDataService, StaticDataService>();
#endregion

builder.Services.AddHttpClient();

var app = builder.Build();

// ✅ 8. Seed or load initial data
using (var scope = app.Services.CreateScope())
{
    var academicYear = scope.ServiceProvider.GetService<IAcademicYearService>();
    await academicYear.GetCurrentAcademic();

    var runStaticData = scope.ServiceProvider.GetRequiredService<IStaticDataService>();
    await runStaticData.RunStaticData();
}

// ✅ 9. Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();

// ✅ 10. Map Razor Components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ✅ 11. Custom POST endpoints
app.MapPost("/", async (IAuthService _Service, [FromForm] string email, [FromForm] string password) =>
{
    var success = await _Service.LoginAsync(new BTECH_APP.Models.Auth.LoginModel() { Email = email, Password = password });
    return success ? Results.Redirect("/loading") : Results.Redirect("/");
})
.AllowAnonymous()
.DisableAntiforgery();

app.MapPost("/logout-page", async (IAuthService _Service) =>
{
    await _Service.LogoutAsync();
    return Results.Redirect("/");
})
.AllowAnonymous()
.DisableAntiforgery();

app.MapPost("/re-login", async (IAuthService _Service, [FromForm] int userId) =>
{
    var success = await _Service.Relogin(userId);
    return success ? Results.Redirect("/profile") : Results.Redirect("/");
})
.AllowAnonymous()
.DisableAntiforgery();

app.Run();
