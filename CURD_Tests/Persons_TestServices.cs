using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
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
            services.AddScoped<IPersonsServices, PersonServices>();
            services.AddScoped<ICountriesService, CountryServices>();

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
                CountryID = null,
                Address = null,
                ReceiveNewsLetters = false,
            };

            PersonResponse addedPerson = personServices.AddPerson(requestParams);

            List<PersonResponse> allPersons = personServices.GetAllPersons();

            Assert.True(addedPerson.PersonID != Guid.Empty);
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
                new PersonAddRequest() {PersonName = "Anya", Email = "Anya@Forger.com"},
                new PersonAddRequest() {PersonName = "Loid", Email = "Loid@Forger.com"},
                new PersonAddRequest() {PersonName = "Yor", Email = "Yor@Forger.com"},
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
                CountryID = cuntryAddResponse.CountryId,
                Address = null,
                ReceiveNewsLetters = false,
            };

            PersonResponse response = personsService.AddPerson(requestParams);

            PersonResponse? countryById = personsService.GetPersonById(response.PersonID);

            Assert.Equal(countryById, response);
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
                CountryID = null,
                Address = null,
                ReceiveNewsLetters = false,
            };

            PersonResponse response = personsService.AddPerson(requestParams);
            PersonResponse? personById = personsService.GetPersonById(Guid.NewGuid());

            Assert.Null(personById);
        }
        #endregion
    }
}
