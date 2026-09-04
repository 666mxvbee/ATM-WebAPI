using Atm.Domain.Common;
using Atm.Domain.Users.Results;

namespace Atm.Domain.Users;

public sealed class Friendship
{
    private Friendship() { }

    public Guid UserId { get; private set; }
    public Guid FriendUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static FriendshipCreationResult Create(
        Guid firstUserId, Guid secondUserId, DateTimeOffset createdAtUtc)
    {
        if (firstUserId == Guid.Empty || secondUserId == Guid.Empty)
            return new FriendshipCreationResult.Failure("User IDs cannot be empty.");
        if (firstUserId == secondUserId)
            return new FriendshipCreationResult.Failure("A user cannot befriend themselves.");
        if (!DomainRules.IsUtc(createdAtUtc))
            return new FriendshipCreationResult.Failure("Created timestamp must be UTC.");

        bool firstIsSmaller = string.CompareOrdinal(firstUserId.ToString("N"), secondUserId.ToString("N")) < 0;
        return new FriendshipCreationResult.Success(new Friendship
        {
            UserId = firstIsSmaller ? firstUserId : secondUserId,
            FriendUserId = firstIsSmaller ? secondUserId : firstUserId,
            CreatedAtUtc = createdAtUtc,
        });
    }
}
