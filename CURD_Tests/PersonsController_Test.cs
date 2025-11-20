using AutoFixture;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading.Tasks;
using FluentAssertions;

using CURD_Practice.Controllers;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Microsoft.AspNetCore.Mvc;
using NuGet.Frameworks;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Services;

namespace CURD_Tests
{
    public class PersonsController_Test
    {
        private readonly IPersonAdderService _personAdderService;
        private readonly Mock<IPersonAdderService> _personAdderServiceMock;

        private readonly IPersonGetterServices _personGetterServices;
        private readonly Mock<IPersonGetterServices> _personGetterServicesMock;

        private readonly IPersonSortPersonsService _personSortedPersonsServices;
        private readonly Mock<IPersonSortPersonsService> _personSortedPersonsServicesMock;

        private readonly IPersonUpdatePersonService _personUpdatePersonService;
        private readonly Mock<IPersonUpdatePersonService> _personUpdatePersonServiceMock;

        private readonly IPersonDeletePersonService _personDeletePersonService;
        private readonly Mock<IPersonDeletePersonService> _personDeletePersonServiceMock;

        private readonly IPersonsToCSVService _personsToCSVService;
        private readonly Mock<IPersonsToCSVService> _personsToCSVServiceMock;

        private readonly IPersonsToExcelService _personsToExcelService;
        private readonly Mock<IPersonsToExcelService> _personsToExcelServiceMock;

        private readonly ICountryGetCountriesServices _countryGetCountriesServices;
        private readonly Mock<ICountryGetCountriesServices> _countryGetCountriesServicesMock;

        private readonly ILogger<PersonsController> _logger;
        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        private readonly IFixture _fixture;
        private readonly Bogus.Faker _faker;

        public PersonsController_Test()
        {
            _faker = new Bogus.Faker();
            _fixture = new Fixture();

            _personAdderServiceMock = new Mock<IPersonAdderService>();
            _personAdderService = _personAdderServiceMock.Object;

            _personGetterServicesMock = new Mock<IPersonGetterServices>();
            _personGetterServices = _personGetterServicesMock.Object;

            _personSortedPersonsServicesMock = new Mock<IPersonSortPersonsService>();
            _personSortedPersonsServices = _personSortedPersonsServicesMock.Object;

            _personUpdatePersonServiceMock = new Mock<IPersonUpdatePersonService>();
            _personUpdatePersonService = _personUpdatePersonServiceMock.Object;

            _personDeletePersonServiceMock = new Mock<IPersonDeletePersonService>();
            _personDeletePersonService = _personDeletePersonServiceMock.Object;

            _personsToCSVServiceMock = new Mock<IPersonsToCSVService>();
            _personsToCSVService = _personsToCSVServiceMock.Object;

            _personsToExcelServiceMock = new Mock<IPersonsToExcelService>();
            _personsToExcelService = _personsToExcelServiceMock.Object;

            _countryGetCountriesServicesMock = new Mock<ICountryGetCountriesServices>();
            _countryGetCountriesServices = _countryGetCountriesServicesMock.Object;

            _loggerMock = new Mock<ILogger<PersonsController>>();
            _logger = _loggerMock.Object;
        }

        #region Index

        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonsList()
        {
            List<PersonResponse> personsResponseList = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(
                _personAdderService, 
                _personGetterServices, 
                _personSortedPersonsServices, 
                _personUpdatePersonService, 
                _personDeletePersonService, 
                _personsToCSVService , 
                _personsToExcelService, 
                _countryGetCountriesServices, 
                _logger);

            _personGetterServicesMock
                .Setup( x => x.GetFilteredPersons(It.IsAny<String>(), It.IsAny<String>()))
                .ReturnsAsync(personsResponseList);

            _personSortedPersonsServicesMock.Setup(x => x.GetSortedPersons( It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>()  ))
                .ReturnsAsync(personsResponseList);

            IActionResult result = await personsController.Index(
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<SortOrderOptions>());

            //Cannot use Fluent Assertion for type
            //ViewResult viewResult = result.Should().BeOfType<ViewResult>();
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();
            viewResult.ViewData.Model.Should().Be(personsResponseList);
        }

        #endregion


        #region Create

        // [Fact] Responsibility shifted to Filters
        public async Task Create_IfModelErrors_ReturnToCreateView()
        {
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryList = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(
               _personAdderService,
               _personGetterServices,
               _personSortedPersonsServices,
               _personUpdatePersonService,
               _personDeletePersonService,
               _personsToCSVService,
               _personsToExcelService,
               _countryGetCountriesServices,
               _logger);

            _countryGetCountriesServicesMock
                .Setup(x => x.GetCountriesAll())
                .ReturnsAsync(countryList);

            _personAdderServiceMock
                .Setup(x => x.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            //Act
            personsController.ModelState.AddModelError("PersonName", "Person name cannot be blank");
            IActionResult result = await personsController.Create(personAddRequest);

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);//Cannot use Fluent Assertion for type Casting

            viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
            viewResult.ViewData.Model.Should().Be(personAddRequest);
        }

        [Fact]
        public async Task Create_IfNoModel_ReturnToIndex()
        {
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryList = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(
                _personAdderService,
                _personGetterServices,
                _personSortedPersonsServices,
                _personUpdatePersonService,
                _personDeletePersonService,
                _personsToCSVService,
                _personsToExcelService,
                _countryGetCountriesServices,
                _logger);

            _countryGetCountriesServicesMock
                .Setup(x => x.GetCountriesAll())
                .ReturnsAsync(countryList);

            _personAdderServiceMock
                .Setup(x => x.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            //Act
            IActionResult result = await personsController.Create(personAddRequest);

            //Assert
            RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result);//Cannot use Fluent Assertion for type Casting

            viewResult.ActionName.Should().Be("Index");
        }

        #endregion



    }
}
