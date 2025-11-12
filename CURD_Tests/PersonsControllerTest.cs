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

namespace CURD_Tests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsServices _personServices;
        private readonly ICountriesService _countriesService;

        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<IPersonsServices> _personsServiceMock;

        private readonly IFixture _fixture;
        private readonly Bogus.Faker _faker;

        public PersonsControllerTest()
        {
            _faker = new Bogus.Faker();
            _fixture = new Fixture();

            _personsServiceMock = new Mock<IPersonsServices>();
            _personServices = _personsServiceMock.Object;

            _countriesServiceMock = new Mock<ICountriesService>();
            _countriesService = _countriesServiceMock.Object;
        }

        #region Index

        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonsList()
        {
            List<PersonResponse> personsResponseList = _fixture.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_personServices, _countriesService);

            _personsServiceMock
                .Setup( x => x.GetFilteredPersons(It.IsAny<String>(), It.IsAny<String>()))
                .ReturnsAsync(personsResponseList);

            _personsServiceMock.Setup(x => x.GetSortedPersons( It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>()  ))
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

        [Fact]
        public async Task Create_IfModelErrors_ReturnToCreateView()
        {
            PersonAddRequest personAddRequest = _fixture.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixture.Create<PersonResponse>();

            List<CountryResponse> countryList = _fixture.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_personServices, _countriesService);

            _countriesServiceMock
                .Setup(x => x.GetAllCountries())
                .ReturnsAsync(countryList);

            _personsServiceMock
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

            PersonsController personsController = new PersonsController(_personServices, _countriesService);

            _countriesServiceMock
                .Setup(x => x.GetAllCountries())
                .ReturnsAsync(countryList);

            _personsServiceMock
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
