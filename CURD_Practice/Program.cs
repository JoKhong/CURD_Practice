using Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using Services;

using RepositoryContracts;
using Repositories;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<ApplicationDbContext>(
options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Optional, For Dependency Injection

//Add Auto, ASP.NET covers parameters when add as service
builder.Services.AddScoped<ICountriesRepository, CountriesRepositories>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepositories>();

builder.Services.AddScoped<ICountriesService, CountryServices>();
builder.Services.AddScoped<IPersonsServices, PersonServices>();

//Add Manual, Useful when constructor has other parameters or want control. 
//BUT NOT RECOMMENDED
#region Manual Add Service
/*
builder.Services.AddScoped<ICountriesService>(
provider =>
{
    PersonsDbContext? dbContext = provider.GetService<PersonsDbContext>();
    return new CountryServices(dbContext);
});
builder.Services.AddScoped<IPersonsServices>(
provider =>
{
    PersonsDbContext? dbContext = provider.GetService<PersonsDbContext>();
    CountryServices countryServices = provider.GetService<CountryServices>();

    return new PersonServices(dbContext, countryServices);
});
*/
#endregion


//Optional, For Options
//builder.Services.Configure<TradingOptions>(builder.Configuration.GetSection(nameof(TradingOptions)));

var app = builder.Build();

if(builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
