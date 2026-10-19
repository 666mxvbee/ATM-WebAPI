using Atm.Application.Contracts.Admins.Models;

namespace Atm.Application.Contracts.Admins;

public interface IAdminService
{
    Task<CreateAccount.Response> CreateAccountAsync(
        CreateAccount.Request request,
        CancellationToken cancellationToken);
}