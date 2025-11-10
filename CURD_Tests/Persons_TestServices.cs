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
                PersonAddRequest request = new PersonAddRequest
                {
                    PersonName = null
                };
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
            _fixture.Customize<PersonAddRequest>(composer =>
                composer.With(u => u.Email, _faker.Internet.Email()));

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
            List<PersonResponse> addedResponses = new List<PersonResponse>();

            List<PersonAddRequest> addRequests = new List<PersonAddRequest>() 
            {
                new PersonAddRequest() {PersonName = "Anya", Email = "Anya@Forger.com",  DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Female,
                CountryId = Guid.NewGuid(),
                Address = null,
                ReceiveNewsLetters = false},
                new PersonAddRequest() {PersonName = "Loid", Email = "Loid@Forger.com",  DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Male,
                CountryId = Guid.NewGuid(),
                Address = null,
                ReceiveNewsLetters = false,},
                new PersonAddRequest() {PersonName = "Yor", Email = "Yor@Forger.com",  DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Female,
                CountryId = Guid.NewGuid(),
                Address = null,
                ReceiveNewsLetters = false,},
            };

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
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "Westeris" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "Eastania" };

            CountryResponse countryResponse1 = await _countryService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countryService.AddCountry(countryRequest2);

            _testOutputHelper.WriteLine($"countryResponse1 ID:{countryResponse1.CountryId} and CountryName: {countryResponse1.CountryName}");
            _testOutputHelper.WriteLine($"countryResponse2 ID:{countryResponse2.CountryId} and CountryName: {countryResponse2.CountryName}");

            List<PersonResponse> personAddResponse = new List<PersonResponse>();
            List<PersonAddRequest> personAddRequest = new List<PersonAddRequest>()
            {
                new PersonAddRequest() {PersonName = "Anya", Email = "Anya@Forger.com", Gender = SexOptions.Female, CountryId = countryResponse2.CountryId},
                new PersonAddRequest() {PersonName = "Loid", Email = "Loid@Forger.com", Gender = SexOptions.Male, CountryId = countryResponse1.CountryId},
                new PersonAddRequest() {PersonName = "Yor", Email = "Yor@Forger.com", Gender = SexOptions.Female, CountryId = countryResponse2.CountryId},
                new PersonAddRequest() {PersonName = "Dond", Email = "Bond@Forger.com", Gender = SexOptions.Male, CountryId = countryResponse2.CountryId}
            };
            foreach (PersonAddRequest addRequest in personAddRequest)
            {
                PersonResponse aResponse = await _personService.AddPerson(addRequest);
                //aResponse.Country = countryService.GetCountryById(aResponse.CountryId).CountryName;

                personAddResponse.Add(aResponse);
            }

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in personAddResponse)
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
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "Westeris" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "Eastania" };

            CountryResponse countryResponse1 = await _countryService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countryService.AddCountry(countryRequest2);

            _testOutputHelper.WriteLine($"countryResponse1 ID:{countryResponse1.CountryId} and CountryName: {countryResponse1.CountryName}");
            _testOutputHelper.WriteLine($"countryResponse2 ID:{countryResponse2.CountryId} and CountryName: {countryResponse2.CountryName}");


            List<PersonResponse> personAddResponse = new List<PersonResponse>();
            List<PersonAddRequest> personAddRequest = new List<PersonAddRequest>()
            {
                new PersonAddRequest() {PersonName = "Anya Forger", Email = "Anya@Forger.com", Gender = SexOptions.Female, CountryId = countryResponse2.CountryId},
                new PersonAddRequest() {PersonName = "Loid Forger", Email = "Loid@Forger.com", Gender = SexOptions.Male, CountryId = countryResponse1.CountryId},
                new PersonAddRequest() {PersonName = "Yor Forger", Email = "Yor@Forger.com", Gender = SexOptions.Female, CountryId = countryResponse2.CountryId},
                new PersonAddRequest() {PersonName = "Dond", Email = "Bond@Forger.com", Gender = SexOptions.Male, CountryId = countryResponse2.CountryId}
            };
            foreach (PersonAddRequest addRequest in personAddRequest)
            {
                PersonResponse aResponse = await _personService.AddPerson(addRequest);
                //aResponse.Country = countryService.GetCountryById(aResponse.CountryId).CountryName;

                personAddResponse.Add(aResponse);
            }

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in personAddResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            string searchString = "for";

            List<PersonResponse> filteredSearchResponse = await _personService.GetFilteredPersons(nameof(Person.PersonName), searchString);
            //foreach (PersonResponse addRequest in filteredSearchResponse)
            //{
            //    addRequest.Country = countryService.GetCountryById(addRequest.CountryId).CountryName;
            //}

            _testOutputHelper.WriteLine("Response:");
            foreach (var response in filteredSearchResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            //Assert
            foreach (PersonResponse addedPerson in personAddResponse)
            {
                if(addedPerson.PersonName != null)
                {
                    if (addedPerson.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
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
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "Westeris" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "Eastania" };

            CountryResponse countryResponse1 = await _countryService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countryService.AddCountry(countryRequest2);

            _testOutputHelper.WriteLine($"countryResponse1 ID:{countryResponse1.CountryId} and CountryName: {countryResponse1.CountryName}");
            _testOutputHelper.WriteLine($"countryResponse2 ID:{countryResponse2.CountryId} and CountryName: {countryResponse2.CountryName}");

            //Adding
            List<PersonResponse> personAddResponse = new List<PersonResponse>();
            List<PersonAddRequest> personAddRequest = new List<PersonAddRequest>()
            {
                new PersonAddRequest() {PersonName = "Anya Forger", Email = "Anya@Forger.com", Gender = SexOptions.Female, CountryId = countryResponse2.CountryId},
                new PersonAddRequest() {PersonName = "Loid Forger", Email = "Loid@Forger.com", Gender = SexOptions.Male, CountryId = countryResponse1.CountryId},
                new PersonAddRequest() {PersonName = "Yor Forger", Email = "Yor@Forger.com", Gender = SexOptions.Female, CountryId = countryResponse2.CountryId},
                new PersonAddRequest() {PersonName = "Dond", Email = "Bond@Forger.com", Gender = SexOptions.Male, CountryId = countryResponse2.CountryId}
            };
            foreach (PersonAddRequest addRequest in personAddRequest)
            {
                PersonResponse aResponse = await _personService.AddPerson(addRequest);
                //aResponse.Country = countryService.GetCountryById(aResponse.CountryId).CountryName;

                personAddResponse.Add(aResponse);
            }

            personAddResponse = personAddResponse.OrderByDescending(a => a.PersonName).ToList();

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
            for(int i = 0; i < personAddResponse.Count; i++)
            {
                Assert.Equal(personAddResponse[i], sortedResponse[i]);
            }
        }


        #endregion

        #region GetPersonById
        [Fact]
        public async Task GetPersonById_ValidId()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = new PersonAddRequest()
            {
                PersonName = "John",
                Email = "example@mail.com",
                DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Male,
                CountryId = cuntryAddResponse.CountryId,
                Address = null,
                ReceiveNewsLetters = false,
            };

            PersonResponse response = await _personService.AddPerson(requestParams);

            PersonResponse? personById = await _personService.GetPersonById(response.PersonId);

            Assert.Equal(personById, response);
        }

        [Fact]
        public async Task GetPersonById_InvalidId()
        {
            PersonAddRequest requestParams = new PersonAddRequest()
            {
                PersonName = "John",
                Email = "example@mail.com",
                DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Male,
                CountryId = Guid.NewGuid(),
                Address = "TestAddRess",
                ReceiveNewsLetters = true,
            };

            PersonResponse response = await _personService.AddPerson(requestParams);
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
                PersonUpdateRequest request = new PersonUpdateRequest()
                {
                    PersonId = Guid.NewGuid()
                };

                await _personService.UpdatePerson(request);
            });
        }

        public async Task UpdatePerson_InvalidName()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = new PersonAddRequest()
            {
                PersonName = "John",
                Email = "example@mail.com",
                DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Male,
                CountryId = cuntryAddResponse.CountryId,
                Address = null,
                ReceiveNewsLetters = false,
            };

            PersonResponse response = await _personService.AddPerson(requestParams);

            PersonUpdateRequest responseToUpdateRequest = response.ToPersonUpdateRequest();
            responseToUpdateRequest.PersonName = null;

            await Assert.ThrowsAsync<ArgumentException>( async() =>
            {
                await _personService.UpdatePerson(responseToUpdateRequest);
            });
        }

        public async Task UpdatePerson_FullDetails()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = new PersonAddRequest()
            {
                PersonName = "John",
                Email = "example@mail.com",
                DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Male,
                CountryId = cuntryAddResponse.CountryId,
                Address = null,
                ReceiveNewsLetters = false,
            };

            PersonResponse response = await _personService.AddPerson(requestParams);

            PersonUpdateRequest responseToUpdateRequest = response.ToPersonUpdateRequest();
            responseToUpdateRequest.PersonName = "Kenny";
            responseToUpdateRequest.Email = "Update@mail.com";
            PersonResponse updateResponse = await _personService.UpdatePerson(responseToUpdateRequest);

            PersonResponse responseFromGetById = await _personService.GetPersonById(responseToUpdateRequest.PersonId);

            Assert.Equal(updateResponse, responseFromGetById);

        }

        #endregion

        #region DeletePerson

        [Fact]
        public async Task DeletePerson_NullParam()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = new PersonAddRequest()
            {
                PersonName = "John",
                Email = "example@mail.com",
                DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Male,
                CountryId = cuntryAddResponse.CountryId,
                Address = null,
                ReceiveNewsLetters = false,
            };

            PersonResponse response = await _personService.AddPerson(requestParams);
            bool deleteSuccess = await _personService.DeletePerson(null);

            Assert.False(deleteSuccess);
        }

        [Fact]
        public async Task Delete_Success()
        {
            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = await _countryService.AddCountry(countryAddRequest);

            PersonAddRequest requestParams = new PersonAddRequest()
            {
                PersonName = "John",
                Email = "example@mail.com",
                DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Male,
                CountryId = cuntryAddResponse.CountryId,
                Address = null,
                ReceiveNewsLetters = false,
            };

            PersonResponse response = await _personService.AddPerson(requestParams);
            bool deleteSuccess = await _personService.DeletePerson(response.PersonId);

            Assert.True(deleteSuccess);
        }

        #endregion

    }
}
