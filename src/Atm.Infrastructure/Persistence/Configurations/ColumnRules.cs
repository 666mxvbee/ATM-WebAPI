using Atm.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

internal static class ColumnRules
{
    public static PropertyBuilder<Money> Money(PropertyBuilder<Money> property, string column)
    {
        property.HasConversion(value => value.Value, value => new Money(value));
        property.Metadata.SetValueComparer(new ValueComparer<Money>(
            (left, right) => left != null && right != null ? left.Value == right.Value : left == right,
            value => value.Value.GetHashCode(), value => value));
        return property.HasColumnName(column).HasColumnType("numeric").IsRequired();
    }

    public static PropertyBuilder<Amount> Amount(PropertyBuilder<Amount> property, string column)
    {
        property.HasConversion(value => value.Value, value => new Amount(value));
        property.Metadata.SetValueComparer(new ValueComparer<Amount>(
            (left, right) => left != null && right != null ? left.Value == right.Value : left == right,
            value => value.Value.GetHashCode(), value => value));
        return property.HasColumnName(column).HasColumnType("numeric").IsRequired();
    }

    public static string MonetaryCheck(string column, bool positive = false)
    {
        string comparison = positive ? ">" : ">=";
        return $"{column} {comparison} 0 AND {column} <= 79228162514264337593543950335 "
            + $"AND {column} = trunc({column}, 4) "
            + $"AND {column} * power(10::numeric, scale(trim_scale({column}))) <= 79228162514264337593543950335";
    }

    public static string NonEmptyUuid(string column)
    {
        return $"{column} <> '00000000-0000-0000-0000-000000000000'::uuid";
    }
}
