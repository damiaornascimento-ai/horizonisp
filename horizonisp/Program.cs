
using Microsoft.EntityFrameworkCore;
using horizonisp.Context;

var builder = WebApplication.CreateBuilder(args);

// Configura o DbContext com SQL Server usando a connection string do appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona suporte a Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Mapeia as rotas padrão do Razor Pages
app.MapRazorPages();

app.Run();