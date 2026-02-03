# ATM [Hexagonal Architecture]

Реализация системы банкомата с Web API интерфейсом (ASP.NET Controllers) и in-memory хранилищем.

## Функциональность
- создание сессии пользователя (AccountNumber + PIN)
- создание сессии администратора (SystemPassword + AccountNumber)
- создание счёта (только через admin session)
- просмотр баланса
- пополнение счёта
- снятие денег
- просмотр истории операций

Любая операция со счётом создаёт запись в истории.

## Архитектура
Проект построен по гексагональному подходу:
- **Domain** — доменная модель и value objects
- **Application** — бизнес-логика/юзкейсы (порты)
- **Application.Abstractions** — абстракции (репозитории, провайдеры и т.п.)
- **Application.Contracts** — контракты запросов/ответов и результаты операций
- **Infrastructure** — реализации абстракций (in-memory репозитории), регистрация DI
- **Presentation** — ASP.NET Web API (Controllers), маппинг HTTP → порты

Связывание слоёв выполнено через `Microsoft.Extensions.DependencyInjection`.
Каждый модуль предоставляет методы расширения для регистрации зависимостей:
- `builder.Services.AddApplication();`
- `builder.Services.AddInfrastructure();`

## Конфигурация системного пароля
Системный пароль читается через `ISystemPasswordProvider` (реализация в `Atm.Presentation.Common.SystemPasswordProvider`)
и берётся из конфигурации:

`appsettings.json`:
```json
{
  "SystemAuth": {
    "SystemPassword": "your_password_here"
  }
}