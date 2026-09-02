using Atm.Domain.Common;
using Atm.Domain.Users.Results;
using Atm.Domain.ValueObjects;

namespace Atm.Domain.Users;

public sealed class User
{
    // Public construction is validated by Create; private setters also allow ORM materialization.
    private User() { }

    public Guid Id { get; private set; }
    public UserLogin Login { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public short Age { get; private set; }
    public UserGender Gender { get; private set; }
    public HairColor HairColor { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static UserCreationResult Create(
        Guid id, UserLogin? login, string? name, short age,
        UserGender gender, HairColor hairColor, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty || login is null)
            return new UserCreationResult.Failure("User ID and login are required.");

        string? error = ValidateProfile(name, age, gender, hairColor);
        if (error is not null)
            return new UserCreationResult.Failure(error);

        if (!DomainRules.IsUtc(createdAtUtc))
            return new UserCreationResult.Failure("Created timestamp must be UTC.");

        return new UserCreationResult.Success(new User
        {
            Id = id,
            Login = login,
            Name = name!.Trim(),
            Age = age,
            Gender = gender,
            HairColor = hairColor,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
        });
    }

    public ProfileUpdateResult UpdateProfile(
        string? name, short age, UserGender gender,
        HairColor hairColor, DateTimeOffset updatedAtUtc)
    {
        string? error = ValidateProfile(name, age, gender, hairColor);
        if (error is not null)
            return new ProfileUpdateResult.Failure(error);

        if (!DomainRules.IsUtc(updatedAtUtc) || updatedAtUtc < UpdatedAtUtc)
            return new ProfileUpdateResult.Failure("Updated timestamp must be UTC and cannot move backwards.");

        Name = name!.Trim();
        Age = age;
        Gender = gender;
        HairColor = hairColor;
        UpdatedAtUtc = updatedAtUtc;
        return new ProfileUpdateResult.Success();
    }

    private static string? ValidateProfile(string? name, short age, UserGender gender, HairColor hairColor)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
            return "Name must be non-empty and cannot exceed 200 characters.";
        if (age is < 0 or > 150)
            return "Age must be between zero and 150.";
        if (!Enum.IsDefined(gender) || !Enum.IsDefined(hairColor))
            return "Unsupported gender or hair color.";
        return null;
    }
}
