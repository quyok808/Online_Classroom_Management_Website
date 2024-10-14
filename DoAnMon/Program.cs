using DoAnMon.Data;
using DoAnMon.IdentityCudtomUser;
using DoAnMon.ModelListSVDownload;
using DoAnMon.Models;
using DoAnMon.SendMail;
using DoAnMon.SignalR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddScoped<IStudent, StudentRepo>();

builder.Services.AddDefaultIdentity<CustomUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue; // 1GB
});
builder.WebHost.ConfigureKestrel(serverOptions =>
{
	serverOptions.Limits.MaxRequestBodySize = 1000 * 1024 * 1024; // 50MB (có thể điều chỉnh tùy nhu cầu)
});

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<ClassroomViewModel>();
builder.Services.AddSignalR();
builder.Services.AddScoped<ICheckNop, CheckNop>();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.LoginPath = "/Identity/Account/Index"; // Đường dẫn đến trang từ chối truy cập mới
});
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddScoped<Mail>();
builder.Services.AddHostedService<EmailSchedulerService>();





ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
			name: "areas",
			pattern: "{area:exists}/{controller=Home}/{action=TrangChu}/{id?}"

		  );
app.MapControllerRoute(
	name: "SaveDataAsync",
	pattern: "/ClassRooms/SaveDataAsync",
	defaults: new { controller = "ClassRooms", action = "SaveDataAsync" }
);

app.MapRazorPages();

app.Run();
