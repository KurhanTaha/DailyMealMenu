using DailyMealMenu.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<MealsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MealsDB")));

// Uygulama başlatılma zamanı – eski cookie’leri geçersiz kılmak için referans
var appStart = DateTimeOffset.UtcNow;

// Cookie Authentication
builder.Services
    .AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.Cookie.Name = "DailyMealMenu.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Denied";

        // Cookie ömrü (örn. 8 saat) ve sliding KAPALI
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;

        // Uygulama her restart olduğunda, o restart'tan önce kesilmiş cookie'leri reddet
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = ctx =>
            {
                var issued = ctx.Properties?.IssuedUtc;
                if (issued.HasValue && issued.Value < appStart)
                {
                    // Cookie uygulama açılmadan önce verilmiş → oturumu düşür
                    ctx.RejectPrincipal();
                    return ctx.HttpContext.SignOutAsync("Cookies");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
