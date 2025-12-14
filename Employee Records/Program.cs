using Employee_Records.Data;
using Employee_Records.Models;
using Employee_Records.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVICE CONFIGURATION ---

builder.Services.AddControllersWithViews();

// MySQL DbContext configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Email Service Configuration
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// Data Seeder Service for mock attendance/schedule data
builder.Services.AddScoped<DataSeederService>();

// Payslip Calculation Service
builder.Services.AddScoped<PayslipCalculationService>();

// Identity Service Configuration with Role Support
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    // Note: You can set this to true if you want email confirmation
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() // Enable role support
.AddEntityFrameworkStores<ApplicationDbContext>();

var app = builder.Build();

// --- 3. SEED ROLES ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Ensure database is created and migrations are applied before accessing Identity stores
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "Admin", "Employee" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

// --- 4. PIPELINE CONFIGURATION (Middleware) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// **CRITICAL FIXES START HERE**

// 1. Add Authentication Middleware
app.UseAuthentication();

// 2. Add Authorization Middleware (required for login/access checks)
app.UseAuthorization();

// 3. Add MapRazorPages for the scaffolded Identity UI pages
app.MapRazorPages();

// 4. Map the default MVC controller route (this must come after MapRazorPages)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();