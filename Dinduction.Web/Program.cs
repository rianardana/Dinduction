using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Dinduction.Infrastructure;
using Dinduction.Application.Interfaces;
using Dinduction.Infrastructure.UnitOfWork;
using Dinduction.Application.Services;
using Dinduction.Web.Profiles;
using Dinduction.Infrastructure.Services;
using Dinduction.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LegacyTrainingDb")));

//  Session (wajib untuk login tanpa auth cookie)
builder.Services.AddDistributedMemoryCache(); // ← wajib
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(GeneralMappingProfile)); // akan scan semua Profile di assembly

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISectionService, SectionService>();

var app = builder.Build();

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session
app.UseSession(); 

app.UseAuthorization();

app.MapControllerRoute(
    name: "root",
    pattern: "",
    defaults: new { controller = "Account", action = "Login" });


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");
    
app.Run();