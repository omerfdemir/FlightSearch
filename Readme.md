# Flight Search and Booking System

A distributed system for searching and booking flights across multiple airline providers, built with .NET 9.0 microservices architecture.

## Project Structure

```
├── FlightSearchApi/           # Aggregates flight search results
├── FlightBookApi/            # Handles flight bookings
├── HopeAirProviderApi/       # SOAP-based airline provider
├── AybJetProviderApi/        # REST-based airline provider
└── Tests/
    ├── FlightSearchApi.Tests/
    ├── FlightBookApi.Tests/
    ├── HopeAirProviderApi.Tests/
    └── AybJetProviderApi.Tests/
```

## Features

- Aggregated flight search across multiple providers
- Real-time flight streaming
- Provider-specific booking handling
- SOAP and REST integration
- Fault tolerance and timeout handling
- In-memory flight caching

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Docker (optional)

### Installation

```bash
# Clone the repository
git clone https://github.com/yourusername/flight-search-system.git

# Navigate to the solution directory
cd flight-search-system

# Restore dependencies
dotnet restore

# Run tests
dotnet test
```

## API Endpoints

### Flight Search API

```http
POST /api/search
Content-Type: application/json

{
    "origin": "LHR",
    "destination": "JFK",
    "departureDate": "2024-03-20"
}
```

### Flight Booking API

```http
POST /api/book
Content-Type: application/json

{
    "flightNumber": "HH123"
}
```

## Testing

The solution includes comprehensive unit tests for all components:

### Running Tests

```bash
dotnet test --collect:"XPlat Code Coverage"
```


### Key Test Areas

- Flight search aggregation
- Provider timeout handling
- Booking validation
- SOAP/REST integration
- Cache management
- Error handling
