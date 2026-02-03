namespace Atm.Application.Contracts.Sessions.CreateAdminSession;

public interface ICreateAdminSessionPort
{
    CreateAdminSessionResult Execute(CreateAdminSessionRequest request);
}