using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CURD_Tests
{
    public class Countries_TestServices
    {
        private readonly ICountriesService _countryService;
        private readonly ITestOutputHelper _testOutputHelper;

        public Countries_TestServices(ITestOutputHelper testOutputHelper)
        {
            var countriesInitialData = new List<Country> { };

            var services = new ServiceCollection();

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>
              (new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContext = dbContextMock.Object;
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

            _countryService = new CountryServices(null);
            _testOutputHelper = testOutputHelper;
        }

        #region AddCountry 
        [Fact]
        public async Task AddCountry_NullAddRequest()
        {
            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                CountryAddRequest request = null;
                await _countryService.AddCountry(request);
            });
        }

        [Fact]
        public async Task AddCountry_NullCountry()
        {
            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                CountryAddRequest request = new CountryAddRequest
                {
                    CountryName = null
                };

                await _countryService.AddCountry(request);
            });
        }

        [Fact]
        public async Task AddCountry_DuplicateCountry()
        {
            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                await _countryService.AddCountry(new CountryAddRequest()
                {
                    CountryName = "Germany"
                });

                await _countryService.AddCountry(new CountryAddRequest()
                {
                    CountryName = "Germany"
                });
            });

        }

        [Fact]
        public async Task AddCountry_AddSuccess()
        {
            CountryAddRequest request = new CountryAddRequest
            {
                CountryName = "Japan"
            };

            CountryResponse addedCountry = await _countryService.AddCountry(request);
            List<CountryResponse> countriesList = await _countryService.GetAllCountries();

            Assert.True(addedCountry.CountryId != Guid.Empty);
            Assert.Contains(addedCountry, countriesList);
        }
        #endregion

        #region GetAllCountries

        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            List<CountryResponse> result = await _countryService.GetAllCountries();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
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
                addedCountryResponses.Add(await _countryService.AddCountry(country));
            }

            List<CountryResponse> getAllCountriesResult = await _countryService.GetAllCountries();

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
            CountryAddRequest request = new  CountryAddRequest() { CountryName = "Germany" };

            CountryResponse response = await _countryService.AddCountry(request);
            CountryResponse? countryById = await _countryService.GetCountryById(response.CountryId);

            Assert.Equal(countryById, response);
        }

        [Fact]
        public async Task GetCountryById_InvalidId()
        {
            Guid? countryGuid = null;
            CountryResponse? countryById = await _countryService.GetCountryById(countryGuid);

            Assert.Null(countryById);
        }

        #endregion

    }
}