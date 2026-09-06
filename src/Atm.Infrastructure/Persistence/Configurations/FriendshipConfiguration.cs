using Atm.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

public sealed class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.ToTable("friendships", table =>
        {
            table.HasCheckConstraint("ck_friendships_distinct_users", "user_id <> friend_user_id");
            table.HasCheckConstraint("ck_friendships_canonical_order", "user_id < friend_user_id");
        });
        builder.HasKey(friendship => new { friendship.UserId, friendship.FriendUserId }).HasName("pk_friendships");
        builder.Property(friendship => friendship.UserId).HasColumnName("user_id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(friendship => friendship.FriendUserId).HasColumnName("friend_user_id").HasColumnType("uuid").ValueGeneratedNever();
        builder.Property(friendship => friendship.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone");
        builder.HasOne<User>().WithMany().HasForeignKey(friendship => friendship.UserId)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_friendships_users_user_id");
        builder.HasOne<User>().WithMany().HasForeignKey(friendship => friendship.FriendUserId)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_friendships_users_friend_user_id");
        builder.HasIndex(friendship => friendship.FriendUserId).HasDatabaseName("ix_friendships_friend_user_id");
    }
}
