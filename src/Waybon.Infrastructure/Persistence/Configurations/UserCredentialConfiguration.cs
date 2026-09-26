using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        // ===================================
        // Table
        // ===================================

        builder.ToTable("user_credential");


        // ===================================
        // Id
        // ===================================

        builder.HasKey(uc => uc.Id);


        // ===================================
        // UserId
        // ===================================

        builder.HasIndex
        (
            uc => uc.UserId
        )
        .IsUnique();

        builder.HasOne<User>()
        .WithOne()
        .HasForeignKey<UserCredential>(uc => uc.UserId)
        .OnDelete(DeleteBehavior.Cascade);


        // ===================================
        // PasswordHash
        // ===================================

        builder.Property
        (
            uc => uc.PasswordHash
        )
        .IsRequired()
        .HasMaxLength(UserCredential.PasswordHashMaxLength);


        // ===================================
        // FailedLoginAttempts
        // ===================================

        builder.Property
        (
            uc => uc.FailedLoginAttempts
        )
        .HasDefaultValue(0);
    }
}