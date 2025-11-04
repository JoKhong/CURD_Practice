using Entities;
using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Reflection.Metadata.Ecma335;
using Xunit.Abstractions;

namespace CURD_Tests
{
    public class Persons_TestServices
    {
        private readonly ServiceProvider _provider;
        private readonly ITestOutputHelper _testOutputHelper;

        public Persons_TestServices(ITestOutputHelper testOutputHelper)
        {
            var services = new ServiceCollection();
            services.AddScoped<IPersonsServices>( provider => 
            {
                return new PersonServices(false);
            });
            
            services.AddScoped<ICountriesService>( provider => 
            {
                return new CountryServices(false);
            });

            _testOutputHelper = testOutputHelper;

            _provider = services.BuildServiceProvider();
        }

        #region AddPerson

        //When PersonAddRequest is null, should throw NullRequestException
        [Fact]
        public void AddPerson_PersonAddRequestNull()
        {
            var personServices = _provider.GetService<IPersonsServices>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                PersonAddRequest request = null;
                personServices.AddPerson(request);
            });
        }

        //When provided field is null, should throw ArgumentExpection
        [Fact]
        public void AddPerson_PersonNameNullOrEmpty()
        {
            var personServices = _provider.GetService<IPersonsServices>();

            Assert.Throws<ArgumentException>(() =>
            {
                PersonAddRequest request = new PersonAddRequest
                {
                    PersonName = null
                };
                personServices.AddPerson(request);
            });
        }

        //When there are duplicate data, should ArgumentExpection
        [Fact]
        public void AddPerson_PersonNameDublicate()
        {
            var personServices = _provider.GetService<IPersonsServices>();

            Assert.Throws<ArgumentException>(() =>
            {
                PersonAddRequest request1 = new PersonAddRequest
                {
                    PersonName = "Bob"
                };

                PersonAddRequest request2 = new PersonAddRequest
                {
                    PersonName = "Bob"
                };

                personServices.AddPerson(request1);
                personServices.AddPerson(request2);

            });

        }

        //When add person perperly should suceed
        [Fact]
        public void AddPerson_Valid() 
        {
            var personServices = _provider.GetService<IPersonsServices>();

            PersonAddRequest requestParams = new PersonAddRequest()
            {
                PersonName = "John",
                Email = "example@mail.com",
                DateOfBirth = new DateTime(1995, 01, 01),
                Gender = SexOptions.Male,
                CountryId = Guid.NewGuid(),
                Address = "TestAdd",
                ReceiveNewsLetters = false,
            };

            PersonResponse addedPerson = personServices.AddPerson(requestParams);

            List<PersonResponse> allPersons = personServices.GetAllPersons();

            Assert.True(addedPerson.PersonId != Guid.Empty);
            Assert.Contains(addedPerson, allPersons);
        }

        #endregion

        #region GetAllPersons 
        [Fact]
        public void  GetAllPersons_Empty()
        {
            var personsServices = _provider.GetService<IPersonsServices>();

            List<PersonResponse> result = personsServices.GetAllPersons();

            Assert.Empty(result);
        }

        [Fact]
        public void GetAllPersons_AddFew()
        {
            var personsServices = _provider.GetService<IPersonsServices>();

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
                addedResponses.Add(personsServices.AddPerson(addRequest));
            }
            
            List<PersonResponse> requestResponse = personsServices.GetAllPersons();

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
        public void GetFilteredPersons_SerchStringEmpty()
        {
            var personsServices = _provider.GetService<IPersonsServices>();
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "Westeris" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "Eastania" };

            CountryResponse countryResponse1 = countryService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = countryService.AddCountry(countryRequest2);

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
                PersonResponse aResponse = personsServices.AddPerson(addRequest);
                aResponse.Country = countryService.GetCountryById(aResponse.CountryId).CountryName;

                personAddResponse.Add(aResponse);
            }

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in personAddResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            List<PersonResponse> requestResponse = personsServices.GetFilteredPersons(nameof(Person.PersonName), "");

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
        public void GetFilteredPersons_SearchByName()
        {
            var personsServices = _provider.GetService<IPersonsServices>();
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "Westeris" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "Eastania" };

            CountryResponse countryResponse1 = countryService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = countryService.AddCountry(countryRequest2);

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
                PersonResponse aResponse = personsServices.AddPerson(addRequest);
                aResponse.Country = countryService.GetCountryById(aResponse.CountryId).CountryName;

                personAddResponse.Add(aResponse);
            }

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in personAddResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            string searchString = "for";

            List<PersonResponse> filteredSearchResponse = personsServices.GetFilteredPersons(nameof(Person.PersonName), searchString);
            foreach (PersonResponse addRequest in filteredSearchResponse)
            {
                addRequest.Country = countryService.GetCountryById(addRequest.CountryId).CountryName;
            }

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
        public void GetFilteredPersons_SortByName()
        {
            var personsServices = _provider.GetService<IPersonsServices>();
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "Westeris" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "Eastania" };

            CountryResponse countryResponse1 = countryService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = countryService.AddCountry(countryRequest2);

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
                PersonResponse aResponse = personsServices.AddPerson(addRequest);
                aResponse.Country = countryService.GetCountryById(aResponse.CountryId).CountryName;

                personAddResponse.Add(aResponse);
            }

            personAddResponse = personAddResponse.OrderByDescending(a => a.PersonName).ToList();

            //Get and Run Function
            List<PersonResponse> allPersons = personsServices.GetAllPersons();
            foreach (PersonResponse personResponse in allPersons)
            {
                personResponse.Country = countryService.GetCountryById(personResponse.CountryId).CountryName;
            }

            List<PersonResponse> sortedResponse = personsServices.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESC);
           

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
        public void GetPersonById_ValidId()
        {
            var personsService = _provider.GetService<IPersonsServices>();
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = countryService.AddCountry(countryAddRequest);

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

            PersonResponse response = personsService.AddPerson(requestParams);

            PersonResponse? personById = personsService.GetPersonById(response.PersonId);

            Assert.Equal(personById, response);
        }

        [Fact]
        public void GetPersonById_InvalidId()
        {
            var personsService = _provider.GetService<IPersonsServices>();

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

            PersonResponse response = personsService.AddPerson(requestParams);
            PersonResponse? personById = personsService.GetPersonById(Guid.NewGuid());

            Assert.Null(personById);
        }
        #endregion

        #region UpdatePerson

        //When PersonUpdateRequest is null, throw ArugmentNull Expection
        [Fact]
        public void UpdatePerson_PersonUpdateRequestNull()
        {
            var personServices = _provider.GetService<IPersonsServices>();

            Assert.Throws<ArgumentNullException>(() =>
            {
                PersonUpdateRequest request = null;
                personServices.UpdatePerson(request);
            });
        }

        [Fact]
        public void UpdatePerson_InvalidId()
        {
            var personServices = _provider.GetService<IPersonsServices>();

            Assert.Throws<ArgumentException>(() =>
            {
                PersonUpdateRequest request = new PersonUpdateRequest()
                {
                    PersonId = Guid.NewGuid()
                };

                personServices.UpdatePerson(request);
            });
        }

        public void UpdatePerson_InvalidName()
        {
            var personServices = _provider.GetService<IPersonsServices>();
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = countryService.AddCountry(countryAddRequest);

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

            PersonResponse response = personServices.AddPerson(requestParams);

            PersonUpdateRequest responseToUpdateRequest = response.ToPersonUpdateRequest();
            responseToUpdateRequest.PersonName = null;


            Assert.Throws<ArgumentException>(() =>
            {
                personServices.UpdatePerson(responseToUpdateRequest);
            });
        }

        public void UpdatePerson_FullDetails()
        {
            var personServices = _provider.GetService<IPersonsServices>();
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = countryService.AddCountry(countryAddRequest);

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

            PersonResponse response = personServices.AddPerson(requestParams);

            PersonUpdateRequest responseToUpdateRequest = response.ToPersonUpdateRequest();
            responseToUpdateRequest.PersonName = "Kenny";
            responseToUpdateRequest.Email = "Update@mail.com";
            PersonResponse updateResponse = personServices.UpdatePerson(responseToUpdateRequest);

            PersonResponse responseFromGetById = personServices.GetPersonById(responseToUpdateRequest.PersonId);

            Assert.Equal(updateResponse, responseFromGetById);

        }

        #endregion

        #region DeletePerson

        [Fact]
        public void DeletePerson_NullParam()
        {
            var personServices = _provider.GetService<IPersonsServices>();
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = countryService.AddCountry(countryAddRequest);

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

            PersonResponse response = personServices.AddPerson(requestParams);
            bool deleteSuccess = personServices.DeletePerson(null);

            Assert.False(deleteSuccess);
        }

        [Fact]
        public void Delete_Success()
        {
            var personServices = _provider.GetService<IPersonsServices>();
            var countryService = _provider.GetService<ICountriesService>();

            CountryAddRequest countryAddRequest = new CountryAddRequest()
            {
                CountryName = "Canada"
            };

            CountryResponse cuntryAddResponse = countryService.AddCountry(countryAddRequest);

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

            PersonResponse response = personServices.AddPerson(requestParams);
            bool deleteSuccess = personServices.DeletePerson(response.PersonId);

            Assert.True(deleteSuccess);
        }

        #endregion

    }
}
