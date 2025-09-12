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

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAutoMapper(typeof(MappingProfile));

// ✅ Get connection string from environment variable (e.g. from Render.com)
var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

// ✅ Optional fallback (for local dev)
// var connectionString = Environment.GetEnvironmentVariable("databaseconnection") 
//     ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BTECHDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped(sp =>
{
    var navigationManager = sp.GetRequiredService<NavigationManager>();
    return new HttpClient
    {
        BaseAddress = new Uri(navigationManager.BaseUri)
    };
});

builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/";
        options.AccessDeniedPath = "/Not-Authorized";
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<UserContext>();

builder.Services.Configure<EmailSettingsModel>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

#region Guest
builder.Services.AddScoped<IGuestService, GuestService>();
#endregion Guest

#region Admin
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<BTECH_APP.Services.Admin.Interfaces.IApplicantService, BTECH_APP.Services.Admin.ApplicantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInstituteService, InstituteService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IRequirementService, RequirementService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
#endregion Admin

#region Applicant
builder.Services.AddScoped<BTECH_APP.Services.Applicant.Interfaces.IApplicantService, BTECH_APP.Services.Applicant.ApplicantService>();
builder.Services.AddScoped<IStep1ApplicantService, Step1ApplicantService>();
builder.Services.AddScoped<IStep2ApplicantService, Step2ApplicantService>();
builder.Services.AddScoped<IStep5ApplicantService, Step5ApplicantService>();
#endregion Applicant

#region StaticData
builder.Services.AddScoped<IStaticDataService, StaticDataService>();
#endregion StaticData

builder.Services.AddHttpClient();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var academicYear = scope.ServiceProvider.GetService<IAcademicYearService>();
    await academicYear.GetCurrentAcademic();

    var runStaticData = scope.ServiceProvider.GetRequiredService<IStaticDataService>();
    await runStaticData.RunStaticData();
}

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/", async (IAuthService _Service, [FromForm] string email, [FromForm] string password) =>
{
    var success = await _Service.LoginAsync(
        new BTECH_APP.Models.Auth.LoginModel() { Email = email, Password = password });

    if (success)
        return Results.Redirect("/loading");

    return Results.Redirect("/");
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

    if (success)
        return Results.Redirect("/profile");

    return Results.Redirect("/");
})
.AllowAnonymous()
.DisableAntiforgery();

app.Run();
