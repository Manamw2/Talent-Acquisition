using Microsoft.EntityFrameworkCore;
using DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Models;
using TalentAcquisitionModule.Extentions;
using Microsoft.AspNetCore.Identity.UI.Services;
using TalentAcquisitionModule.Services;
using DataAccess.Repository.IRepository;
using DataAccess.Repository;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<Job>), typeof(Repository<Job>));
builder.Services.AddScoped(typeof(IRepository<JobApplication>), typeof(Repository<JobApplication>));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddRazorPages();
builder.Services.AddSingleton<FileStorageService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new FileStorageService(config);
});

var smtpSettings = builder.Configuration.GetSection("SmtpSettings");

builder.Services.AddTransient(_ => new SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"]))
{
    EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
    UseDefaultCredentials = false,
    Credentials = new NetworkCredential(
        userName: smtpSettings["Username"],
        password: smtpSettings["Password"]
    )
});

// Register EmailSender with dynamically loaded SMTP settings
builder.Services.AddTransient<IEmailSender>(provider =>
{
    var smtpClient = provider.GetRequiredService<SmtpClient>();
    return new EmailSender(
        smtpClient: smtpClient,
        emailAddress: smtpSettings["SenderEmail"],
        signature: smtpSettings["Signature"]
    );
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.ApplyMigrations();
//app.Seed();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
