using Microsoft.Extensions.DependencyInjection;
using ServiceContracts;
using ServiceContracts.DTO;
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

        [Fact]
        public void AddPerson_Test()
        {
            var personsServices = _provider.GetService<IPersonsServices>();

            Assert.Throws<NotImplementedException>(() => {
                PersonAddRequest request = new PersonAddRequest();
                personsServices.AddPerson(request);
            });
        }

    }
}
