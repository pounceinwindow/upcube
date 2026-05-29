using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UpperCube.Application;
using UpperCube.Infrastructure;
using UpperCube.Infrastructure.Persistence;
using UpperCube.Infrastructure.Seeding;
using UpperCube.Web;
using UpperCube.Web.Hubs;
using UpperCube.Web.Middleware;
using UpperCube.Web.Services.Inquiries;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddScoped<IInquiryChatService, InquiryChatService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource));
    });

builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

var supportedCultures = new[]
{
    new CultureInfo("ru"),
    new CultureInfo("en")
};

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

await DataSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error/500");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ru"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRouting();
app.UseStatusCodePagesWithReExecute("/error/{0}");
app.UseSerilogRequestLogging();
app.UseMiddleware<ErrorLoggingMiddleware>();

app.UseAuthentication();
app.UseMiddleware<AdminAccessMiddleware>();
app.UseMiddleware<AdminAuditMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    "areas",
    "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.MapHub<InquiryHub>("/hubs/inquiries");

app.Run();