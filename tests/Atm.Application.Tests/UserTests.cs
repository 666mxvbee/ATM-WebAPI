using Atm.Domain.Users;
using Atm.Domain.Users.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Application.Tests;

public sealed class UserTests
{
    private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 9, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ValidProfileIsCreatedAndUpdated()
    {
        User user = CreateUser();
        Assert.Equal("Test User", user.Name);
        Assert.IsType<ProfileUpdateResult.Success>(
            user.UpdateProfile(" Changed ", 30, UserGender.Other, HairColor.Black, Now.AddMinutes(1)));
        Assert.Equal("Changed", user.Name);
        Assert.Equal(Now.AddMinutes(1), user.UpdatedAtUtc);
        Assert.Equal(Now, user.CreatedAtUtc);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(151)]
    public void InvalidProfileDoesNotPartiallyMutateState(short age)
    {
        User user = CreateUser();
        Assert.IsType<ProfileUpdateResult.Failure>(
            user.UpdateProfile("Changed", age, UserGender.Other, HairColor.Black, Now.AddMinutes(1)));
        Assert.Equal("Test User", user.Name);
        Assert.Equal((short)25, user.Age);
        Assert.Equal(HairColor.Unspecified, user.HairColor);
        Assert.Equal(Now, user.UpdatedAtUtc);
    }

    [Fact]
    public void InvalidIdentifiersEnumsAndTimestampsAreRejected()
    {
        var login = new UserLogin("user");
        Assert.IsType<UserCreationResult.Failure>(
            User.Create(Guid.Empty, login, "Name", 25, UserGender.Unspecified, HairColor.Unspecified, Now));
        Assert.IsType<UserCreationResult.Failure>(
            User.Create(Guid.NewGuid(), login, "Name", 25, (UserGender)99, HairColor.Unspecified, Now));
        User user = CreateUser();
        Assert.IsType<ProfileUpdateResult.Failure>(
            user.UpdateProfile("Name", 25, UserGender.Unspecified, HairColor.Unspecified, Now.AddSeconds(-1)));
        Assert.IsType<ProfileUpdateResult.Failure>(
            user.UpdateProfile("Name", 25, UserGender.Unspecified, HairColor.Unspecified, Now.ToOffset(TimeSpan.FromHours(3))));
    }

    [Fact]
    public void FriendshipIsCanonicalAndRejectsSelfReference()
    {
        var first = Guid.Parse("00000100-0000-0000-0000-000000000001");
        var second = Guid.Parse("000000ff-ffff-ffff-ffff-ffffffffffff");
        Friendship forward = Assert.IsType<FriendshipCreationResult.Success>(
            Friendship.Create(first, second, Now)).Friendship;
        Friendship reverse = Assert.IsType<FriendshipCreationResult.Success>(
            Friendship.Create(second, first, Now)).Friendship;
        Assert.Equal(second, forward.UserId);
        Assert.Equal(first, forward.FriendUserId);
        Assert.Equal(forward.UserId, reverse.UserId);
        Assert.Equal(forward.FriendUserId, reverse.FriendUserId);
        Assert.IsType<FriendshipCreationResult.Failure>(Friendship.Create(first, first, Now));
        Assert.IsType<FriendshipCreationResult.Failure>(Friendship.Create(first, Guid.Empty, Now));
    }

    private static User CreateUser()
    {
        return Assert.IsType<UserCreationResult.Success>(
            User.Create(Guid.NewGuid(), new UserLogin("user"), " Test User ", 25,
                UserGender.Unspecified, HairColor.Unspecified, Now)).User;
    }
}
