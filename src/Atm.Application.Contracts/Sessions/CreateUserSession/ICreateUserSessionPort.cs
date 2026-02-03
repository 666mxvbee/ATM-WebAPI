namespace Atm.Application.Contracts.Sessions.CreateUserSession;

public interface ICreateUserSessionPort
{
    CreateUserSessionResult Execute(CreateUserSessionRequest request);
}