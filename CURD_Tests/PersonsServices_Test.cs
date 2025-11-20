using AutoFixture;
using AutoFixture.Kernel;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OfficeOpenXml.Drawing.Chart;
using RepositoryContracts;
using Serilog;
using Serilog.Extensions.Hosting;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Xunit.Abstractions;



namespace CURD_Tests
{
    public class PersonsServices_Test
    {
        private readonly IPersonAdderService _personAdderService;
        private readonly IPersonGetterServices _personGetterServices;
        private readonly IPersonSortPersonsService _personSortPersonsService;
        private readonly IPersonUpdatePersonService _personUpdatePersonService;
        private readonly IPersonDeletePersonService _personDeletePersonService;

        private readonly IPersonsToCSVService _personsToCSVService;
        private readonly IPersonsToExcelService _personsToExcelService;

        private readonly ITestOutputHelper _testOutputHelper;

        private readonly Mock<IPersonsRepository> _personsRepoMock;
        private readonly IPersonsRepository _personsRepository;

        private readonly IFixture _fixture;
        private readonly Bogus.Faker _faker;

        public PersonsServices_Test(ITestOutputHelper testOutputHelper)
        {
            _faker = new Bogus.Faker();
            _fixture = new Fixture();

            #region Repository Mock
            _personsRepoMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepoMock.Object;

            var PersonsInitialData = new List<Person>() { };
            var countriesInitialData = new List<Country> { };

            var services = new ServiceCollection();

            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>
             (new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            ApplicationDbContext dbContextPersons = dbContextMock.Object;
            dbContextMock.CreateDbSetMock(temp => temp.Persons, PersonsInitialData);

            ApplicationDbContext dbContextCountries = dbContextMock.Object;
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            #endregion

            var diagContextMock = new Mock<IDiagnosticContext>();

            var personsToCSVLoggerMock = new Mock<ILogger<PersonToCSVService>>();
            _personsToCSVService = new PersonToCSVService(_personsRepository, personsToCSVLoggerMock.Object, diagContextMock.Object);

            var personsToExcelLoggerMock = new Mock<ILogger<PersonToExcelService>>();
            _personsToExcelService = new PersonToExcelService(_personsRepository, personsToExcelLoggerMock.Object, diagContextMock.Object);

            var personAdderServiceLoggerMock = new Mock<ILogger<PersonAdderService>>();
            _personAdderService = new PersonAdderService(_personsRepository, personAdderServiceLoggerMock.Object, diagContextMock.Object);

            var personGetterServiceLoggerMock = new Mock<ILogger<PersonGetterServices>>();
            _personGetterServices = new PersonGetterServices(_personsRepository, _personsToCSVService, _personsToExcelService, personGetterServiceLoggerMock.Object, diagContextMock.Object);

            var personSortPersonsLoggerMock = new Mock<ILogger<PersonSortPersonsService>>();
            _personSortPersonsService = new PersonSortPersonsService(_personsRepository, personSortPersonsLoggerMock.Object, diagContextMock.Object);

            var personUpdatePersonLoggerMock = new Mock<ILogger<PersonUpdatePersonService>>();
            _personUpdatePersonService = new PersonUpdatePersonService(_personsRepository, personUpdatePersonLoggerMock.Object, diagContextMock.Object);

            var personDeletePersonLoggerMock = new Mock<ILogger<PersonDeletePersonService>>();
            _personDeletePersonService = new PersonDeletePersonService(_personsRepository, personDeletePersonLoggerMock.Object, diagContextMock.Object);

            _testOutputHelper = testOutputHelper;

            _fixture.Customize<PersonAddRequest>(composer =>
               composer.With(u => u.Email, _faker.Internet.Email()));

            //_fixture.Customize<Person>(composer =>
            //   composer.With(u => u.Email, _faker.Internet.Email()));
        }

        #region AddPerson

        //When PersonAddRequest is null, should throw NullRequestException
        [Fact]
        public async Task AddPerson_NullRequest_ArgumentNullException()
        {
            PersonAddRequest request = null;

            Func<Task> action =  async () =>
            {
                await _personAdderService.AddPerson(request);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();

        }

        //When provided field is null, should throw ArgumentExpection
        [Fact]
        public async Task AddPerson_PersonNameNullOrEmpty_ArgumentException()
        {
            PersonAddRequest personAddRequesst = _fixture.Create<PersonAddRequest>();
            personAddRequesst.PersonName = null as string;

            Person mockPerson = personAddRequesst.ToPerson();

            _personsRepoMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(mockPerson);

            Func<Task> action = async () =>
            {
                await _personAdderService.AddPerson(personAddRequesst);
            };

            await action.Should().ThrowAsync<ArgumentException>();

        }

        //When there are duplicate data, should ArgumentExpection
        public async Task AddPerson_PersonNameDublicate()
        {
            PersonAddRequest request1 = _fixture.Create<PersonAddRequest>();
            request1.PersonName = "Bob";

            PersonAddRequest request2 = _fixture.Create<PersonAddRequest>();
            request2.PersonName = "Bob";

            Func<Task> action = async () =>
            {
                await _personAdderService.AddPerson(request1);
                await _personAdderService.AddPerson(request2);
            };

            await action.Should().ThrowAsync<ArgumentException>();

        }

        //When add person perperly should suceed
        [Fact]
        public async Task AddPerson_ValidDetails_Successful() 
        {
            PersonAddRequest requestParams = _fixture.Create<PersonAddRequest>();
            Person mockPerson = requestParams.ToPerson();
            PersonResponse mockResponseExpected = mockPerson.ToPersonResponse();

            _personsRepoMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(mockPerson);
            
            //Act
            PersonResponse responseFromAdd = await _personAdderService.AddPerson(requestParams);
            mockResponseExpected.PersonId = responseFromAdd.PersonId;//Make sure IDs match

            responseFromAdd.PersonId.Should().NotBe(Guid.Empty);
            responseFromAdd.Should().Be(mockResponseExpected);
        }

        #endregion

        #region GetAllPersons 
        [Fact]
        public async Task GetAllPersons_Empty()
        {
            List<Person> person = new List<Person>();

            _personsRepoMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(person);

            List<PersonResponse> result = await _personGetterServices.GetAllPersons();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllPersons_AddFew()
        {
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "email1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "email2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "email3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> expectedResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepoMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);

            List<PersonResponse> requestResponse = await _personGetterServices.GetAllPersons();

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in expectedResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            _testOutputHelper.WriteLine("Response:");
            foreach (var response in requestResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            //Validate
            requestResponse.Should().BeEquivalentTo(expectedResponse);

        }

        #endregion

        #region GetFilteredPersons

        //If the search text is empty, return all Persons
        [Fact]
        public async Task GetFilteredPersons_SerchStringEmpty()
        {
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "email1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "email2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "email3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> expectedResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepoMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);
           
            List<PersonResponse> requestResponse = await _personGetterServices.GetFilteredPersons(nameof(Person.PersonName), "");

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in expectedResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            _testOutputHelper.WriteLine("Response:");
            foreach (var response in requestResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            requestResponse.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task GetFilteredPersons_SearchByName()
        {
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "email1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "email2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "email3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> expectedResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepoMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            List<PersonResponse> requestResponse = await _personGetterServices.GetFilteredPersons(nameof(Person.PersonName), "");

            _testOutputHelper.WriteLine("Added:");
            foreach (var response in expectedResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            _testOutputHelper.WriteLine("Response:");
            foreach (var response in requestResponse)
            {
                _testOutputHelper.WriteLine(response.ToString());
            }

            requestResponse.Should().BeEquivalentTo(expectedResponse);
        }

        #endregion

        #region GetSortedPersons

        [Fact]
        public async Task GetFilteredPersons_SortByName()
        {
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "email1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "email2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "email3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> unsortedPersonsResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepoMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            List<PersonResponse> requestResponse = await _personGetterServices.GetFilteredPersons(nameof(Person.PersonName), "");

            List<PersonResponse> sortedResponse = await _personSortPersonsService.GetSortedPersons(unsortedPersonsResponse, nameof(Person.PersonName), SortOrderOptions.DESC);
           
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
        public async Task GetPersonById_ValidId_NotNull()
        {
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "email@example.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            PersonResponse responseExpected = person.ToPersonResponse();

            _personsRepoMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            PersonResponse? personById = await _personGetterServices.GetPersonById(person.PersonId);

            personById.Should().Be(responseExpected);
        }

        [Fact]
        public async Task GetPersonById_InvalidId_Null()
        {
            PersonResponse? personById = await _personGetterServices.GetPersonById(Guid.NewGuid());

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
                await _personUpdatePersonService.UpdatePerson(request);
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
                await _personUpdatePersonService.UpdatePerson(request);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdatePerson_NameNull()
        {
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, "email@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse response = person.ToPersonResponse();
            
            PersonUpdateRequest responseToUpdateRequest = response.ToPersonUpdateRequest();

            _personsRepoMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
            _personsRepoMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            Func<Task> action = async () => 
            {
                await _personUpdatePersonService.UpdatePerson(responseToUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();

        }

        [Fact]
        public async Task UpdatePerson_FullDetails()
        {
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Johan")
                .With(temp => temp.Email, "email@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse personResponse = person.ToPersonResponse();
            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
         
            _personsRepoMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
            _personsRepoMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            PersonResponse updateResponse = await _personUpdatePersonService.UpdatePerson(personUpdateRequest);

            updateResponse.Should().Be(personResponse);
        }

        #endregion

        #region DeletePerson

        [Fact]
        public async Task DeletePerson_NullParam()
        {
            Person person = _fixture.Build<Person>()
               .With(temp => temp.PersonName, "Johan")
               .With(temp => temp.Email, "email@example.com")
               .With(temp => temp.Country, null as Country)
               .With(temp => temp.Gender, "Male")
               .Create();

            _personsRepoMock.Setup( temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);
            _personsRepoMock.Setup(temp => temp.DeletePersonsById(It.IsAny<Guid>())).ReturnsAsync(false);

            bool deleteSuccess = await _personDeletePersonService.DeletePerson(null);

            deleteSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Delete_Success()
        {
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Johan")
                .With(temp => temp.Email, "email@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            _personsRepoMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);
            _personsRepoMock.Setup(temp => temp.DeletePersonsById(It.IsAny<Guid>())).ReturnsAsync(true);
            
            bool deleteSuccess = await _personDeletePersonService.DeletePerson(person.PersonId);

            deleteSuccess.Should().BeTrue();
        }

        #endregion

    }
}
