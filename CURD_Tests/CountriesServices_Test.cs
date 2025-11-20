using AutoFixture;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CURD_Tests
{
    public class CountriesServices_Test
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countriesRepoMock;

        private readonly ICountryAdderService _countryAdderService;
        private readonly ICountryGetterServices _countryGetterServices;
        private readonly ICountryUploadFromExcelService _countryUploadFromExcelService;

        private readonly ITestOutputHelper _testOutputHelper;

        private readonly IFixture _fixture;

        public CountriesServices_Test(ITestOutputHelper testOutputHelper)
        {
            var countriesInitialData = new List<Country> { };

            var services = new ServiceCollection();

            _fixture = new Fixture();

            _countriesRepoMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepoMock.Object;

            //CountryAdder 
            _countryAdderService = new CountryAdderService(_countriesRepository);
            _countryGetterServices = new CountryGetterServices(_countriesRepository);
            _countryUploadFromExcelService = new CountryUploadFromExcelService(_countriesRepository);

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>
              (new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContext = dbContextMock.Object;
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);



            _testOutputHelper = testOutputHelper;
        }

        #region AddCountry 
        [Fact]
        public async Task AddCountry_AddRequestNull_ArgumentNullException()
        {
            CountryAddRequest request = null;

            Country country = _fixture.Build<Country>()
              .With(temp => temp.Persons, null as ICollection<Person>)
              .Create();

            _countriesRepoMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);

            var action = async () => { await _countryAdderService.AddCountry(request); };

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddCountry_CountryNameNull_ArgumentException()
        {
            CountryAddRequest request = new CountryAddRequest
            {
                CountryName = null as string
            };

            Country country = _fixture.Build<Country>()
             .With(temp => temp.Persons, null as ICollection<Person>)
             .Create();

            _countriesRepoMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);

            var action = async () =>
            {
                await _countryAdderService.AddCountry(request);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddCountry_DuplicateCountry()
        {
            CountryAddRequest countryAddRequest1 = new CountryAddRequest { CountryName = "Test" };
            CountryAddRequest countryAddRequest2 = new CountryAddRequest { CountryName = "Test" };

            Country country1 = countryAddRequest1.ToCountry();
            Country country2 = countryAddRequest2.ToCountry();

            _countriesRepoMock.Setup( temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country1);
            _countriesRepoMock.Setup( temp => temp.GetCountryByName(It.IsAny<string>())).ReturnsAsync(null as Country);

            CountryResponse firstResponse = await _countryAdderService.AddCountry(countryAddRequest1);

            //Act
            var action = async () =>
            {
                //Return first country when GetCountryByCountryName is called
                _countriesRepoMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country1);
                _countriesRepoMock.Setup(temp => temp.GetCountryByName(It.IsAny<string>())).ReturnsAsync(country1);

                await _countryAdderService.AddCountry(countryAddRequest2);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();

        }

        [Fact]
        public async Task AddCountry_AddSuccess()
        {
            CountryAddRequest countryAddRequest1 = new CountryAddRequest { CountryName = "Japan" };
            Country country1 = countryAddRequest1.ToCountry();
            CountryResponse countryResponse = country1.ToCountryResponse();

            _countriesRepoMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country1);
            _countriesRepoMock.Setup(temp => temp.GetCountryByName(It.IsAny<string>())).ReturnsAsync(null as Country);

            CountryResponse addedCountry = await _countryAdderService.AddCountry(countryAddRequest1);
            country1.CountryId = addedCountry.CountryId;
            countryResponse.CountryId = addedCountry.CountryId;

            addedCountry.CountryId.Should().NotBe(Guid.Empty);
            addedCountry.Should().BeEquivalentTo(countryResponse);

        }
        #endregion

        #region GetAllCountries

        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            List<Country> countries = new List<Country>();

            _countriesRepoMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countries);

            List<CountryResponse> result = await _countryGetterServices.GetCountriesAll();

            result.Should().BeEmpty();

        }

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            //AddCountries First
            List<CountryAddRequest> countryAddRequests = new List<CountryAddRequest>() 
            {
                new CountryAddRequest(){ CountryName = "Germany"},
                new CountryAddRequest(){ CountryName = "Japan"},
                new CountryAddRequest(){ CountryName = "Malaysia"}
            };

            List<Country> countries = countryAddRequests.Select( x => x.ToCountry() ).ToList();
            List<CountryResponse> mockResponse = countries.Select(x => x.ToCountryResponse()).ToList();

            _countriesRepoMock.Setup(x => x.GetAllCountries()).ReturnsAsync(countries);
            
            List<CountryResponse> actualResponse = await _countryGetterServices.GetCountriesAll();

            actualResponse.Should().BeEquivalentTo(mockResponse);
        }

        #endregion

        #region GetCountryById

        [Fact]
        public async Task GetCountryById_ValidId()
        {
            Country mockCountry = _fixture.Build<Country>()
              .With(temp => temp.Persons, null as ICollection<Person>)
              .Create();

            CountryResponse mockResponse = mockCountry.ToCountryResponse();

            _countriesRepoMock.Setup(x => x.GetCountryById(It.IsAny<Guid>())).ReturnsAsync(mockCountry);

          
            CountryResponse? countryById = await _countryGetterServices.GetCountryById(mockCountry.CountryId);

            countryById.Should().Be(mockResponse);

        }

        [Fact]
        public async Task GetCountryById_InvalidId()
        {
            Guid? countryGuid = null;

            _countriesRepoMock.Setup(x => x.GetCountryById(It.IsAny<Guid>())).ReturnsAsync(null as Country);

            CountryResponse? countryById = await _countryGetterServices.GetCountryById(countryGuid);

            countryById.Should().BeNull();
        }

        #endregion

    }
}