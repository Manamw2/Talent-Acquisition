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
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
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


// Register SmtpClient as a transient service
builder.Services.AddTransient(_ => new SmtpClient("smtp.gmail.com", 587)
{
    EnableSsl = true,
    UseDefaultCredentials = false,
    Credentials = new NetworkCredential(
        userName: "mahmoud.amr.nabil23@gmail.com",
        password: "aybe vgmx zzqz ypgt"
    )
});
// Register SmtpEmailSender as a transient service
builder.Services.AddTransient<IEmailSender>(provider =>
{
    var smtpClient = provider.GetRequiredService<SmtpClient>();
    return new EmailSender(
        smtpClient: smtpClient,
        emailAddress: "mahmoud.amr.nabil23@gmail.com",
        signature: "<br/><div style='font-family: Arial,sans-serif;font-size: 10.0pt;'><p>Thank you and best regards,</p><p><strong>SoftTrend Marketplace</strong></p><p>    <strong>T</strong> +20 (2) 21 26 7000 | <strong>F</strong> +20 (2) 21 26 7026 <br />    <strong>SoftTrend Headquarter</strong> | 37 H/1 Shokry Abdel Halim Street, Takseem Elaselky, Maadi, Cairo, Egypt</p><div>    <img src='https://soft-trend.com/EmailSignature/22-11-2020/images/Logo.png' /><a style='margin-right:5px;' target='_blank' href='https://www.facebook.com/SoftTrend-440957282629179/?ref=ts'><img src='https://soft-trend.com/EmailSignature/22-11-2020/images/FacebookIcon.png' /></a></td><a style='margin-right:5px;' target='_blank' href='https://www.instagram.com/soft_trend/'><img src='https://soft-trend.com/EmailSignature/22-11-2020/images/InstagramIcon.png' /></a></td><a style='margin-right:5px;' target='_blank' href='https://www.linkedin.com/company/softtrend/'><img src='https://soft-trend.com/EmailSignature/22-11-2020/images/LinkedinIcon.png' /></a></td><a style='margin-right:5px;' target='_blank' href='https://twitter.com/Soft_Trend'><img src='https://soft-trend.com/EmailSignature/22-11-2020/images/TwitterIcon.png' /></a></td><a style='margin-right:5px;' target='_blank' href='https://marketplace.soft-trend.com/'><img src='https://soft-trend.com/EmailSignature/22-11-2020/images/MarketplaceIcon.png' /></a></td><a style='margin-right:5px;' target='_blank' href='https://marketplacedemo.soft-trend.com/'><img src='https://soft-trend.com/EmailSignature/22-11-2020/images/MarketplaceDemoIcon.png' /></a></td></div></div>"
    );
});


builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();

}
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
