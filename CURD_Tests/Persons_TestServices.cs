using Entities;
using EntityFrameworkCoreMock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Xunit.Abstractions;

using AutoFixture;
using OfficeOpenXml.Drawing.Chart;
using AutoFixture.Kernel;

using FluentAssertions;

namespace CURD_Tests
{
    public class Persons_TestServices
    {
        private readonly ICountriesService _countryService;
        private readonly IPersonsServices _personService;
        private readonly ITestOutputHelper _testOutputHelper;

        private readonly IFixture _fixture;
        private readonly Bogus.Faker _faker;

        public Persons_TestServices(ITestOutputHelper testOutputHelper)
        {
            _faker = new Bogus.Faker();
            _fixture = new Fixture();   

            var PersonsInitialData = new List<Person>() { };
            var countriesInitialData = new List<Country> { };

            var services = new ServiceCollection();

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>
             (new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContextPersons = dbContextMock.Object;
            dbContextMock.CreateDbSetMock(temp => temp.Persons, PersonsInitialData);

            ApplicationDbContext dbContextCountries = dbContextMock.Object;
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);

            _countryService = new CountryServices(null);
            _personService = new PersonServices(null, _countryService);

            _testOutputHelper = testOutputHelper;

            _fixture.Customize<PersonAddRequest>(composer =>
               composer.With(u => u.Email, _faker.Internet.Email()));
        }

        #region AddPerson

