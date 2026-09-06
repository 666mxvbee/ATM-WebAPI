using Atm.Domain.Accounts;
using Atm.Domain.Accounts.Results;
using Atm.Domain.Operations;
using Atm.Domain.Operations.Results;
using Atm.Domain.Sessions;
using Atm.Domain.Sessions.Results;
using Atm.Domain.Users;
using Atm.Domain.Users.Results;
using Atm.Domain.ValueObjects;
using Atm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Atm.Infrastructure.Tests.Persistence;

public sealed class MaterializationTests
{
    [Fact]
    public void EfCanRestorePrivateConstructorsAndImmutableValueObjects()
    {
        using AtmDbContext context = ModelTestContext.Create();
        AddSampleEntities(context);
        VerifyMaterialization(context);
    }

    private static void AddSampleEntities(AtmDbContext context)
    {
        var now = new DateTimeOffset(2026, 9, 6, 12, 0, 0, TimeSpan.Zero);
        User user = Assert.IsType<UserCreationResult.Success>(User.Create(
            Guid.NewGuid(), new UserLogin("Alice"), "Alice", 30,
            UserGender.Unspecified, HairColor.Unspecified, now)).User;
        Account account = Assert.IsType<AccountCreationResult.Success>(Account.Create(
            Guid.NewGuid(), new AccountNumber("account-1"), user.Id,
            new PinHash("test-hash"), new Money(10m), now)).Account;
        var amount = new Amount(1m);
        BankTransaction transaction = Assert.IsType<BankTransactionCreationResult.Success>(BankTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), BankTransactionType.Deposit, null, account.Id,
            amount, new CommissionSnapshot(amount, 0m, 0m), now)).Transaction;
        context.Add(user);
        context.Add(account);
        context.Add(transaction);
        context.Add(Assert.IsType<FriendshipCreationResult.Success>(
            Friendship.Create(user.Id, Guid.NewGuid(), now)).Friendship);
        context.Add(Assert.IsType<SessionCreationResult.Success>(
            Session.Create(Guid.NewGuid(), SessionType.User, account.Id, now, now.AddMinutes(30))).Session);
        context.Add(Assert.IsType<AccountHistoryEntryCreationResult.Success>(
            AccountHistoryEntry.Create(Guid.NewGuid(), transaction, account.Id, account.Balance)).Entry);
        context.ChangeTracker.DetectChanges();
    }

    private static void VerifyMaterialization(AtmDbContext context)
    {
        IEntityMaterializerSource materializer = context.GetService<IEntityMaterializerSource>();
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in context.ChangeTracker.Entries())
        {
            Microsoft.EntityFrameworkCore.Metadata.IProperty[] properties = entry.Metadata.GetProperties().ToArray();
            object[] values = new object[properties.Length];
            foreach (Microsoft.EntityFrameworkCore.Metadata.IProperty? property in properties)
                values[property.GetIndex()] = entry.Property(property.Name).CurrentValue!;

            object restored = materializer.GetMaterializer(entry.Metadata)(
                new MaterializationContext(new ValueBuffer(values), context));
            Assert.Equal(entry.Entity.GetType(), restored.GetType());
            foreach (Microsoft.EntityFrameworkCore.Metadata.IProperty? property in properties.Where(property => !property.IsShadowProperty()))
            {
                object? value = property.PropertyInfo!.GetValue(restored);
                Assert.True(property.GetValueComparer()!.Equals(entry.Property(property.Name).CurrentValue, value));
            }
        }
    }
}
