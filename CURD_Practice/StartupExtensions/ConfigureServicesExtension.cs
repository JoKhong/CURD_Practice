using CURD_Practice.Filters.ActionFilters;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CURD_Practice
{
    public static class ConfigureServicesExtension
    {
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment enviroment)
        {
            services.AddTransient<ResponseHeaderActionFilter>();
            services.AddControllersWithViews(options => {

                var logger = services.BuildServiceProvider().GetRequiredService<ILogger<ResponseHeaderActionFilter>>();

                options.Filters.Add(new ResponseHeaderActionFilter(logger)
                {
                    Key = "X-Global-Key",
                    Value = "X-Global-Value",
                    Order = 0
                }); // Add in Global Filter

            });

            services.AddHttpClient();

            services.AddHttpLogging(options =>
            {
                options.LoggingFields =
                Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestProperties
                | Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
            });

            if (enviroment.IsEnvironment("Test") == false)
            {
                services.AddDbContext<ApplicationDbContext>
                    (options =>
                    {
                        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                    });
            }

            //Add Auto, ASP.NET covers parameters when add as service
            services.AddScoped<ICountriesRepository, CountriesRepositories>();
            services.AddScoped<IPersonsRepository, PersonsRepositories>();

            services.AddScoped<ICountriesService, CountryServices>();
            services.AddScoped<ICountryAdderService, CountryAdderService>();
            services.AddScoped<ICountryGetAllCountriesService, CountryGetAllCountriesService>();
            services.AddScoped<ICountryGetByIdService, CountryGetByIdService>();
            services.AddScoped<ICountryUploadFromExcelService, CountryUploadFromExcelService>();

            services.AddScoped<IPersonsServices, PersonServices>();

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

            return services;

        }
    }
}
