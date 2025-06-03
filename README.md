# SchraeglagenProtokoll API

A motorcycle riding protocol API built with ASP.NET Core and event sourcing using Marten. The application tracks motorcycle riders and their rides, providing a comprehensive system for logging journeys and managing rider information.

## Overview

SchraeglagenProtokoll (German for "lean angle protocol") is designed to help motorcycle enthusiasts track their rides, distances, and build a community around their shared passion. The API uses event sourcing patterns to maintain a complete history of all rider activities and ride logs.

### Key Features

- **Rider Management**: Register riders with email, full name, and road name
- **Ride Logging**: Track rides with start/destination locations, dates, and distances
- **Event Sourcing**: Complete audit trail of all activities using Marten
- **Projections**: Optimized read models for rider statistics and history
- **Comments**: Add comments to rides for additional context
- **API Documentation**: Integrated OpenAPI/Swagger documentation with Scalar UI

### Domain Models

#### Riders
- Registration with unique email and road name
- Renaming capabilities
- Account deletion with optional feedback
- Ride statistics and history tracking

#### Rides
- Complete ride logging with locations and distances
- Comment system for ride discussions
- Distance tracking with flexible units
- Temporal tracking with DateTimeOffset

## Prerequisites

- .NET 9.0 SDK
- Docker and Docker Compose (recommended)
- OR PostgreSQL database with manual configuration

## Getting Started

### Option 1: Using Docker Compose (Recommended)

The project includes a `docker-compose.yml` for easy PostgreSQL setup:

```bash
git clone <repository-url>
cd SchraeglagenProtokoll

# Start PostgreSQL container
docker-compose up -d

# Navigate to API project
cd SchraeglagenProtokoll.Api

# Configure connection string (using default docker-compose values)
dotnet user-secrets set "ConnectionStrings:Marten" "Host=localhost;Database=mydatabase;Username=myuser;Password=mypassword"

# Install dependencies and run
dotnet restore
dotnet run
```

### Option 2: Manual Database Setup

Configure your PostgreSQL connection string using user secrets:

```bash
cd SchraeglagenProtokoll.Api
dotnet user-secrets set "ConnectionStrings:Marten" "Host=localhost;Database=schraeglagen;Username=youruser;Password=yourpassword"
```

Or set the environment variable:
```bash
export ConnectionStrings__Marten="Host=localhost;Database=schraeglagen;Username=youruser;Password=yourpassword"
```

### Running the Application

```bash
# From the SchraeglagenProtokoll.Api directory
dotnet restore
dotnet run
```

The API will be available at:
- HTTP: http://localhost:5191
- HTTPS: https://localhost:7085

### API Documentation

Visit the Scalar API documentation interface at:
- Development: `https://localhost:7085/scalar/v1` (or HTTP equivalent)

### Docker Compose Configuration

The included `docker-compose.yml` provides:
- PostgreSQL 16+ container
- Configurable environment variables
- Persistent data volume
- Default values: database `mydatabase`, user `myuser`, password `mypassword`

Environment variables can be customized:
```bash
# .env file or environment
POSTGRES_CONTAINER_NAME=my_postgres
POSTGRES_USER=customuser
POSTGRES_PASSWORD=custompass
POSTGRES_DB=customdb
POSTGRES_HOST_PORT=5433
```

## Project Structure

```
SchraeglagenProtokoll/
├── SchraeglagenProtokoll.Api/     # Main API project
│   ├── Riders/                    # Rider domain and features
│   │   ├── Features/             # Rider-related endpoints and handlers
│   │   ├── Projections/          # Read model projections
│   │   └── Rider.cs             # Rider aggregate and events
│   ├── Rides/                    # Ride domain and features
│   │   ├── Features/            # Ride-related endpoints and handlers
│   │   ├── Distance.cs          # Distance value object
│   │   └── Ride.cs             # Ride aggregate and events
│   ├── Program.cs               # Application startup and configuration
│   └── StoreOptionsExtensions.cs # Marten configuration extensions
├── SchraeglagenProtokoll.Tests/   # Test project
│   ├── Riders/                   # Rider feature tests
│   ├── Rides/                    # Ride feature tests
│   ├── Faker/                    # Test data generation
│   └── WebAppFixture.cs         # Test infrastructure
├── docker-compose.yml            # PostgreSQL container setup
└── SchraeglagenProtokoll.sln     # Solution file
```

## Testing

The project includes comprehensive integration tests using:

### Test Framework
- **TUnit** - Modern .NET testing framework
- **Alba** - Integration testing for ASP.NET Core
- **Testcontainers** - PostgreSQL containers for isolated testing
- **Verify** - Snapshot testing for API responses
- **Bogus** - Test data generation
- **Shouldly** - Fluent assertions

### Test Structure
The test project (`SchraeglagenProtokoll.Tests`) includes:
- **Rider Tests**: Registration, retrieval, renaming, deletion, statistics
- **Ride Tests**: Logging rides, retrieving rides, adding comments
- **Performance Tests**: Testing with large event counts (absurd event counts)
- **Integration Tests**: Full end-to-end API testing with real database

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with specific filter
dotnet test --filter "GetAllTests"

# Run tests in the test project directory
cd SchraeglagenProtokoll.Tests/SchraeglagenProtokoll.Tests
dotnet test
```

### Test Features
- **Containerized Database**: Each test run uses a fresh PostgreSQL container
- **Snapshot Testing**: API responses are verified against stored snapshots
- **Test Data Generation**: Realistic test data using Faker patterns
- **Performance Testing**: Tests with high event volumes (10,000+ events per rider)
- **Projection Testing**: Validates both aggregation and projection-based retrieval

## Available Endpoints

### Riders
- `POST /riders` - Register a new rider
- `GET /riders` - Get all riders
- `GET /riders/{id}` - Get rider by ID (multiple implementations)
- `PUT /riders/{id}/rename` - Rename a rider
- `DELETE /riders/{id}` - Delete rider account
- `GET /riders/{id}/stats` - Get rider statistics
- `POST /riders/{id}/log-ride` - Log a new ride for rider

### Rides
- `GET /rides/{id}` - Get ride by ID
- `POST /rides/{id}/comments` - Add comment to ride

## Configuration Options

### Application Settings

- `InitializeWithInitialData`: Populate database with sample data on startup
- `CleanAllMartenData`: WARNING - Clears all data on startup (development only)

### Logging Configuration

The application uses structured logging with different levels for various components:
- Default: Information
- ASP.NET Core: Warning
- Npgsql (PostgreSQL): Warning  
- Marten: Warning

## Development Tools

### Marten Command Line

The application integrates Marten.CommandLine with Oakton for database management:

```bash
# View available commands
dotnet run -- help

# List projections
dotnet run -- projections -l

# Rebuild projections
dotnet run -- projections -r
```

### Technology Stack

- **ASP.NET Core 9.0** - Web API framework
- **Marten 7.40.5** - Event sourcing and document database
- **PostgreSQL** - Primary data store
- **Scalar** - API documentation UI
- **Oakton** - Command line parsing

## Contributing

The application follows domain-driven design principles with clear separation between aggregates (Riders, Rides) and uses event sourcing for complete audit trails. When contributing:

1. Follow the existing event sourcing patterns
2. Add appropriate tests for new features
3. Update API documentation for new endpoints
4. Consider projection impacts for read models

## License

MIT