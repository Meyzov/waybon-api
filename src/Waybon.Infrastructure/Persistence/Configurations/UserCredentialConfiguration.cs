using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        builder.ToTable("user_credential");

        builder.HasKey(uc => uc.Id);

        builder.HasIndex
        (
            uc => uc.UserId
        )
        .IsUnique();

        builder.HasOne<User>()
        .WithOne()
        .HasForeignKey<UserCredential>(uc => uc.UserId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.Property
        (
            uc => uc.PasswordHash
        )
        .IsRequired()
        .HasMaxLength(1024);

        builder.Property
        (
            uc => uc.FailedLoginAttempts
        )
        .HasDefaultValue(0);

        builder.Property
        (
            uc => uc.CreatedAt
        )
        .HasColumnType("timestamptz");

        builder.Property
        (
            uc => uc.UpdatedAt
        )
        .HasColumnType("timestamptz");
    }
}