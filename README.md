# VeritasVault Platform

> A scalable, modular, institution-grade DeFi platform for storing, querying, and archiving diverse market data objects, optimized for derivative pricing and market tracking.

Built on Azure services with a .NET 9 backend, following Clean/Hexagonal Architecture principles.

---

## 📚 Project Overview

VeritasVault stores various types of market data — spot prices, forward curves, yield curves, and volatility surfaces — in Azure Cosmos DB for hot storage, with long-term archival to Azure Data Lake.

### Phased Delivery

- **Phase 1:** Temporal Data (Official, Intraday) — CosmosDB storage *(In Progress)*
- **Phase 2:** Live Data (Streaming, Latest Value Only) — CosmosDB optimized container
- **Phase 3:** Archival to Azure Data Lake — cold storage for historical analytics

---

## 🏗 Architecture

The platform follows **Clean/Hexagonal/Onion Architecture** principles with clear separation of concerns:

```
┌───────────────────────────────────────────────────────────┐
│                    Presentation Layer                     │
│               (vv.Api, vv.Functions)                      │
└──────────────────────────┬────────────────────────────────┘
                           │
                           ▼
┌───────────────────────────────────────────────────────────┐
│                   Application Layer                       │
│                   (vv.Application)                        │
└───────────────┬───────────────────────────┬───────────────┘
                │                           │
                ▼                           ▼
┌───────────────────────────┐   ┌───────────────────────────┐
│        Domain Layer       │◄──│    Infrastructure Layer   │
│   (vv.Domain, vv.Core,    │   │     (vv.Infrastructure,   │
│        vv.Data)           │   │         vv.Data)          │
└───────────────────────────┘   └───────────────────────────┘
```

---

## 🗂 Project Structure

```
vv/
├── src/
│   ├── vv.Api/              # ASP.NET Core Web API controllers and services
│   ├── vv.Application/      # Application services, commands, queries, handlers
│   ├── vv.Core/             # Cross-cutting concerns, configuration, validation
│   ├── vv.Data/             # Data access utilities and repository helpers
│   ├── vv.Domain/           # Domain entities, value objects, interfaces (ports)
│   ├── vv.Functions/        # Azure Functions for background processing
│   └── vv.Infrastructure/   # External service implementations (Cosmos DB, etc.)
├── tests/
│   ├── vv.Api.Tests/        # API layer tests
│   ├── vv.Application.Tests/# Application layer tests
│   └── vv.Infrastructure.Tests/ # Infrastructure layer tests
├── docs/                    # Technical documentation
├── scripts/                 # Build and utility scripts
└── vv.Platform.sln          # Visual Studio solution file
```

---

## 🔧 Technology Stack

| Component | Technology |
|:----------|:-----------|
| Runtime | .NET 9.0 |
| Web API | ASP.NET Core |
| Serverless | Azure Functions |
| Database | Azure Cosmos DB (Core SQL API) |
| Messaging | Azure Event Grid |
| Resilience | Polly |
| CQRS/Mediator | MediatR |
| Code Quality | Prettier, Husky |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download) or later
- [Node.js 18+](https://nodejs.org/) (for tooling)
- [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) (for local development)

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/JustAGhosT/vv.git
   cd vv
   ```

2. Install Node.js dependencies (for linting/formatting):
   ```bash
   npm install
   ```

3. Restore .NET packages and build:
   ```bash
   dotnet restore
   dotnet build
   ```

4. Run tests:
   ```bash
   dotnet test
   ```

---

## 🧪 Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/vv.Infrastructure.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Code Formatting

```bash
# Format all files
npm run lint
```

---

## 📦 Domain Model

### Market Data Types

| Entity | Description |
|:-------|:------------|
| `FxSpotPriceData` | Foreign exchange spot price data |
| `FxVolSurfaceData` | FX volatility surface data |
| `CryptoSpotPriceData` | Cryptocurrency spot price data |
| `CryptoOrdinalSpotPriceData` | Cryptocurrency ordinal spot price data |
| `CryptoOrderBookData` | Cryptocurrency order book data |
| `CryptoPerpetualData` | Cryptocurrency perpetual contract data |

### Base Entity Properties

All market data entities inherit from `BaseMarketData` and include:

- `Id` - Computed unique identifier
- `AssetId` - Asset identifier (e.g., "EURUSD", "BTC")
- `AssetClass` - Classification (fx, crypto, equity, etc.)
- `DataType` - Type of data (price, volatility, etc.)
- `Region` - Geographic region
- `DocumentType` - Document classification (official, intraday, etc.)
- `AsOfDate` - Business date of the data
- `Version` - Entity version for optimistic concurrency
- `SchemaVersion` - Schema version for evolution

---

## 🏗 Azure Cosmos DB Design

| Design Choice | Details |
|:--------------|:--------|
| Database | Azure CosmosDB (Core SQL API) |
| Partition Key | `/assetId` |
| Containers | `marketdata-history` (temporal), `marketdata-live` (live) |
| ID Format | `<dataType>__<assetClass>__<assetId>__<region>__<date>__<documentType>__<schemaVersion>__<version>` |

---

## 📖 Documentation

- [Architecture Guide](docs/Architecture.md)
- [Domain Layer Documentation](src/vv.Domain/README.md)
- [CosmosDB Setup Guide](docs/Setup_CosmosDB.md)
- [Architecture Implementation Guide](docs/architecture-implementation-guide.md)

---

## ✨ Design Principles

- **Clean Architecture** — Domain at the center, dependencies point inward
- **CQRS** — Separate read and write paths for scalability
- **Domain-Driven Design** — Rich domain models with business logic
- **Immutability & Auditability** — Every entity is versioned and tracked
- **Defense-in-Depth** — Multi-layered security controls
- **Scalable by Design** — Support for GB–TB data volumes

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## 📄 License

This project is proprietary and unlicensed for public use.

---

## 📢 Status

🔵 **Phase 1 - In Progress**

