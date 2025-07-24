using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TenentManagement.Services.Bcrypt;
using TenentManagement.Services.Database;
using TenentManagement.Services.Mail;
using AuthenticationService = TenentManagement.Services.Authentication.AuthenticationService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<TenentManagement.Services.User.UserService>();

builder.Services.AddSingleton<DatabaseConnection>();


builder.Services.AddScoped<AuthenticationService>();


builder.Services.AddScoped<MailService>();

// Register the PropertyService
builder.Services.AddScoped<TenentManagement.Services.Property.PropertyService>();

builder.Services.AddScoped<TenentManagement.Services.Property.Unit.UnitService>();

builder.Services.AddScoped<TenentManagement.Services.Payment.PaymentInvoiceService>();

builder.Services.AddScoped<TenentManagement.Services.Payment.PaymentService>();

builder.Services.AddScoped<TenentManagement.Services.Payment.PaymentQRImageService>();

builder.Services.AddScoped<TenentManagement.Services.Payment.PaymentProofService>();

//BCrypt service for password hashing
builder.Services.AddScoped<BCryptService>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Authentication/Login";
        options.LogoutPath = "/Authentication/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Session expires in 30 minutes
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.MaxAge = options.ExpireTimeSpan;
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "tenent_management.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
