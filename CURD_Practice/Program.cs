using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;

using RepositoryContracts;
using Repositories;

using Serilog;
using Serilog.AspNetCore;
using CURD_Practice.Filters.ActionFilters;

var builder = WebApplication.CreateBuilder(args);

//Serilog
builder.Host.UseSerilog( (HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfig) =>
{
    loggerConfig
    .ReadFrom.Configuration(context.Configuration) //Read config settings from built-in IConfiguration
    .ReadFrom.Services(services);// Read current app services and make them avilable to serilog
});

builder.Services.AddControllersWithViews( options => {

    var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();
    options.Filters.Add(new ResponseHeaderActionFilter(logger, "X-Global-Key", "X-Global-Value" , 0));

});

builder.Services.AddHttpClient();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = 
    Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties 
    | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});

if (builder.Environment.IsEnvironment("Test") == false)
{
    builder.Services.AddDbContext<ApplicationDbContext>
        (options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
}

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

var app = builder.Build();

app.UseSerilogRequestLogging();

if(builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if(builder.Environment.IsEnvironment("Test") == false)
    Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseHttpLogging();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();

public partial class Program { }