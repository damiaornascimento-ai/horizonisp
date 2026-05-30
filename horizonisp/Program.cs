
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using horizonisp.Auth;
using horizonisp.Configuration;
using horizonisp.Context;
using horizonisp.Data;
using horizonisp.Models;
using horizonisp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HorizonIspOptions>(
    builder.Configuration.GetSection(HorizonIspOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("Mikrotik")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = AuthSchemes.Admin;
        options.DefaultChallengeScheme = AuthSchemes.Admin;
    })
    .AddCookie(AuthSchemes.Admin, options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.Cookie.Name = "HorizonISP.Admin";
    })
    .AddCookie(AuthSchemes.Cliente, options =>
    {
        options.LoginPath = "/Portal/Login";
        options.AccessDeniedPath = "/Portal/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.Cookie.Name = "HorizonISP.Cliente";
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PasswordHasher<Usuario>>();
builder.Services.AddScoped<PasswordHasher<Cliente>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IClienteAuthService, ClienteAuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPortalService, PortalService>();
builder.Services.AddScoped<IPixService, PixService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMikrotikService, MikrotikService>();
builder.Services.AddScoped<IFaturamentoService, FaturamentoService>();
builder.Services.AddHostedService<FaturamentoBackgroundService>();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Portal", AuthSchemes.Cliente);
    options.Conventions.AllowAnonymousToPage("/Login");
    options.Conventions.AllowAnonymousToPage("/Portal/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Privacy");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<Usuario>>();
    var clientePasswordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<Cliente>>();
    await DbInitializer.SeedAsync(db, passwordHasher, clientePasswordHasher);
}

app.MapRazorPages();
app.Run();
