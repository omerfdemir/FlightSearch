

# Flight Search API System

## Important Note ⚠️
This project requires .NET 9.0 and uses .NET Aspire for service orchestration. It cannot be run with earlier versions of .NET.

## Overview
A distributed microservices-based flight search system built with:
- .NET 9.0
- .NET Aspire
- Docker support
- Multi-environment configuration

## Services
The system consists of three main services:
1. **FlightSearchApi** (Port: 5085)
   - Aggregator service
   - Manages provider connections
   - Combines flight search results

2. **HopeAirProviderService** (Port: 5012)
   - First flight provider
   - Handles HopeAir specific integrations

3. **AybJetProviderApi** (Port: 5126)
   - Second flight provider
   - Manages AybJet specific operations

## Prerequisites
- .NET 9.0 SDK
- Docker (optional)
- Visual Studio 2022 17.9+ or JetBrains Rider 2024.1+

## Quick Start

### Using .NET Aspire
```bash
cd FlightSearch.AppHost
dotnet run
```

### Manual Service Start
```bash
# Start individual services
dotnet run --project HopeAirProviderService
dotnet run --project AybJetProviderApi
dotnet run --project FlightSearchApi
```

## Environment Configuration

### Supported Environments
- Development (default)
- Test
- Preprod
- Production

### Service URLs
- FlightSearchApi: http://localhost:5085
- HopeAirProviderService: http://localhost:5012
- AybJetProviderApi: http://localhost:5126
- Aspire Dashboard: http://localhost:15290

### Configuration Files
Each service includes environment-specific settings:
- appsettings.json
- appsettings.Development.json
- appsettings.Test.json
- appsettings.Preprod.json
- appsettings.Production.json

## Project Structure
```
FlightSearch/
├── FlightSearch.AppHost/        # .NET Aspire orchestrator
├── FlightSearchApi/             # Main aggregator service
├── HopeAirProviderService/      # First provider service
└── AybJetProviderApi/          # Second provider service
```

## Docker Support

### Building Images
```bash
docker build -t flightsearchapi -f FlightSearchApi/Dockerfile .
docker build -t hopeairprovider -f HopeAirProviderService/Dockerfile .
docker build -t aybjetprovider -f AybJetProviderApi/Dockerfile .
```

### Running Containers
```bash
docker run -e ASPNETCORE_ENVIRONMENT=Production flightsearchapi
docker run -e ASPNETCORE_ENVIRONMENT=Production hopeairprovider
docker run -e ASPNETCORE_ENVIRONMENT=Production aybjetprovider
```

## Development

### API Documentation
Swagger UI available in Development and Test environments:
- FlightSearchApi: http://localhost:5085/swagger
- HopeAirProviderService: http://localhost:5012/swagger
- AybJetProviderApi: http://localhost:5126/swagger

### Dependencies
All services use:
- Microsoft.AspNetCore.OpenApi (9.0.0)
- Swashbuckle.AspNetCore (7.0.0)

### .NET Aspire Features
- Centralized service orchestration
- Built-in service discovery
- Integrated monitoring dashboard
- Environment configuration management

## Contributing
1. Ensure .NET 9.0 SDK is installed
2. Fork the repository
3. Create feature branch
4. Submit pull request

## License
[Add your license information here]

## Notes
- All services require .NET 9.0
- Environment variables must be properly configured
- Docker support is available for containerized deployment
- Aspire dashboard provides monitoring capabilities