using Invoice_Generator;
using Invoice_Generator.AppSettings;
using Invoice_Generator.Data;
using Invoice_Generator.Infrastructures.Emails;
using Invoice_Generator.Infrastructures.Pdfs;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using WkHtmlToPdfDotNet.Contracts;
using WkHtmlToPdfDotNet;
using Serilog.Events;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    var identitySettings = builder.Configuration.GetSection(IdentitySettings.IdentitySettingsName).Get<IdentitySettings>();
    if (identitySettings != null)
    {
        options.SignIn.RequireConfirmedAccount = identitySettings.RequireConfirmedAccount;
        options.Lockout.DefaultLockoutTimeSpan = identitySettings.DefaultLockoutTimeSpan;
        options.Lockout.MaxFailedAccessAttempts = identitySettings.MaxFailedAccessAttempts;
    }
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI()
    .AddRoles<IdentityRole>()
    .AddDefaultTokenProviders();

// Register the SMTP email service
builder.Services.AddTransient<IEmailSender, SMTPEmailService>();

// Configure SMTP settings
builder.Services.Configure<SmtpConfiguration>(builder.Configuration.GetSection("SmtpConfiguration"));
builder.Services.Configure<RegistrationConfiguration>(builder.Configuration.GetSection("RegistrationConfiguration"));
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationConfiguration"));
builder.Services.AddRazorPages();

builder.Services.AddAllCustomServices();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
builder.Services.AddSingleton<IPdfService, PdfService>();

builder.Services.AddSession();
builder.Services.AddResponseCompression();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true }
    )
    .CreateLogger();

builder.Host.UseSerilog();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseResponseCompression();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
