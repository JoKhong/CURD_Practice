using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<PersonsDbContext>(
options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Optional, For Dependency Injection

//Add Auto, ASP.NET auto covers it
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

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
