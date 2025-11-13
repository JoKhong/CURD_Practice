using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fizzler;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

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

            string responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument htmlDom = new HtmlDocument();
            htmlDom.LoadHtml(responseBody);

            var document = htmlDom.DocumentNode;

            document.QuerySelectorAll("table.persons").Should().NotBeNull();

        }
    }
}
