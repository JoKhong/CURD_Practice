using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CURD_Tests
{
    public class PersonsController_Integration_Test : IClassFixture<CustomWebAppFactory>
    {
        private readonly HttpClient _httpClient;

        public PersonsController_Integration_Test(CustomWebAppFactory factory)
        {
            _httpClient = factory.CreateClient();
        }

        [Fact]
        public async Task Index_ToReturnView()
        {
            //Act
            HttpResponseMessage response = await _httpClient.GetAsync("/Persons/Index");

            //Assert
            response.IsSuccessStatusCode.Should().BeTrue();
        }
    }
}
