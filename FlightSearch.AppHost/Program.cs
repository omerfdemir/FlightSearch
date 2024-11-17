var builder = DistributedApplication.CreateBuilder(args);

var flightSearchApi = builder.AddProject<Projects.FlightSearchApi>("flightSearchApi");
var aybJetProviderApi = builder.AddProject<Projects.AybJetProviderApi>("aybJetProviderApi");
var hopeAirProviderService = builder.AddProject<Projects.HopeAirProviderService>("hopeAirProviderService");

builder.Build().Run();