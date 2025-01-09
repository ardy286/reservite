using Microsoft.EntityFrameworkCore;
using ReserViteApplication.Data;
using Microsoft.AspNetCore.Http;
using ReserViteApplication.Models;
using ReserViteApplication.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using ReserViteApplication.Settings;
using SendGrid.Extensions.DependencyInjection;
using ReserViteApplication.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Ajouter le contexte de base de données à la collection de services.
// Utilisation de SQL Server avec la chaîne de connexion nommée "LocalSqlServerConnection"
// définie dans le fichier de configuration (appsettings.json).
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocalSqlServerConnection")));
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Ajouter les services pour Twilio et SendGrid
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("TwilioSettings"));
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("SendGridSettings"));
builder.Services.AddScoped<ISMSSenderService, SMSSenderService>();
builder.Services.AddScoped<IEmailSender, EmailSenderService>();
builder.Services.AddSendGrid(options =>
{
    options.ApiKey = builder.Configuration.GetSection("SendGridSettings:ApiKey").Value;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSignalR(); // Ajoutez SignalR au conteneur DI

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login"; // Chemin vers la page de connexion
        options.AccessDeniedPath = "/Account/AccessDenied"; // Optionnel
    });

builder.Services.AddAuthorization();


// Ajoutez la gestion des sessions
builder.Services.AddDistributedMemoryCache(); // Utilisation de la mémoire pour stocker les sessions
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Définir la durée de la session
    options.Cookie.Name = ".AspNetCore.Session"; // Nom du cookie de session
});
// Ajoutez la configuration du service IHttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Créer un scope pour les services afin d'initialiser la base de données.
// Cela permet d'appeler TestData.Initialize pour ajouter les données de test.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Appliquer les migrations et créer la base de données si nécessaire.
    context.Database.Migrate();
    // Appeler la méthode Initialize pour ajouter les données de test dans la base de données
    DonneeBD.Initialize(services);
}



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.Services.CreateScope();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chambres}/{action=Index}/{id?}");

app.MapHub<MessagerieHub>("/messagerieHub").RequireAuthorization();
// Mappez les pages Razor
app.MapRazorPages();

app.Run();
