namespace Atm.Domain.Users.Results;

public abstract record FriendshipCreationResult
{
    private FriendshipCreationResult() { }

    public sealed record Success(Friendship Friendship) : FriendshipCreationResult;

    public sealed record Failure(string ErrorMessage) : FriendshipCreationResult;
}
