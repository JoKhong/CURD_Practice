using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

using System.Threading.Tasks;

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
        public async Task AddCountry_NullAddRequest()
        {
            var countryService = _provider.GetService<ICountriesService>();

            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                CountryAddRequest request = null;
                await countryService.AddCountry(request);
            });
        }

        [Fact]
        public async Task AddCountry_NullCountry()
        {
            var countryService = _provider.GetService<ICountriesService>();

            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                CountryAddRequest request = new CountryAddRequest
                {
                    CountryName = null
                };

                await countryService.AddCountry(request);
            });
        }

        [Fact]
        public async Task AddCountry_DuplicateCountry()
        {
            var countryService = _provider.GetService<ICountriesService>();

            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                await countryService.AddCountry(new CountryAddRequest()
                {
                    CountryName = "Germany"
                });

                await countryService.AddCountry(new CountryAddRequest()
                {
                    CountryName = "Germany"
                });
            });

        }

        [Fact]
        public async Task AddCountry_AddSuccess()
        {
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest request = new CountryAddRequest
            {
                CountryName = "Japan"
            };

            CountryResponse addedCountry = await countryService.AddCountry(request);
            List<CountryResponse> countriesList = await countryService.GetAllCountries();

            Assert.True(addedCountry.CountryId != Guid.Empty);
            Assert.Contains(addedCountry, countriesList);
        }
        #endregion

        #region GetAllCountries

        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            var countryService = _provider.GetService<ICountriesService>();

            List<CountryResponse> result = await countryService.GetAllCountries();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
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
                addedCountryResponses.Add(await countryService.AddCountry(country));
            }

            List<CountryResponse> getAllCountriesResult = await countryService.GetAllCountries();

            foreach (CountryResponse countryResponse in addedCountryResponses)
            {
                Assert.Contains(countryResponse, getAllCountriesResult);
            }
        }

        #endregion

        #region GetCountryById

        [Fact]
        public async Task GetCountryById_ValidId()
        {
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest request = new  CountryAddRequest() { CountryName = "Germany" };

            CountryResponse response = await countryService.AddCountry(request);
            CountryResponse? countryById = await countryService.GetCountryById(response.CountryId);

            Assert.Equal(countryById, response);
        }

        [Fact]
        public async Task GetCountryById_InvalidId()
        {
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest request = new CountryAddRequest() { CountryName = "Japan" };

            CountryResponse response = await countryService.AddCountry(request);
            CountryResponse? countryById = await countryService.GetCountryById(Guid.NewGuid());

            Assert.Null(countryById);
        }

        #endregion

    }
}