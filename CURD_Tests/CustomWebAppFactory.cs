using Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace CURD_Tests
{
    public class CustomWebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.UseEnvironment("Test");

            builder.ConfigureServices( services =>
            {
                #region Remove old actual DB to replace with mock

                var descriptor = services.SingleOrDefault
                    (temp => temp.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if(descriptor != null)
                {
                    services.Remove(descriptor);
                }

                #endregion

                //Add mock DBContext
                services.AddDbContext<ApplicationDbContext>( options =>
                {
                    options.UseInMemoryDatabase("DatabaseforTesting");
                });

            });

        }
    }
}
