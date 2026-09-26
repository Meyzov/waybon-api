using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ===================================
        // Table
        // ===================================

        builder.ToTable("user");


        // ===================================
        // Id
        // ===================================

        builder.HasKey(user => user.Id);


        // ===================================
        // Username
        // ===================================

        builder.Property
        (
            user => user.Username
        )
        .IsRequired()
        .HasMaxLength(User.UsernameMaxLength);


        // ===================================
        // Email
        // ===================================

        builder.Property
        (
            user => user.Email
        )
        .IsRequired()
        .HasMaxLength(User.EmailMaxLength);

        builder.HasIndex
        (
            user => user.Email
        )
        .IsUnique();


        // ===================================
        // RoleId
        // ===================================

        builder.HasOne<Role>()
        .WithMany()
        .HasForeignKey(user => user.RoleId)
        .OnDelete(DeleteBehavior.Restrict);


        // ===================================
        // IsActive
        // ===================================

        builder.Property
        (
            user => user.IsActive
        )
        .HasDefaultValue(true)
        .HasSentinel(true);


        // ===================================
        // EmailVerified
        // ===================================

        builder.Property
        (
            user => user.EmailVerified
        )
        .HasDefaultValue(false);
    }
}