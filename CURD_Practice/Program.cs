using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

//Optional, For Dependency Injection
builder.Services.AddSingleton<ICountriesService>( 
provider =>
{
    return new CountryServices(true);
});

builder.Services.AddSingleton<IPersonsServices>( 
provider =>
{
    return new PersonServices(true);
});

builder.Services.AddDbContext<PersonsDbContext>(
options => 
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); 
});

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
