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
```

## Running the API

The HTTP launch profile uses:

```text
http://localhost:5015
```

Swagger UI is available at:

```text
http://localhost:5015/swagger
```

## API Use Cases

### 1. Create an admin session

Use this when an administrator needs to perform admin-only actions, such as creating a new account.

`POST /api/sessions/admin`

```json
{
  "systemPassword": "your_password_here",
  "accountNumber": "acc-1"
}
```

Successful response:

```json
{
  "sessionId": "admin-session-guid"
}
```

The returned `sessionId` is used as `adminSessionId` in admin-only requests.

### 2. Create an account

Requires an admin session.

`POST /api/accounts/create`

```json
{
  "adminSessionId": "admin-session-guid",
  "accountNumber": "acc-1",
  "pin": "1234",
  "initialBalance": 100
}
```

Successful response:

```json
{
  "message": "Account created"
}
```

### 3. Create a user session

Use this after an account has been created. The user signs in with the account number and PIN.

`POST /api/sessions/user`

```json
{
  "accountNumber": "acc-1",
  "pin": "1234"
}
```

Successful response:

```json
{
  "sessionId": "user-session-guid"
}
```

The returned `sessionId` is used as the user session identifier for balance, deposit, withdrawal, and history operations.

### 4. View balance

Requires a user session.

`GET /api/accounts/balance?sessionId=user-session-guid`

Successful response:

```json
{
  "balance": 100
}
```

### 5. Deposit money

Requires a user session.

`POST /api/accounts/deposit`

```json
{
  "userSessionId": "user-session-guid",
  "amount": 50
}
```

Successful response:

```json
{
  "balance": 150
}
```

### 6. Withdraw money

Requires a user session.

`POST /api/accounts/withdraw`

```json
{
  "userSessionId": "user-session-guid",
  "amount": 30
}
```

Successful response:

```json
{
  "balance": 120
}
```

### 7. View transaction history

Requires a user session.

`GET /api/accounts/history?sessionId=user-session-guid`

Successful response:

```json
{
  "transactions": [
    {
      "type": "Deposit",
      "amount": 50,
      "balanceAfter": 150
    },
    {
      "type": "Withdraw",
      "amount": 30,
      "balanceAfter": 120
    }
  ]
}
```

## Authentication and Authorization Model

The project currently uses application-level sessions instead of ASP.NET Core JWT or cookie authentication.

- Admin authentication is performed with `SystemPassword`.
- User authentication is performed with `AccountNumber` and `PIN`.
- Authorization is performed by checking the session type: `Admin` or `User`.
- Admin-only operations require an admin session.
- Account operations require a user session.