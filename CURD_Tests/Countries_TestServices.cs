using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace CURD_Tests
{
    public class Countries_TestServices
    {
        private readonly ServiceProvider _provider;

        public Countries_TestServices()
        {
            string _sqlServer = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=PersonsDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

            var services = new ServiceCollection();

            services.AddDbContext<PersonsDbContext>(
            options =>
            {
                options.UseSqlServer(_sqlServer);
            });

            services.AddScoped<ICountriesService>(provider =>
            {
                PersonsDbContext? dbContext = provider.GetService<PersonsDbContext>();
                return new CountryServices(dbContext);
            });

            _provider = services.BuildServiceProvider();
        }

        #region AddCountry 
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

            CountryAddRequest request = new CountryAddRequest
            {
                CountryName = "Japan"
            };

            CountryResponse addedCountry = countryService.AddCountry(request);
            List<CountryResponse> countriesList = countryService.GetAllCountries();

            Assert.True(addedCountry.CountryId != Guid.Empty);
            Assert.Contains(addedCountry, countriesList);
        }
        #endregion

        #region GetAllCountries

        [Fact]
        public void GetAllCountries_EmptyList()
        {
            var countryService = _provider.GetService<ICountriesService>();

            List<CountryResponse> result = countryService.GetAllCountries();

            Assert.Empty(result);
        }

        [Fact]
        public void GetAllCountries_AddFewCountries()
        {
            var countryService = _provider.GetService<ICountriesService>();

            List<CountryResponse> addedCountryResponses = new List<CountryResponse>();

            //AddCountries First
            List<CountryAddRequest> addCountries = new List<CountryAddRequest>() 
            {
                new CountryAddRequest(){ CountryName = "Germany"},
                new CountryAddRequest(){ CountryName = "Japan"},
                new CountryAddRequest(){ CountryName = "Malaysia"}
            };

            foreach (CountryAddRequest country in addCountries) 
            {
                addedCountryResponses.Add(countryService.AddCountry(country));
            }

            List<CountryResponse> getAllCountriesResult = countryService.GetAllCountries();

            foreach (CountryResponse countryResponse in addedCountryResponses)
            {
                Assert.Contains(countryResponse, getAllCountriesResult);
            }
        }

        #endregion

        #region GetCountryById

        [Fact]
        public void GetCountryById_ValidId()
        {
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest request = new  CountryAddRequest() { CountryName = "Germany" };

            CountryResponse response = countryService.AddCountry(request);
            CountryResponse? countryById = countryService.GetCountryById(response.CountryId);

            Assert.Equal(countryById, response);
        }

        [Fact]
        public void GetCountryById_InvalidId()
        {
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest request = new CountryAddRequest() { CountryName = "Japan" };

            CountryResponse response = countryService.AddCountry(request);
            CountryResponse? countryById = countryService.GetCountryById(Guid.NewGuid());

            Assert.Null(countryById);
        }

        #endregion

    }
}