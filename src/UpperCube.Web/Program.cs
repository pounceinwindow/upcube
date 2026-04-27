using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Serilog;
using UpperCube.Application;
using UpperCube.Infrastructure;

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
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var supportedCultures = new[]
{
    new CultureInfo("ru"),
    new CultureInfo("en")
};

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "areas",
    "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");


app.Run();