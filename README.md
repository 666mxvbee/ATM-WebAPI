# ATM [Hexagonal Architecture]

Implementation of an ATM system with a Web API interface using ASP.NET Controllers and in-memory storage.

## Functionality

- creating a user session using AccountNumber + PIN
- creating an administrator session using SystemPassword + AccountNumber
- creating an account, available only through an admin session
- viewing the account balance
- depositing money into an account
- withdrawing money
- viewing transaction history

Any account-related operation creates a record in the transaction history.

## Architecture

The project follows the hexagonal architecture approach:

- **Domain** — domain model and value objects
- **Application** — business logic / use cases through ports
- **Application.Abstractions** — abstractions such as repositories, providers, and similar components
- **Application.Contracts** — request/response contracts and operation results
- **Infrastructure** — implementations of abstractions, including in-memory repositories, and DI registration
- **Presentation** — ASP.NET Web API Controllers and HTTP-to-port mapping

The layers are connected using `Microsoft.Extensions.DependencyInjection`.

Each module provides extension methods for dependency registration:

- `builder.Services.AddApplication();`
- `builder.Services.AddInfrastructure();`

## System Password Configuration

The system password is read through `ISystemPasswordProvider`, implemented in `Atm.Presentation.Common.SystemPasswordProvider`, and is taken from the configuration.

`appsettings.json`:

```json
{
  "SystemAuth": {
    "SystemPassword": "your_password_here"
  }
}
