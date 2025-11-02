using Microsoft.Extensions.DependencyInjection;
using ServiceContracts.DTO;
using ServiceContracts;
using Services;
using FluentAssertions;

namespace CURD_Tests
{
    public class Countries_TestServices
    {
        private readonly ServiceProvider _provider;

        public Countries_TestServices()
        {
            var services = new ServiceCollection();
            services.AddScoped<ICountriesService, CountryServices>();

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void AddCountry_NullAddRequest()
        {
            var countryService = _provider.GetService<ICountriesService>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                CountryAddRequest request = null;
                countryService.AddCountry(request);
            });
        }

        [Fact]
        public void AddCountry_NullCountry()
        {
            var countryService = _provider.GetService<ICountriesService>();

            Assert.Throws<ArgumentException>(() =>
            {
                CountryAddRequest request = new CountryAddRequest
                {
                    CountryName = null
                };

                countryService.AddCountry(request);
            });
        }

        [Fact]
        public void AddCountry_DuplicateCountry()
        {
            var countryService = _provider.GetService<ICountriesService>();

            Assert.Throws<ArgumentException>(() =>
            {
                countryService.AddCountry(new CountryAddRequest()
                {
                    CountryName = "Germany"
                });

                countryService.AddCountry(new CountryAddRequest()
                {
                    CountryName = "Germany"
                });
            });

        }

        [Fact]
        public void AddCountry_AddSuccess()
        {
            var countryService = _provider.GetService<ICountriesService>();

            CountryResponse result = countryService.AddCountry(new CountryAddRequest()
            {
                CountryName = "Japan"
            });

            Assert.True(result.CountryId != Guid.Empty);
        }

    }
}