        //When PersonAddRequest is null, should throw NullRequestException
        [Fact]
        public async Task AddPerson_PersonAddRequestNull()
        {
            PersonAddRequest request = null;

            Func<Task> action =  async () =>
            {
                await _personService.AddPerson(request);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();

        }

        //When provided field is null, should throw ArgumentExpection
        [Fact]
        public async Task AddPerson_PersonNameNullOrEmpty()
        {
            PersonAddRequest request = _fixture.Create<PersonAddRequest>();
            request.PersonName = null as string;

            Func<Task> action = async () =>
            {
                await _personService.AddPerson(request);
            };

            await action.Should().ThrowAsync<ArgumentException>();

        }

        //When there are duplicate data, should ArgumentExpection
        [Fact]
        public async Task AddPerson_PersonNameDublicate()
        {
            PersonAddRequest request1 = _fixture.Create<PersonAddRequest>();
            request1.PersonName = "Bob";

            PersonAddRequest request2 = _fixture.Create<PersonAddRequest>();
            request2.PersonName = "Bob";

            Func<Task> action = async () =>
            {
                await _personService.AddPerson(request1);
                await _personService.AddPerson(request2);
            };

            await action.Should().ThrowAsync<ArgumentException>();

        }

        //When add person perperly should suceed
        [Fact]
        public async Task AddPerson_Valid() 
        {
            PersonAddRequest requestParams = _fixture.Create<PersonAddRequest>();
            PersonResponse addedPerson = await _personService.AddPerson(requestParams);

            List<PersonResponse> allPersons = await _personService.GetAllPersons();

            addedPerson.PersonId.Should().NotBe(Guid.Empty);
            allPersons.Should().Contain(addedPerson);
        }

        #endregion

        #region GetAllPersons 
        [Fact]
        public async Task GetAllPersons_Empty()
        {
            List<PersonResponse> result = await _personService.GetAllPersons();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllPersons_AddFew()
        {
            CountryAddRequest addCountry1 = new CountryAddRequest() { CountryName = "USA" };
            CountryResponse responseCounry1 = await _countryService.AddCountry(addCountry1);

            CountryAddRequest addCountry2 = new CountryAddRequest() { CountryName = "Japan" };
            CountryResponse responseCounry2 = await _countryService.AddCountry(addCountry2);

            PersonAddRequest addRequest_1 = _fixture.Create<PersonAddRequest>();
            addRequest_1.CountryId = responseCounry1.CountryId;
            addRequest_1.Country = responseCounry1.CountryName;

            PersonAddRequest addRequest_2 = _fixture.Create<PersonAddRequest>();
            addRequest_2.CountryId = responseCounry2.CountryId;
            addRequest_2.Country = responseCounry2.CountryName;

            PersonAddRequest addRequest_3 = _fixture.Create<PersonAddRequest>();
            addRequest_3.CountryId = responseCounry1.CountryId;
            addRequest_3.Country = responseCounry1.CountryName;

            List<PersonAddRequest> addRequests = new List<PersonAddRequest>();
            addRequests.Add(addRequest_1);
            addRequests.Add(addRequest_2);
            addRequests.Add(addRequest_3);

            List<PersonResponse> addedResponses = new List<PersonResponse>();
            foreach (PersonAddRequest addRequest in addRequests)
            {
                addedResponses.Add(await _personService.AddPerson(addRequest));
            }
            
            List<PersonResponse> requestResponse = await _personService.GetAllPersons();

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in addedResponses)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            _testOutputHelper.WriteLine("Response:");
            foreach (var response in requestResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            //Validate
            foreach (PersonResponse item in addedResponses)
            {
                //Assert.Contains(item, requestResponse);
                requestResponse.Should().Contain(item);
            }
        }

        #endregion

        #region GetFilteredPersons

        //If the search text is empty, return all Persons
        [Fact]
        public async Task GetFilteredPersons_SerchStringEmpty()
        {
            CountryAddRequest addCountry1 = new CountryAddRequest() { CountryName = "USA" };
            CountryResponse responseCounry1 = await _countryService.AddCountry(addCountry1);

            CountryAddRequest addCountry2 = new CountryAddRequest() { CountryName = "Japan" };
            CountryResponse responseCounry2 = await _countryService.AddCountry(addCountry2);

            PersonAddRequest addRequest_1 = _fixture.Create<PersonAddRequest>();
            addRequest_1.CountryId = responseCounry1.CountryId;
            addRequest_1.Country = responseCounry1.CountryName;

            PersonAddRequest addRequest_2 = _fixture.Create<PersonAddRequest>();
            addRequest_2.CountryId = responseCounry2.CountryId;
            addRequest_2.Country = responseCounry2.CountryName;

            PersonAddRequest addRequest_3 = _fixture.Create<PersonAddRequest>();
            addRequest_3.CountryId = responseCounry1.CountryId;
            addRequest_3.Country = responseCounry1.CountryName;

            List<PersonAddRequest> addRequests = new List<PersonAddRequest>();
            addRequests.Add(addRequest_1);
            addRequests.Add(addRequest_2);
            addRequests.Add(addRequest_3);

            List<PersonResponse> addedResponses = new List<PersonResponse>();
            foreach (PersonAddRequest addRequest in addRequests)
            {
                addedResponses.Add(await _personService.AddPerson(addRequest));
            }

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in addedResponses)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            List<PersonResponse> requestResponse = await _personService.GetFilteredPersons(nameof(Person.PersonName), "");

            _testOutputHelper.WriteLine("Response:");
            foreach (var response in requestResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            requestResponse.Should().BeEquivalentTo(addedResponses);
        }

        [Fact]
        public async Task GetFilteredPersons_SearchByName()
        {
            CountryAddRequest addCountry1 = new CountryAddRequest() { CountryName = "Eastania" };
            CountryResponse responseCounry1 = await _countryService.AddCountry(addCountry1);

            CountryAddRequest addCountry2 = new CountryAddRequest() { CountryName = "Westeres" };
            CountryResponse responseCounry2 = await _countryService.AddCountry(addCountry2);

            PersonAddRequest addRequest_1 = _fixture.Create<PersonAddRequest>();
            addRequest_1.PersonName = "Loid Forger";
            addRequest_1.CountryId = responseCounry2.CountryId;
            addRequest_1.Country = responseCounry2.CountryName;

            PersonAddRequest addRequest_2 = _fixture.Create<PersonAddRequest>();
            addRequest_1.PersonName = "Yor Brior";
            addRequest_2.CountryId = responseCounry1.CountryId;
            addRequest_2.Country = responseCounry1.CountryName;

            PersonAddRequest addRequest_3 = _fixture.Create<PersonAddRequest>();
            addRequest_1.PersonName = "Anya Forger";
            addRequest_3.CountryId = responseCounry1.CountryId;
            addRequest_3.Country = responseCounry1.CountryName;

            PersonAddRequest addRequest_4 = _fixture.Create<PersonAddRequest>();
            addRequest_1.PersonName = "Bond";
            addRequest_3.CountryId = responseCounry1.CountryId;
            addRequest_3.Country = responseCounry1.CountryName;

            List<PersonAddRequest> addRequests = new List<PersonAddRequest>();
            addRequests.Add(addRequest_1);
            addRequests.Add(addRequest_2);
            addRequests.Add(addRequest_3);
            addRequests.Add(addRequest_4);

            List<PersonResponse> addedResponses = new List<PersonResponse>();
            foreach (PersonAddRequest addRequest in addRequests)
            {
                addedResponses.Add(await _personService.AddPerson(addRequest));
            }

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in addedResponses)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            string serchStr = "For";

            List<PersonResponse> filteredSearchResponse = await _personService.GetFilteredPersons(nameof(Person.PersonName), serchStr);
           

            _testOutputHelper.WriteLine("Response:");
            foreach (var response in filteredSearchResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            //Assert
            filteredSearchResponse.Should().OnlyContain(temp => temp.PersonName.Contains(serchStr, StringComparison.OrdinalIgnoreCase));

        }

        #endregion

        #region GetSortedPersons

        [Fact]
        public async Task GetFilteredPersons_SortByName()
        {
            CountryAddRequest addCountry1 = new CountryAddRequest() { CountryName = "Eastania" };
            CountryResponse responseCounry1 = await _countryService.AddCountry(addCountry1);

            CountryAddRequest addCountry2 = new CountryAddRequest() { CountryName = "Westeres" };
            CountryResponse responseCounry2 = await _countryService.AddCountry(addCountry2);

            PersonAddRequest addRequest_1 = _fixture.Create<PersonAddRequest>();
            addRequest_1.PersonName = "Loid Forger";
            addRequest_1.CountryId = responseCounry2.CountryId;
            addRequest_1.Country = responseCounry2.CountryName;

            PersonAddRequest addRequest_2 = _fixture.Create<PersonAddRequest>();
            addRequest_1.PersonName = "Yor Brior";
            addRequest_2.CountryId = responseCounry1.CountryId;
            addRequest_2.Country = responseCounry1.CountryName;

            PersonAddRequest addRequest_3 = _fixture.Create<PersonAddRequest>();
            addRequest_1.PersonName = "Anya Forger";
            addRequest_3.CountryId = responseCounry1.CountryId;
            addRequest_3.Country = responseCounry1.CountryName;

            PersonAddRequest addRequest_4 = _fixture.Create<PersonAddRequest>();
            addRequest_1.PersonName = "Bond";
            addRequest_3.CountryId = responseCounry1.CountryId;
            addRequest_3.Country = responseCounry1.CountryName;

            List<PersonAddRequest> addRequests = new List<PersonAddRequest>();
            addRequests.Add(addRequest_1);
            addRequests.Add(addRequest_2);
            addRequests.Add(addRequest_3);
            addRequests.Add(addRequest_4);

            List<PersonResponse> addedResponses = new List<PersonResponse>();
            foreach (PersonAddRequest addRequest in addRequests)
            {
                addedResponses.Add(await _personService.AddPerson(addRequest));
            }

            addedResponses = addedResponses.OrderByDescending(a => a.PersonName).ToList();

            //Get and Run Function
            List<PersonResponse> allPersons = await _personService.GetAllPersons();
            //foreach (PersonResponse personResponse in allPersons)
            //{
            //    personResponse.Country = countryService.GetCountryById(personResponse.CountryId).CountryName;
            //}

            List<PersonResponse> sortedResponse = await _personService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);
           
            _testOutputHelper.WriteLine("Response:");
            foreach (var response in sortedResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            sortedResponse.Should().BeInDescendingOrder(temp => temp.PersonName);

        }


        #endregion

        #region GetPersonById
        [Fact]
        public async Task GetPersonById_ValidId()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = _fixture.Create<PersonAddRequest>();
            PersonResponse response = await _personService.AddPerson(requestParams);

            PersonResponse? personById = await _personService.GetPersonById(response.PersonId);

            personById.Should().Be(response);
        }

        [Fact]
        public async Task GetPersonById_InvalidId()
        {
            PersonResponse? personById = await _personService.GetPersonById(Guid.NewGuid());

            personById.Should().BeNull();
        }
        #endregion

        #region UpdatePerson

        //When PersonUpdateRequest is null, throw ArugmentNull Expection
        [Fact]
        public async Task UpdatePerson_PersonUpdateRequestNull()
        {
            PersonUpdateRequest request = null;

            Func<Task> action = async () =>
            {
                await _personService.UpdatePerson(request);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdatePerson_InvalidId()
        {
            PersonUpdateRequest request = _fixture.Create<PersonUpdateRequest>();
            request.PersonId = Guid.NewGuid();

            Func<Task> action = async () =>
            {
                await _personService.UpdatePerson(request);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdatePerson_InvalidName()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = _fixture.Create<PersonAddRequest>();
            requestParams.CountryId = cuntryAddResponse.CountryId;
            requestParams.Country = cuntryAddResponse.CountryName;

            PersonResponse response = await _personService.AddPerson(requestParams);

            PersonUpdateRequest responseToUpdateRequest = response.ToPersonUpdateRequest();
            responseToUpdateRequest.PersonName = null;

            Func<Task> action = async () => 
            {
                await _personService.UpdatePerson(responseToUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();

        }

        [Fact]
        public async Task UpdatePerson_FullDetails()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = _fixture.Create<PersonAddRequest>();
            requestParams.CountryId = cuntryAddResponse.CountryId;
            requestParams.Country = cuntryAddResponse.CountryName;

            PersonResponse response = await _personService.AddPerson(requestParams);

            PersonUpdateRequest responseToUpdateRequest = response.ToPersonUpdateRequest();
            responseToUpdateRequest.PersonName = "Kenny";
            responseToUpdateRequest.Email = "Update@mail.com";
            PersonResponse updateResponse = await _personService.UpdatePerson(responseToUpdateRequest);

            PersonResponse? responseFromGetById = await _personService.GetPersonById(responseToUpdateRequest.PersonId);

            updateResponse.Should().Be(responseFromGetById);
        }

        #endregion

        #region DeletePerson

        [Fact]
        public async Task DeletePerson_NullParam()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = _fixture.Create<PersonAddRequest>();
            requestParams.CountryId = cuntryAddResponse.CountryId;
            requestParams.Country = cuntryAddResponse.CountryName;

            PersonResponse response = await _personService.AddPerson(requestParams);
            bool deleteSuccess = await _personService.DeletePerson(null);

            deleteSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_Success()
        {
            CountryAddRequest countryAddRequest = _fixture.Create<CountryAddRequest>();
            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = _fixture.Create<PersonAddRequest>();
            requestParams.CountryId = cuntryAddResponse.CountryId;
            requestParams.Country = cuntryAddResponse.CountryName;

            PersonResponse response = await _personService.AddPerson(requestParams);
            bool deleteSuccess = await _personService.DeletePerson(response.PersonId);

            deleteSuccess.Should().BeTrue();
        }

        #endregion

    }
}
