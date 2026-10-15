using Atm.Application.Abstractions.Persistence;
using Atm.Application.Contracts.Admins;
using Atm.Application.Contracts.Admins.Models;

namespace Atm.HighLoad.Application.Services;

public sealed class AdminService : IAdminService
{
    private readonly IPersistenceContext _context;

    public AdminService(IPersistenceContext context)
    {
        _context = context;
    }


    public async Task<CreateAccount.Response> CreateAccountAsync(
        CreateAccount.Request request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}