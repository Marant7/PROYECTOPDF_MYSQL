using Microsoft.AspNetCore.Authentication.Cookies;
using NegocioPDF.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Obtener la cadena de conexión desde appsettings.json
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Verificar que la cadena de conexión no sea nula o vacía
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("La cadena de conexión 'DefaultConnection' no se encontró o es nula.");
}

// Registrar servicios
builder.Services.AddSingleton<UsuarioRepository>(provider => new UsuarioRepository(connectionString));
builder.Services.AddSingleton<DetalleSuscripcionRepository>(provider => new DetalleSuscripcionRepository(connectionString));
builder.Services.AddSingleton<OperacionesPDFRepository>(provider => new OperacionesPDFRepository(connectionString));

// Configurar autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();