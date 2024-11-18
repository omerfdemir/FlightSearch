var builder = DistributedApplication.CreateBuilder(args);

var flightSearchApi = builder.AddProject<Projects.FlightSearchApi>("flightSearchApi");
var flightBookApi = builder.AddProject<Projects.FlightBookApi>("flightBookApi");
var aybJetProviderApi = builder.AddProject<Projects.AybJetProviderApi>("aybJetProviderApi");
var hopeAirProviderService = builder.AddProject<Projects.HopeAirProviderApi>("hopeAirProviderService");

builder.Build().Run();