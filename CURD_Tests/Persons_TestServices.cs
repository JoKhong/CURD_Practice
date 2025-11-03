using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;

namespace CURD_Tests
{
    public class Persons_TestServices
    {
        private readonly ServiceProvider _provider;

        public Persons_TestServices()
        {
            var services = new ServiceCollection();
            services.AddScoped<IPersonsServices, PersonServices>();

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
                new PersonAddRequest() {PersonName = "Anya"},
                new PersonAddRequest() {PersonName = "Loid"},
                new PersonAddRequest() {PersonName = "Yor"},
            };

            foreach (PersonAddRequest addRequest in addRequests)
            {
                addedResponses.Add(personsServices.AddPerson(addRequest));
            }
            
            List<PersonResponse> requestResponse = personsServices.GetAllPersons();

            foreach (PersonResponse item in addedResponses)
            {
                Assert.Contains(item, requestResponse);
            }
        }

        #endregion




    }
}
