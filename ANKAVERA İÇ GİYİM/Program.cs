using ANKAVERA_İÇ_GİYİM.Models;
using ANKAVERA_İÇ_GİYİM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// 1. DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.SignIn.RequireConfirmedAccount = false; // ← ÖNEMLİ
    options.SignIn.RequireConfirmedEmail = false; // ← ÖNEMLİ
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// 4. Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// ── Email Ayarları ────────────────────────────────────────────────────────
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddTransient<IEmailService, EmailService>();

// 5. MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 6. Pipeline — SIRA KESİNLİKLE BU OLMALI
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();        // ← Session önce
app.UseAuthentication(); // ← Sonra Authentication
app.UseAuthorization();  // ← En son Authorization

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 7. Seed
await SeedAsync(app);

app.Run();

static async Task SeedAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    // Apply any pending EF Core migrations so the schema exists on a fresh
    // database (e.g. the SQL Server container spun up by docker compose).
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { "Admin", "User" })
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var email = config["SeedAdmin:Email"] ?? "admin@ankavera.com";
    var pass = config["SeedAdmin:Password"];

    // Never hardcode the admin password. If it isn't configured (user-secrets,
    // env var, etc.), skip seeding rather than falling back to a known value.
    if (string.IsNullOrWhiteSpace(pass))
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("SeedAdmin:Password is not configured — skipping admin user seed.");
        return;
    }

    if (await userMgr.FindByEmailAsync(email) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Ankavera Admin",
            EmailConfirmed = true,
            RegisteredAt = DateTime.Now
        };
        var r = await userMgr.CreateAsync(admin, pass);
        if (r.Succeeded)
            await userMgr.AddToRoleAsync(admin, "Admin");
    }
}