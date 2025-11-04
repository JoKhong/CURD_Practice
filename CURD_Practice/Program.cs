using Services;
using ServiceContracts;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

//Optional, For Dependency Injection
builder.Services.AddTransient<ICountriesService>( provider =>
{
    return new CountryServices(true);
});
builder.Services.AddTransient<IPersonsServices, PersonServices>();

//Optional, For Options
//builder.Services.Configure<TradingOptions>(builder.Configuration.GetSection(nameof(TradingOptions)));

var app = builder.Build();

if(builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
