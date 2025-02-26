using DataAccess.Data;
using DataAccess.Repository.IRepository;
using DataAccess.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using HrBackOffice.Helper;
using HrBackOffice.Helper.EmailSetting;
using Microsoft.AspNetCore.Identity.UI.Services;
using HrBackOffice.Helper.ApplicantService;
using HrBackOffice.Hubs;
using Hangfire;
using HrBackOffice.Helper.FileProcessingService;

var builder = WebApplication.CreateBuilder(args);

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"),
        new Hangfire.SqlServer.SqlServerStorageOptions
        {
            PrepareSchemaIfNecessary = true, // Ensure schema is created
            EnableHeavyMigrations = true // Ensures database schema migrations run correctly
        }));

// Add the Hangfire server
builder.Services.AddHangfireServer();

// Add SignalR
builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IApplicantService, ApplicantService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<Job>), typeof(Repository<Job>));
builder.Services.AddScoped(typeof(IRepository<JobApplication>), typeof(Repository<JobApplication>));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddSingleton<FileStorageService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new FileStorageService(config);
});

builder.Services.AddScoped<IBackgroundJobClient, BackgroundJobClient>();
builder.Services.AddScoped<PdfProcessingJob>();

// Load SmtpSettings from appsettings.json
var smtpSettings = new SmtpSettings();
builder.Configuration.GetSection("SmtpSettings").Bind(smtpSettings);
builder.Services.AddSingleton(smtpSettings);

// Register EmailSender from EmailSetting project
builder.Services.AddSingleton<IEmailSend, EmailSender>();
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength =  8;

}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization(); 

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Login}/{id?}");

app.MapHub<ProcessingHub>("/processingHub");

app.Run();
