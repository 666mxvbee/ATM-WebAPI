# ATM Web API

[![.NET CI](https://github.com/666mxvbee/ATM-WebAPI/actions/workflows/dotnet.yml/badge.svg)](https://github.com/666mxvbee/ATM-WebAPI/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/github/license/666mxvbee/ATM-WebAPI)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)

ATM API built with .NET 10 and ASP.NET Core controllers. The project separates application logic, domain models, HTTP contracts, and persistence adapters using a `hexagonal architecture`.

## Features

- Create administrator sessions using a configurable system password.
- Open accounts through an administrator session.
- Create user sessions using an account number and PIN.
- Retrieve balances, deposit money, and withdraw money.
- Retrieve operation history with amounts and balances after each operation.
- Explore and call the API through Swagger UI.

## Project structure

| Project | Responsibility |
| --- | --- |
| `Atm.Application.Models` | Accounts, sessions, operations, value objects, and domain results |
| `Atm.Application.Contracts` | Service interfaces, requests, responses, and DTOs |
| `Atm.Application.Abstractions` | Repository interfaces, persistence context, and transaction abstractions |
| `Atm.Application` | Application services, DTO mappings, and session options |
| `Atm.Infrastructure.Persistence` | In-memory repositories, ID generation, and persistence context implementation |
| `Atm.Presentation` | Controllers, HTTP request models, and HTTP response mapping |
| `Atm.Api` | Application entry point, configuration, dependency registration, and Swagger |

Application services access storage through interfaces. In-memory repositories are registered as singletons so their data survives between HTTP requests within the same process. Account and operation IDs are numeric; session keys are GUIDs.

## Run locally

Prerequisite: a .NET 10 SDK that supports the project's package dependencies.

Run these commands from the repository root:

```powershell
dotnet restore Atm.slnx
dotnet build Atm.slnx -c Release --no-restore
dotnet run --project src/Atm.Api --launch-profile http
```

The HTTP launch profile uses:

- API: `http://localhost:5140`
- Swagger UI: `http://localhost:5140/swagger`

Stop and restart the application to reset the in-memory data. Accounts, sessions, and operation history are not persisted to disk.

## Configuration

The administrator password is bound to `SessionOptions` and validated at startup. It must contain at least five characters.

The checked-in `src/Atm.Api/appsettings.Development.json` contains this local development configuration:

```json
{
  "Session": {
    "SystemPassword": "admin"
  }
}
```

The `http` launch profile selects the `Development` environment, which loads this file. To override the password without editing it, set an environment variable before starting the application:

```powershell
$env:Session__SystemPassword = "your-local-password"
dotnet run --project src/Atm.Api --launch-profile http
```

The value `admin` is a development placeholder, not a production credential. Do not commit real passwords.

## API workflow

Administrator sessions authorize account creation. User sessions authorize operations on the account linked to that session. Use the appropriate returned `sessionId` in subsequent requests.

### 1. Create an administrator session

`POST /api/sessions/admin`

```json
{
  "systemPassword": "admin"
}
```

The response contains an administrator `sessionId`. If you overrode the configured password, use that value instead.

### 2. Create an account

`POST /api/admin/accounts`

```json
{
  "sessionId": "<administrator-session-id>",
  "accountNumber": "12345678901234567890",
  "pinCode": "1111"
}
```

Replace the placeholder with the actual GUID. Choose an unused 20-digit account number and a four-digit PIN. Account numbers are supplied by the caller; internal account IDs are assigned by the repository. New accounts start with a zero balance.

### 3. Create a user session

`POST /api/sessions/user`

```json
{
  "accountNumber": "12345678901234567890",
  "pinCode": "1111"
}
```

The response contains a user `sessionId`. Use this key, rather than the administrator session key, for account operations.

### 4. Deposit and withdraw

`POST /api/accounts/deposit`

```json
{
  "sessionId": "<user-session-id>",
  "money": 1000
}
```

`POST /api/accounts/withdraw`

```json
{
  "sessionId": "<user-session-id>",
  "money": 200
}
```

Both operations return the resulting balance. After these requests, it should be `800`. Deposit and withdrawal amounts must be greater than zero; withdrawals exceeding the available balance are rejected.

### 5. Read the balance and history

Send the user session key as a query parameter, without a request body:

```http
GET /api/accounts/balance?sessionId=<user-session-id>
GET /api/accounts/history?sessionId=<user-session-id>
```

History is returned as a JSON array. Each record includes `operationId`, `accountId`, `time`, `operationType`, `amount`, and `balanceAfter`. Balance inquiries also create history records, with an amount of zero. An account with no history returns an empty array.

## Validation and responses

HTTP request models validate account number and PIN formats. Application services return scenario-specific `Success` and `Failure` responses, which controllers map to HTTP results:

- `200 OK` for successful requests.
- `400 Bad Request` for invalid HTTP models and account or administrator operation failures.
- `401 Unauthorized` for session-creation failures.

Authorization failures in account and administrator operations are not yet represented separately from other failures.

## License

Licensed under the [Apache License 2.0](LICENSE).