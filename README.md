# MicroERP

![CI](https://github.com/Nerso1/MicroERP/actions/workflows/ci.yml/badge.svg)
![CodeQL](https://github.com/Nerso1/MicroERP/actions/workflows/codeql.yml/badge.svg)

Warehouse & inventory REST API built with ASP.NET Core 10 and Clean Architecture.

A portfolio project focused on code quality and Microsoft standards rather than feature count:
layered architecture, CQRS-style request handling, validation pipeline, RFC 7807 error responses,
unit + integration tests against a real SQL Server, Docker and CI.

## Stack

| Area | Technology |
|---|---|
| Framework | ASP.NET Core 10 |
| ORM | EF Core 10 (Code First, migrations, Fluent API) |
| Database | SQL Server 2022 |
| Request handling | MediatR (Command/Query + Handler) |
| Validation | FluentValidation (MediatR pipeline behavior) |
| Messaging | Azure Service Bus (low-stock alerts) |
| API docs | Swagger / OpenAPI (Swashbuckle, XML comments) |
| Unit tests | xUnit, Moq, FluentAssertions |
| Integration tests | WebApplicationFactory, Testcontainers (SQL Server) |
| DevOps | Docker, docker compose, GitHub Actions, CodeQL, Dependabot |

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) - required to run the app in containers **and** to run integration tests
- [.NET 10 SDK](https://dotnet.microsoft.com/download) - only needed for local development and tests

### Option A - run everything in Docker (no .NET SDK needed)

```bash
git clone https://github.com/Nerso1/MicroERP.git
cd MicroERP
cp .env.example .env
docker compose up --build
```

Open **http://localhost:8080/swagger**.

The API waits for SQL Server to pass its health check, then applies EF Core migrations
(including seed data) on startup. Host ports can be changed in `.env` (`API_PORT`, `DB_PORT`).

Stop with `Ctrl+C`; remove containers with `docker compose down` (add `-v` to also delete the database volume).

### Option B - local development (`dotnet run`)

```bash
git clone https://github.com/Nerso1/MicroERP.git
cp .env.example .env
docker compose up -d db                     # Docker for SQL Server only
dotnet tool install --global dotnet-ef      # once
dotnet ef database update --project src/MicroERP.Infrastructure --startup-project src/MicroERP.Api
dotnet run --project src/MicroERP.Api --launch-profile https
```

Open **https://localhost:5101/swagger**.

Optional - send low-stock alerts to a real Azure Service Bus queue (without it alerts are only logged):

```bash
dotnet user-secrets set "AzureServiceBus:ConnectionString" "<connection-string>" --project src/MicroERP.Api

### Running tests

```bash
dotnet test
```

Integration tests start their own SQL Server container via Testcontainers - Docker Desktop must be running.

### API endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/products?page=1&pageSize=10&search=` | Paginated product list with optional search |
| `GET` | `/api/products/{id}` | Single product |
| `POST` | `/api/products` | Create product -> `201 Created` |
| `PUT` | `/api/products/{id}` | Update product -> `204 No Content` |
| `DELETE` | `/api/products/{id}` | Delete product -> `204 No Content` |
| `GET` | `/api/products/{id}/stock` | Stock levels of a product across warehouses |
| `POST` | `/api/products/{id}/stock/adjustments` | Change stock in one warehouse by `quantityChange` (+/-) -> `200` with the new quantity |

Errors are returned as `application/problem+json` (RFC 7807):
`400` validation errors, `404` not found, `409` duplicate SKU / insufficient stock / concurrent update, `500` unexpected error.

## Architecture

### Project dependencies

Dependencies point inwards - `Domain` knows nothing about EF Core, MediatR or ASP.NET Core.

```mermaid
flowchart TD
    Api["<b>MicroERP.Api</b><br/>Controllers, middleware, Program.cs"]
    Infrastructure["<b>MicroERP.Infrastructure</b><br/>AppDbContext, Fluent API configurations, migrations, Service Bus publisher"]
    Application["<b>MicroERP.Application</b><br/>Commands, queries, handlers, validators, DTOs, IAppDbContext, IStockAlertPublisher"]
    Domain["<b>MicroERP.Domain</b><br/>Entities, domain exceptions"]
    Database[("SQL Server")]
    ServiceBus[("Azure Service Bus<br/>queue: stock-alerts")]
    Infrastructure -. "EF Core" .-> Database
    Infrastructure -. "low-stock alerts" .-> ServiceBus

    Api --> Application
    Api --> Infrastructure
    Infrastructure --> Application
    Infrastructure --> Domain
    Application --> Domain

    UnitTests["MicroERP.UnitTests"] -.-> Application
    IntegrationTests["MicroERP.IntegrationTests"] -.-> Api
```

`Application` defines the `IAppDbContext` interface; `Infrastructure` implements it with EF Core.
The dependency is inverted, so handlers can be unit-tested with a mocked context.

### Request flow

```mermaid
sequenceDiagram
    participant C as Client
    participant M as UseExceptionHandler / GlobalExceptionHandler
    participant Ctrl as ProductsController
    participant V as ValidationBehavior
    participant H as Handler
    participant DB as AppDbContext / SQL Server

    C->>M: HTTP request
    M->>Ctrl: next()
    Ctrl->>V: mediator.Send(command)
    V->>V: run FluentValidation validators
    V->>H: next()
    H->>DB: query / SaveChangesAsync
    DB-->>H: result
    H-->>Ctrl: result
    Ctrl-->>C: 200 / 201 / 204
    Note over M: Exceptions are mapped to ProblemDetails<br/>(400 / 404 / 409 / 500)
```

### Data model

```mermaid
erDiagram
    PRODUCT ||--o{ STOCK_LEVEL : "stored as"
    WAREHOUSE ||--o{ STOCK_LEVEL : holds

    PRODUCT {
        int Id PK
        string Sku UK
        string Name
        decimal Price
        bool IsActive
    }
    WAREHOUSE {
        int Id PK
        string Name
        string Location
        bool IsActive
    }
    STOCK_LEVEL {
        int Id PK
        int ProductId FK
        int WarehouseId FK
        int Quantity
        rowversion RowVersion
    }
```

Tables live in two schemas: `Products.Products`, `Products.StockLevels`, `Settings.Warehouses`.
All entities implement `IAuditable` - `CreatedAt` / `UpdatedAt` are set automatically in `SaveChangesAsync`.

### Solution layout

```
src/
├── MicroERP.Domain/          # Entities, IAuditable, domain exceptions - no external dependencies
├── MicroERP.Application/     # CQRS commands/queries, handlers, validators, DTOs, IAppDbContext
├── MicroERP.Infrastructure/  # EF Core (AppDbContext, configurations, migrations), Service Bus publisher
└── MicroERP.Api/             # Controllers, exception middleware, Program.cs, Dockerfile
tests/
├── MicroERP.UnitTests/         # Handlers and validators with mocked IAppDbContext
└── MicroERP.IntegrationTests/  # Full HTTP flow against SQL Server in Testcontainers
```

## Design Decisions

**Monolith, not microservices.** One consistent domain (products and warehouses) in one database.
Splitting it into services just to use a message broker would add distributed-system complexity with no benefit at this scale.

**CQRS light.** MediatR organises code into Command/Query + Handler, one use case per class.
There are no separate read/write models or databases - this is a code-organisation pattern, not an architectural necessity here.

**`IAppDbContext` instead of Repository + Unit of Work.** `DbContext` already is a unit of work and `DbSet` already is a repository.
A thin interface exists only so handlers can be unit-tested with mocks.

**Validation in the MediatR pipeline.** A generic `ValidationBehavior` runs FluentValidation validators before every handler,
so controllers and handlers contain no validation code. Fluent API (not Data Annotations) keeps `Domain` free of EF Core attributes.

**One place for error handling.** `GlobalExceptionHandler` (`IExceptionHandler` + `AddProblemDetails`) maps exceptions to RFC 7807 `ProblemDetails`,
so every error response - thrown or returned by a controller - has the same shape: `type`, `title`, `status`, `detail`, `traceId`.
A duplicate SKU is enforced by a unique index in the database and surfaces as `409 Conflict`.

**Manual pagination instead of OData.** `PagedResult<T>` with `Skip`/`Take` on `IQueryable` - OData would be too heavy for a handful of tables.

**Optimistic concurrency on stock.** `StockLevel.RowVersion` is a SQL Server `rowversion` column. Two concurrent adjustments of the same stock level
cannot both succeed - the second one gets `409 Conflict` instead of silently overwriting the first.

**Low-stock alerts through Azure Service Bus.** When a stock adjustment drops the quantity below 10, the API publishes a `StockLowAlertMessage`
to the `stock-alerts` queue. The alert is sent only when the threshold is crossed (12 -> 9, not again for 9 -> 7).
`Application` only knows the `IStockAlertPublisher` interface; the Service Bus implementation lives in `Infrastructure`,
and without a configured connection string a logging implementation is used, so the project runs without an Azure account.
The alert is published after the stock change is saved, and a publishing failure is logged instead of failing the request
(a retried request would adjust the stock twice). Known limitation: an alert can be lost if the process stops between saving and publishing -
the full solution would be a transactional outbox, which is out of scope here.

**Integration tests on real SQL Server (Testcontainers), not SQLite or InMemory.** SQLite does not support `rowversion`,
behaves differently with `decimal` and text comparison, and does not test migrations. With Testcontainers only the connection string is replaced -
the same provider and the same migrations as in production.

**Migrations on startup - opt-in.** Enabled with `Database:MigrateOnStartup=true` (set in `docker-compose.yml`), disabled by default.
Convenient for a one-command demo; in a cloud deployment migrations will run as a separate pipeline step.

**Authentication deferred to Stage 3.** A deliberate choice, not an omission. Dependencies flow through interfaces,
so adding ASP.NET Identity + JWT means changing `AppDbContext` and `Program.cs` without touching business logic.

**Secrets.** The SA password for local Docker lives in a git-ignored `.env` file (`.env.example` is committed).
Cloud deployment (Stage 2) will use App Service settings / Key Vault instead.

## Roadmap

- [x] **Stage 1 - Foundation:** Clean Architecture, EF Core, CQRS, validation, tests, Docker, CI
- [ ] **Stage 2 - Azure:** Service Bus (low-stock alerts), Azure Function, App Service + Azure SQL deployment
- [ ] **Stage 3 - Auth:** ASP.NET Identity + JWT
- [ ] **Stage 4 - Extras:** stock movements with transactions, Serilog, OpenTelemetry, rate limiting, CORS

## License

[MIT](LICENSE)
