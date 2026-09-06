using Atm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Atm.Infrastructure.Tests.Persistence;

internal static class ModelTestContext
{
    public static AtmDbContext Create()
    {
        return new AtmDbContext(new DbContextOptionsBuilder<AtmDbContext>()
            .UseNpgsql()
            .Options);
    }
}
