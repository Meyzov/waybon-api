using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
{
    // ===================================
    // Constants
    // ===================================

    private const string TableName = "user_credential";


    // ===================================
    // Configure
    // ===================================

    public void Configure(EntityTypeBuilder<UserCredential> builder)
    {
        // ===================================
        // Table
        // ===================================

        builder.ToTable(TableName);


        // ===================================
        // Id
        // ===================================

        builder.HasKey(credential => credential.Id);


        // ===================================
        // UserId
        // ===================================

        builder.HasIndex
        (
            credential => credential.UserId
        )
        .IsUnique();

        builder.HasOne<User>()
        .WithOne()
        .HasForeignKey<UserCredential>(credential => credential.UserId)
        .OnDelete(DeleteBehavior.Cascade);


        // ===================================
        // PasswordHash
        // ===================================

        builder.Property
        (
            credential => credential.PasswordHash
        )
        .IsRequired()
        .HasMaxLength(UserCredential.PasswordHashMaxLength);


        // ===================================
        // FailedLoginAttempts
        // ===================================

        builder.Property
        (
            credential => credential.FailedLoginAttempts
        )
        .HasDefaultValue(0);
    }
}