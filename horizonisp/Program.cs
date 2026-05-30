
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using horizonisp.Api;
using horizonisp.Auth;
using horizonisp.Configuration;
using horizonisp.Context;
using horizonisp.Data;
using horizonisp.Models;
using Microsoft.Extensions.Options;

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

builder.Services.AddHttpClient("WhatsApp");

builder.Services.AddHttpClient("Olt")
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthSchemes.Admin, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.Admin);
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy(AuthSchemes.Cliente, policy =>
    {
        policy.AddAuthenticationSchemes(AuthSchemes.Cliente);
        policy.RequireAuthenticatedUser();
    });
});
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
builder.Services.AddScoped<IChamadoService, ChamadoService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IRedeService, RedeService>();
builder.Services.AddScoped<IOltIntegracaoService, OltIntegracaoService>();
builder.Services.AddScoped<IRedeSincronizacaoService, RedeSincronizacaoService>();
builder.Services.AddHostedService<FaturamentoBackgroundService>();
builder.Services.AddHostedService<RedeBackgroundService>();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Index", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Clientes", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Planos", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Assinaturas", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Faturas", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Faturamento", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Chamados", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Relatorios", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Rede", AuthSchemes.Admin);
    options.Conventions.AuthorizeFolder("/Integracoes", AuthSchemes.Admin);
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
app.UseMiddleware<ApiKeyMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<Usuario>>();
    var clientePasswordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<Cliente>>();
    await DbInitializer.SeedAsync(db, passwordHasher, clientePasswordHasher);
}

app.MapPost("/api/pix/webhook", async (
    PixWebhookRequest request,
    HttpRequest httpRequest,
    IFaturamentoService faturamentoService,
    IOptions<HorizonIspOptions> options) =>
{
    var token = httpRequest.Headers["X-Webhook-Token"].FirstOrDefault();
    if (!string.Equals(token, options.Value.Pix.WebhookToken, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    var resultado = await faturamentoService.ConfirmarPagamentoPixAsync(
        request.TxId,
        request.Valor,
        "Webhook",
        request.EndToEndId);

    return resultado.Sucesso
        ? Results.Ok(resultado)
        : Results.BadRequest(resultado);
}).AllowAnonymous();

app.MapV1Endpoints();
app.MapRazorPages();
app.Run();
