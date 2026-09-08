using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("user");

        builder.HasKey(user => user.Id);

        builder.Property
        (
            user => user.Username
        )
        .IsRequired()
        .HasMaxLength(30);

        builder.Property
        (
            user => user.Email
        )
        .IsRequired()
        .HasMaxLength(255);

        builder.HasIndex
        (
            user => user.Email
        )
        .IsUnique();

        builder.HasOne<Role>()
        .WithMany()
        .HasForeignKey(user => user.RoleId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Property
        (
            user => user.IsActive
        )
        .HasDefaultValue(true);

        builder.Property
        (
            user => user.EmailVerified
        )
        .HasDefaultValue(false);

        builder.Property
        (
            user => user.CreatedAt
        )
        .HasColumnType("timestamptz");

        builder.Property
        (
            user => user.UpdatedAt
        )
        .HasColumnType("timestamptz");
    }
}