using FllightSearchApi.Services;
using FllightSearchApi.Configuration;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<ProviderServiceSettings>(
    builder.Configuration.GetSection("ProviderServices"));

builder.Services.AddControllers();

builder.Services.AddHttpClient("HopeAirClient", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<ProviderServiceSettings>>().Value;
    client.BaseAddress = new Uri(settings.HopeAir.BaseUrl);
});

builder.Services.AddHttpClient("AybJetClient", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<ProviderServiceSettings>>().Value;
    client.BaseAddress = new Uri(settings.AybJet.BaseUrl);
});

builder.Services.AddScoped<IFlightSearchService, FlightSearchService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();