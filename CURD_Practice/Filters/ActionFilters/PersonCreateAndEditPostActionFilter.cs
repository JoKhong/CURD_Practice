using CURD_Practice.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO;

namespace CURD_Practice.Filters.ActionFilters
{
    public class PersonCreateAndEditPostActionFilter : IAsyncActionFilter
    {
        private readonly ICountryGetterServices _countryGetCountriesServices;

        public PersonCreateAndEditPostActionFilter(ICountryGetterServices countryGeetCountriesServices)
        {
            _countryGetCountriesServices = countryGeetCountriesServices;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if(context.Controller is PersonsController personsController)
            {
                if(!personsController.ModelState.IsValid)
                {
                    List<CountryResponse> allCountires = await _countryGetCountriesServices.GetCountriesAll();

                    IEnumerable<SelectListItem> selectCountires =
                        allCountires.Select(aCountry => new SelectListItem()
                        {
                            Text = aCountry.CountryName,
                            Value = aCountry.CountryId.ToString()
                        });

                    personsController.ViewBag.Countries = selectCountires;
                    personsController.ModelState.Values.SelectMany( v => v.Errors).Select(e  => e.ErrorMessage).ToList();

                    var personsRequest = context.ActionArguments["personRequest"];
                    context.Result = personsController.View(personsRequest);//Short circuite execution
                }
                else
                {
                    await next();
                }
            }
            else
            {
                await next();
            }


        }
    }
}
