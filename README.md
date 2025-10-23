# 📚 Configuration Reader - Dynamic Configuration Manager

![API](https://i.imgur.com/8FIsvQn.png)&nbsp;&nbsp;&nbsp;
![Service-A](https://i.imgur.com/8927VOD.png)&nbsp;&nbsp;&nbsp;
![AdminPanel](https://i.imgur.com/KFWfGrv.png)&nbsp;&nbsp;&nbsp;

A **production-ready**, **enterprise-grade** dynamic configuration management system for microservices architecture. Manage application configurations in real-time without service restarts, featuring multi-level caching, centralized administration, and zero-downtime updates.

---

## 🎯 Features

### Core Capabilities
- ✅ **Dynamic Configuration Updates** - Change configs without restarting services
- ✅ **Zero Downtime** - Update configurations while services are running
- ✅ **Multi-Level Caching** - In-Memory → Redis → PostgreSQL
- ✅ **Auto-Refresh** - Background service syncs configs every 30 seconds
- ✅ **Type-Safe** - Strong typing with `string`, `int`, `bool`, `double`
- ✅ **Multi-Service Support** - Manage configs for multiple applications
- ✅ **Centralized Management** - Beautiful Blazor WebAssembly admin panel
- ✅ **RESTful API** - Complete CRUD operations with Swagger documentation
- ✅ **Client Library** - Easy integration via NuGet-style package

### Architecture
- 🏗️ **Clean Architecture** - Separation of concerns with DDD principles
- 🎯 **CQRS Pattern** - Command/Query separation with MediatR
- 📦 **Repository Pattern** - Abstracted data access layer
- 🔄 **Background Services** - Automatic configuration refresh
- 🎨 **Blazor WebAssembly** - Modern, responsive admin UI with MudBlazor
- 🔐 **Validation** - FluentValidation for input validation
- 📊 **Logging** - Structured logging with Serilog

---

## 📋 Table of Contents

- [Quick Start](#-quick-start)
- [Architecture](#️-architecture)
- [Project Structure](#-project-structure)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Configuration](#️-configuration)
- [Usage](#-usage)
- [API Documentation](#-api-documentation)
- [Admin Panel](#-admin-panel)
- [Client Library](#-client-library)
- [Examples](#-examples)
- [Deployment](#-deployment)
- [Testing](#-testing)
- [Contributing](#-contributing)

---

## 🚀 Quick Start

### 1. Clone the repository
```bash
git clone https://github.com/cevdettsedeff/DynamicConfigurationManager_NetCore
cd ConfigurationReader
```

### 2. Start infrastructure (Docker)
```bash
# PostgreSQL
docker run -d --name postgres-config \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:latest

# Redis
docker run -d --name redis-cache \
  -p 6379:6379 \
  redis:latest
```

### 3. Apply database migrations
```bash
cd ConfigurationReader.Persistence
dotnet ef database update --startup-project ../ConfigurationReader.Api
```

### 4. Seed the database
```bash
cd ConfigurationReader.Api
dotnet run

# In another terminal
curl -X POST http://localhost:5000/api/seed
```

### 5. Start services
```bash
# Terminal 1 - API
cd ConfigurationReader.Api
dotnet run
# Running on: http://localhost:5000

# Terminal 2 - Admin Panel
cd ConfigurationReader.AdminPanel
dotnet run
# Running on: https://localhost:7001

# Terminal 3 - Sample Service (Optional)
cd Sample.ServiceA
dotnet run
# Running on: https://localhost:7002
```

### 6. Access the applications

- **Admin Panel**: https://localhost:7001/
- **API Swagger**: http://localhost:5000/
- **Sample Service**: https://localhost:7002/

---

## 🏗️ Architecture

### System Overview
```
┌──────────────────────────────────────────────────────────┐
│                     PRESENTATION LAYER                    │
├────────────────────────────┬─────────────────────────────┤
│  Admin Panel (Blazor WASM) │  API (ASP.NET Core)         │
│  - MudBlazor UI            │  - REST Endpoints           │
│  - CRUD Operations         │  - Swagger/OpenAPI          │
└────────────────────────────┴─────────────────────────────┘
                              ▼
┌──────────────────────────────────────────────────────────┐
│                   APPLICATION LAYER                       │
│  - CQRS (Commands/Queries)                               │
│  - MediatR                                                │
│  - FluentValidation                                       │
│  - AutoMapper                                             │
└──────────────────────────────────────────────────────────┘
                              ▼
┌──────────────────────────────────────────────────────────┐
│                     DOMAIN LAYER                          │
│  - Entities (ConfigurationItem)                          │
│  - Enums (ConfigurationType)                             │
│  - Interfaces (IConfigurationRepository)                 │
│  - Business Rules                                         │
└──────────────────────────────────────────────────────────┘
                              ▼
┌──────────────────────┬───────────────────────────────────┤
│ INFRASTRUCTURE       │  PERSISTENCE                       │
│ - Redis Cache        │  - Entity Framework Core           │
│ - Background Jobs    │  - PostgreSQL                      │
│ - External Services  │  - Repositories                    │
└──────────────────────┴────────────────────────────────────┘
                              ▼
┌──────────────────────────────────────────────────────────┐
│            CLIENT SDK (ConfigurationReader.Library)       │
│  - Used by microservices to read configurations          │
└──────────────────────────────────────────────────────────┘
```

### Configuration Flow
```
1. ADMIN PANEL: Update MaxItemCount = 3 → 5
   PUT /api/configurations/2
                ↓
2. API: Update database & clear Redis cache
                ↓
3. BACKGROUND SERVICE: Auto-refresh (30s interval)
   - Reload all configs from database
   - Update in-memory cache
   - Sync to Redis
                ↓
4. MICROSERVICE: Use new value
   _configReader.GetValue<int>("MaxItemCount") → 5
                ↓
5. RESULT: Zero downtime configuration change ✅
```

---

## 📁 Project Structure
```
ConfigurationReader/
├── ConfigurationReader.Domain/              # Core business logic
│   ├── Entities/
│   │   └── ConfigurationItem.cs            # Main entity
│   ├── Enums/
│   │   └── ConfigurationType.cs            # String, Int, Bool, Double
│   └── Interfaces/
│       └── IConfigurationRepository.cs     # Repository contract
│
├── ConfigurationReader.Application/         # Use cases & business rules
│   ├── Features/
│   │   └── Configurations/
│   │       ├── Commands/                   # Write operations
│   │       │   ├── CreateConfiguration/
│   │       │   ├── UpdateConfiguration/
│   │       │   └── DeleteConfiguration/
│   │       └── Queries/                    # Read operations
│   │           ├── GetAllConfigurations/
│   │           ├── GetConfigurationById/
│   │           └── GetConfigurationByKey/
│   ├── DTOs/                               # Data Transfer Objects
│   ├── Mappings/                           # AutoMapper profiles
│   └── Validators/                         # FluentValidation
│
├── ConfigurationReader.Persistence/         # Data access layer
│   ├── Context/
│   │   └── ConfigurationDbContext.cs       # EF Core DbContext
│   ├── Configurations/
│   │   └── ConfigurationItemConfiguration.cs # Entity configuration
│   ├── Repositories/
│   │   └── ConfigurationRepository.cs      # Implementation
│   ├── Seeders/
│   │   └── DatabaseSeeder.cs               # Sample data
│   └── Migrations/                         # EF Core migrations
│
├── ConfigurationReader.Infrastructure/      # External services
│   ├── Caching/
│   │   └── RedisConfigurationCache.cs      # Redis implementation
│   └── BackgroundServices/
│       └── ConfigurationRefreshService.cs  # Auto-refresh worker
│
├── ConfigurationReader.Api/                 # REST API
│   ├── Controllers/
│   │   └── ConfigurationsController.cs     # CRUD endpoints
│   ├── Endpoints/
│   │   └── SeedEndpoints.cs                # Minimal API for seeding
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs  # Global error handling
│   └── Program.cs                          # Application entry point
│
├── ConfigurationReader.AdminPanel/          # Blazor WebAssembly UI
│   ├── Pages/
│   │   ├── Index.razor                     # Home page
│   │   ├── Configurations.razor            # Config management
│   │   ├── CreateConfigurationDialog.razor # Create dialog
│   │   └── EditConfigurationDialog.razor   # Edit dialog
│   ├── Services/
│   │   └── ConfigurationApiService.cs      # API client
│   ├── Models/
│   │   └── ConfigurationItemDto.cs         # View models
│   └── wwwroot/                            # Static files
│
├── ConfigurationReader.Library/             # Client SDK
│   ├── ConfigurationReader.cs              # Main reader class
│   ├── IConfigurationReader.cs             # Interface
│   ├── ConfigurationReaderOptions.cs       # Configuration options
│   └── ServiceCollectionExtensions.cs      # DI registration
│
└── Sample.ServiceA/                         # Example microservice
    ├── Controllers/
    │   └── ProductsController.cs           # Sample endpoints
    ├── Services/
    │   └── ProductService.cs               # Uses ConfigurationReader
    └── Program.cs                          # Library integration
```

---

## 📋 Prerequisites

- **.NET 9.0 SDK** or later
- **PostgreSQL 16+** (or Docker)
- **Redis 7.0+** (or Docker)
- **Visual Studio 2022** 
- **Docker Desktop** (recommended for infrastructure)

---

## 🔧 Installation

### Option 1: Docker Compose (Recommended)

Create `docker-compose.yml`:
```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    container_name: configreader-postgres
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: ConfigurationDb
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7
    container_name: configreader-redis
    ports:
      - "6379:6379"

volumes:
  postgres_data:
```

Run:
```bash
docker-compose up -d
```

### Option 2: Manual Installation

#### PostgreSQL
```bash
# Ubuntu/Debian
sudo apt-get install postgresql-16

# macOS
brew install postgresql@16

# Windows - Download from:
# https://www.postgresql.org/download/windows/
```

#### Redis
```bash
# Ubuntu/Debian
sudo apt-get install redis-server

# macOS
brew install redis

# Windows - Download from:
# https://redis.io/download
```

---

## ⚙️ Configuration

### API Configuration

`ConfigurationReader.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;Database=ConfigurationDb;Username=postgres;Password=postgres",
    "Redis": "localhost:6379,abortConnect=false"
  },
  "ConfigurationRefreshIntervalSeconds": 30,
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Admin Panel Configuration

`ConfigurationReader.AdminPanel/Program.cs`:
```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("http://localhost:5000")
});
```

### Client Library Configuration

`Sample.ServiceA/Program.cs`:
```csharp
builder.Services.AddConfigurationReader(options =>
{
    options.ApplicationName = "SERVICE-A";
    options.ConnectionString = "Host=localhost;Port=5432;...";
    options.RedisConnectionString = "localhost:6379";
    options.RefreshIntervalSeconds = 30;
    options.CacheExpirationMinutes = 5;
    options.EnableLogging = true;
});
```

---

## 🎯 Usage

### Creating a Configuration

**Via Admin Panel:**
1. Navigate to https://localhost:7001/configurations
2. Click **Create** button
3. Fill in the form
4. Click **Create**

**Via API:**
```bash
curl -X POST http://localhost:5000/api/configurations \
  -H "Content-Type: application/json" \
  -d '{
    "name": "MaxItemCount",
    "type": "int",
    "value": "10",
    "applicationName": "SERVICE-A",
    "isActive": true
  }'
```

### Reading a Configuration
```csharp
public class ProductService
{
    private readonly IConfigurationReader _configReader;
    
    public List<Product> GetProducts()
    {
        var maxItems = _configReader.GetValue<int>("MaxItemCount");
        return _products.Take(maxItems).ToList();
    }
}
```

### Updating a Configuration

**Via Admin Panel:**
1. Click **Edit** ✏️ icon
2. Change value
3. Click **Update**

**Via API:**
```bash
curl -X PUT http://localhost:5000/api/configurations/1 \
  -H "Content-Type: application/json" \
  -d '{"value": "20", "isActive": true}'
```

---

## 📡 API Documentation

### Base URL
```
http://localhost:5000
```

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/configurations` | Get all configurations |
| `GET` | `/api/configurations?applicationName=SERVICE-A` | Filter by application |
| `GET` | `/api/configurations/{id}` | Get configuration by ID |
| `GET` | `/api/configurations/{app}/{key}` | Get by app and key |
| `POST` | `/api/configurations` | Create new configuration |
| `PUT` | `/api/configurations/{id}` | Update configuration |
| `DELETE` | `/api/configurations/{id}` | Delete configuration |
| `GET` | `/api/configurations/applications` | Get all application names |
| `POST` | `/api/seed` | Seed database with sample data |
| `GET` | `/health` | Health check |

### Example Response
```json
{
  "isSuccess": true,
  "message": "Configurations retrieved successfully",
  "data": [
    {
      "id": 1,
      "name": "SiteName",
      "type": "String",
      "value": "soty.io",
      "isActive": true,
      "applicationName": "SERVICE-A",
      "createdAt": "2025-01-23T10:00:00Z",
      "updatedAt": "2025-01-23T10:00:00Z"
    }
  ]
}
```

---

## 🎨 Admin Panel

### Features

- 📊 Dashboard statistics
- 🔍 Search & filter
- ➕ Create configuration
- ✏️ Edit configuration
- 🗑️ Delete configuration
- 🔄 Auto-refresh
- 📱 Responsive design

### Access
```
https://localhost:7001/configurations
```

---

## 📦 Client Library

### Installation
```bash
dotnet add reference ../ConfigurationReader.Library/ConfigurationReader.Library.csproj
```

### API Reference
```csharp
public interface IConfigurationReader
{
    T GetValue<T>(string key);
    Task<T> GetValueAsync<T>(string key);
    bool TryGetValue<T>(string key, out T value);
    Task RefreshAsync();
}
```

### Usage
```csharp
public class MyService
{
    private readonly IConfigurationReader _config;
    
    public void DoSomething()
    {
        var siteName = _config.GetValue<string>("SiteName");
        var maxItems = _config.GetValue<int>("MaxItemCount");
        var isEnabled = _config.GetValue<bool>("IsFeatureEnabled");
        var timeout = _config.GetValue<double>("ConnectionTimeout");
    }
}
```

---

## 💡 Examples

### Feature Toggle
```csharp
public IActionResult ProcessCheckout()
{
    var useNewCheckout = _config.GetValue<bool>("UseNewCheckoutFlow");
    
    if (useNewCheckout)
        return ProcessNewCheckout();
    else
        return ProcessLegacyCheckout();
}
```

### A/B Testing
```csharp
public IActionResult GetHomePage()
{
    var variant = _config.GetValue<string>("HomePageVariant");
    
    return variant switch
    {
        "A" => View("HomePageA"),
        "B" => View("HomePageB"),
        _ => View("HomePageDefault")
    };
}
```

### Rate Limiting
```csharp
public async Task<IActionResult> CallExternalApi()
{
    var maxRequests = _config.GetValue<int>("ApiRateLimitPerMinute");
    
    if (_rateLimiter.GetRequestCount() >= maxRequests)
        return StatusCode(429, "Rate limit exceeded");
    
    return await CallApi();
}
```

---


```


## 🚢 Deployment

### Docker Compose
```yaml
version: '3.8'

services:
  postgres:
    image: postgres:16
    environment:
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: ConfigurationDb
    ports:
      - "5432:5432"

  redis:
    image: redis:7
    ports:
      - "6379:6379"

  api:
    image: configreader-api:latest
    ports:
      - "5000:8080"
    environment:
      - ConnectionStrings__PostgreSQL=Host=postgres;...
      - ConnectionStrings__Redis=redis:6379
    depends_on:
      - postgres
      - redis
```
---

## 🧪 Testing (Not Started Yet)
```bash
# Run all tests
dotnet test

# Run specific project
dotnet test ConfigurationReader.Tests/
```

---

**Made by [Cevdet SEDEF](https://github.com/cevdettsedeff)**
