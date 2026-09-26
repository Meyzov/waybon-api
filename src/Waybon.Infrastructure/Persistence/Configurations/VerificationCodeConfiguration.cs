using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;
using Waybon.Domain.Enums;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class VerificationCodeConfiguration : IEntityTypeConfiguration<VerificationCode>
{
    public void Configure(EntityTypeBuilder<VerificationCode> builder)
    {
        // ===================================
        // Table
        // ===================================

        builder.ToTable("verification_code", table =>
        {
            var purposes = string.Join(", ", Enum.GetNames<VerificationPurpose>().Select(name => $"'{name}'"));
            table.HasCheckConstraint("ck_verification_code_purpose", $"purpose IN ({purposes})");
        });


        // ===================================
        // Id
        // ===================================

        builder.HasKey(code => code.Id);


        // ===================================
        // UserId
        // ===================================

        builder.HasOne<User>()
        .WithMany()
        .HasForeignKey(code => code.UserId)
        .OnDelete(DeleteBehavior.Cascade);


        // ===================================
        // Purpose
        // ===================================

        builder.Property
        (
            code => code.Purpose
        )
        .HasConversion<string>()
        .HasMaxLength(VerificationCode.PurposeMaxLength);

        builder.HasIndex
        (
            code => new { code.UserId, code.Purpose }
        )
        .IsUnique();


        // ===================================
        // TokenHash
        // ===================================

        builder.Property
        (
            code => code.TokenHash
        )
        .IsRequired()
        .HasMaxLength(VerificationCode.HashLength);

        builder.HasIndex
        (
            code => code.TokenHash
        )
        .IsUnique();


        // ===================================
        // CodeHash
        // ===================================

        builder.Property
        (
            code => code.CodeHash
        )
        .HasMaxLength(VerificationCode.HashLength);
    }
}