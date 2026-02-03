using Atm.Application.Abstractions.Repositories;

namespace Atm.Presentation.Common;

public sealed class SystemPasswordProvider : ISystemPasswordProvider
{
    private readonly IConfiguration _config;

    public SystemPasswordProvider(IConfiguration config)
    {
        _config = config;
    }

    public string GetSystemPassword()
    {
        IConfigurationSection section = _config.GetSection("SystemAuth");
        string? password = section["SystemPassword"];

        if (password is null)
        {
            return string.Empty;
        }

        return password;
    }
}