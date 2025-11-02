using Xunit;
using Microsoft.Extensions.DependencyInjection;

using MathServices;
using MathServiceContracts;
using FluentAssertions;

namespace Math_Tests
{
    public class MathServicesTest
    {
        private readonly ServiceProvider _provider;

        public MathServicesTest()
        {
            var services = new ServiceCollection();
            services.AddTransient<ISum, MathService>();

            _provider = services.BuildServiceProvider();
        }

        [Fact]
        public void SumOf_2Plus2_ShouldBe4()
        {
            var sumService = _provider.GetService<ISum>();
            var result = sumService.Add(2, 2);

            result.Should().Be(4);
        }

    }
}
