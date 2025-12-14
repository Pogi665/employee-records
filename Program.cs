using Employee_Records.Data;
using Employee_Records.Services;
using Microsoft.AspNetCore.Identity;
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

// Identity Service Configuration
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // Set true if you want to require confirmed email before sign-in
    options.SignIn.RequireConfirmedAccount = false;
})
// This links Identity to your DbContext
.AddEntityFrameworkStores<ApplicationDbContext>();

// ---- SMTP email sender registration ----
builder.Services.Configure<Employee_Records.Services.SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, Employee_Records.Services.SmtpEmailSender>();

var app = builder.Build();

// --- 2. PIPELINE CONFIGURATION (Middleware) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Add Authentication Middleware
app.UseAuthentication();

// Add Authorization Middleware (required for login/access checks)
app.UseAuthorization();

// Map Razor Pages for scaffolded Identity UI
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=Index}/{id?}");

app.Run();