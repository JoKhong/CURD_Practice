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

            _countryService = new CountryServices(dbContextCountries);
            _personService = new PersonServices(dbContextPersons, _countryService);

            _testOutputHelper = testOutputHelper;

            _fixture.Customize<PersonAddRequest>(composer =>
               composer.With(u => u.Email, _faker.Internet.Email()));
        }

        #region AddPerson

        //When PersonAddRequest is null, should throw NullRequestException
        [Fact]
        public async Task AddPerson_PersonAddRequestNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                PersonAddRequest request = null;
                await _personService.AddPerson(request);
            });
        }

        //When provided field is null, should throw ArgumentExpection
        [Fact]
        public async Task AddPerson_PersonNameNullOrEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                PersonAddRequest test = _fixture.Create<PersonAddRequest>();

                PersonAddRequest request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, _faker.Internet.Email() )
                .Create();

                await _personService.AddPerson(request);
            });
        }

        //When there are duplicate data, should ArgumentExpection
        [Fact]
        public async Task AddPerson_PersonNameDublicate()
        {
            await Assert.ThrowsAsync<ArgumentException>(async() =>
            {
                PersonAddRequest request1 = new PersonAddRequest
                {
                    PersonName = "Bob"
                };

                PersonAddRequest request2 = new PersonAddRequest
                {
                    PersonName = "Bob"
                };

                await _personService.AddPerson(request1);
                await _personService.AddPerson(request2);

            });

        }

        //When add person perperly should suceed
        [Fact]
        public async Task AddPerson_Valid() 
        {
            PersonAddRequest requestParams = _fixture.Create<PersonAddRequest>();

            PersonResponse addedPerson = await _personService.AddPerson(requestParams);

            List<PersonResponse> allPersons = await _personService.GetAllPersons();

            Assert.True(addedPerson.PersonId != Guid.Empty);
            Assert.Contains(addedPerson, allPersons);
        }

        #endregion

        #region GetAllPersons 
        [Fact]
        public async Task GetAllPersons_Empty()
        {
            List<PersonResponse> result = await _personService.GetAllPersons();

            Assert.Empty(result);
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
                Assert.Contains(item, requestResponse);
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

            //Assert
            foreach (PersonResponse item in requestResponse)
            {
                Assert.Contains(item, requestResponse);
            }
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
            foreach (PersonResponse addedPerson in addedResponses)
            {
                if(addedPerson.PersonName != null)
                {
                    if (addedPerson.PersonName.Contains(serchStr, StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(addedPerson, filteredSearchResponse);
                    }
                }
            }
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

            //Assert
            for(int i = 0; i < addedResponses.Count; i++)
            {
                Assert.Equal(addedResponses[i], sortedResponse[i]);
            }
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

            Assert.Equal(personById, response);
        }

        [Fact]
        public async Task GetPersonById_InvalidId()
        {
            PersonResponse? personById = await _personService.GetPersonById(Guid.NewGuid());

            Assert.Null(personById);
        }
        #endregion

        #region UpdatePerson

        //When PersonUpdateRequest is null, throw ArugmentNull Expection
        [Fact]
        public async Task UpdatePerson_PersonUpdateRequestNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                PersonUpdateRequest request = null;
                await _personService.UpdatePerson(request);
            });
        }

        [Fact]
        public async Task UpdatePerson_InvalidId()
        {
            await Assert.ThrowsAsync<ArgumentException>( async () =>
            {
                PersonUpdateRequest request = _fixture.Create<PersonUpdateRequest>();
                request.PersonId = Guid.NewGuid();  

                await _personService.UpdatePerson(request);
            });
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

            await Assert.ThrowsAsync<ArgumentException>( async() =>
            {
                await _personService.UpdatePerson(responseToUpdateRequest);
            });
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

            Assert.Equal(updateResponse, responseFromGetById);

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

            Assert.False(deleteSuccess);
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

            Assert.True(deleteSuccess);
        }

        #endregion

    }
}
