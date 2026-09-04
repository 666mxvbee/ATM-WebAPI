# ATM Web API

An ATM training project built with .NET 9, ASP.NET Core Controllers, and hexagonal architecture.

Database stage 1 is currently in progress. Microstages 13.1 and 13.2 are implemented; runtime repositories are still temporary in-memory adapters until the EF Core repositories replace them in microstage 13.9.

## Current functionality

- create expiring user and administrator sessions;
- create an account through an administrator session;
- view an account balance;
- deposit and withdraw money;
- view the legacy in-memory operation history.

The domain model now also contains persistent shapes for users, canonical friendships, accounts, sessions, bank transactions, commission snapshots, and immutable account history entries. Transactional persistence, idempotency enforcement, commissions in runtime use cases, and transfers are deliberately implemented in later microstages.

## Architecture

- **Atm.Domain** — entities, value objects, invariants, and domain operation results;
- **Atm.Application** — use-case orchestration;
- **Atm.Application.Abstractions** — repository, clock, security, and provider ports;
- **Atm.Application.Contracts** — input/output ports and application results;
- **Atm.Infrastructure** — temporary repositories plus infrastructure adapters;
- **Atm.Presentation** — HTTP controllers and composition root.

Domain and Application have no EF Core or PostgreSQL dependency.

## Validation and result style

Small value objects such as `AccountNumber`, `UserLogin`, `Money`, and `PinCode` validate themselves in constructors and reject invalid values with `ArgumentException`.

Business operations that can normally be accepted or rejected use a dedicated result type with nested `Success` and `Failure` records. The project intentionally does not introduce a universal `Result<T>` or one result hierarchy per primitive field.

## Money rules

Amounts use .NET `decimal` and allow at most four fractional digits. There is no artificial `numeric(19,4)` business ceiling: for example, values above `1_000_000_000_000_000` are valid.

This does not mean infinite precision. Values must still fit in `decimal`, and arithmetic is rejected when it would overflow or silently lose a small amount near the representable boundary. Future PostgreSQL mappings use unconstrained `numeric` with scale checks instead of `numeric(19,4)`.

Commission snapshots retain the applied rate, fixed component, and calculated commission. The formula is:

```text
round(amount * rate + fixed, 4, MidpointRounding.ToEven)
```

Runtime deposit and withdrawal use cases still have their legacy no-commission behavior until microstage 13.7 wires the new transaction model into one database transaction.

## PIN security

The account entity stores `PinHash`, never `PinCode`. `IPinHasher` is implemented using the versioned Microsoft ASP.NET Core `PasswordHasher`, which generates a unique salt and provides the matching verification routine.

Do not log request bodies containing PINs. JWT, cookies, and ASP.NET Core Identity user management are outside this stage.

## Clock and sessions

`IClock` supplies the current UTC time. Production uses `SystemClock`; tests use a controllable `FixedClock`. This keeps time-dependent domain behavior deterministic.

A user session references the stable account UUID. A system administrator session is not associated with an account. Sessions have UTC creation, expiration, and optional revocation timestamps. They are active from creation inclusively until expiration exclusively, unless revoked first.

Session lifetimes are validated at startup:

```json
{
  "Sessions": {
    "UserLifetimeMinutes": 30,
    "AdminLifetimeMinutes": 15
  }
}
```

Both values must be positive.

## System password configuration

The administrator password comes from configuration through `ISystemPasswordProvider`:

```json
{
  "SystemAuth": {
    "SystemPassword": "use-a-secret-provider"
  }
}
```

Do not commit a real password. Override the development placeholder with user secrets or an environment variable.

## API examples

The local HTTP profile uses `http://localhost:5015`; Swagger is available at `http://localhost:5015/swagger` in Development.

### Create an administrator session

`POST /api/sessions/admin`

```json
{
  "systemPassword": "use-a-secret-provider",
  "accountNumber": "legacy-admin-context"
}
```

`accountNumber` remains required only for backward compatibility with the existing HTTP contract. A system administrator session does not store or authorize against this number.

### Create an account

`POST /api/accounts/create`

```json
{
  "adminSessionId": "00000000-0000-0000-0000-000000000001",
  "accountNumber": "acc-1",
  "pin": "1234",
  "initialBalance": 100,
  "ownerUserId": "00000000-0000-0000-0000-000000000002"
}
```

`ownerUserId` is a breaking addition to the old contract. In the current transitional in-memory implementation only a non-empty UUID is checked. Existence is enforced by the user repository and PostgreSQL foreign key in later persistence microstages. There is intentionally no user/profile HTTP CRUD endpoint in stage 1.

### Create a user session

`POST /api/sessions/user`

```json
{
  "accountNumber": "acc-1",
  "pin": "1234"
}
```

The returned `sessionId` authorizes balance, deposit, withdrawal, and history operations.

### Account operations

- `GET /api/accounts/balance?sessionId={user-session-id}`
- `POST /api/accounts/deposit` with `userSessionId` and `amount`
- `POST /api/accounts/withdraw` with `userSessionId` and `amount`
- `GET /api/accounts/history?sessionId={user-session-id}`

## Build and test

```powershell
dotnet restore Atm.sln --locked-mode
dotnet build Atm.sln -c Release --no-restore
dotnet test Atm.sln -c Release --no-build
```

Unit tests create mocks or focused test objects directly and do not build the production DI container or start a database.

## Transitional limitations

The following stage-1 work is not yet complete:

- `AtmDbContext`, mappings, migration, and PostgreSQL repositories;
- asynchronous persistence ports and cancellation tokens;
- atomic balance/transaction/history commits;
- database-backed idempotency and concurrency handling;
- runtime commission policies and transfers;
- PostgreSQL integration tests, production DI replacement, and CI environment.

Do not treat the current in-memory repositories as production persistence.
