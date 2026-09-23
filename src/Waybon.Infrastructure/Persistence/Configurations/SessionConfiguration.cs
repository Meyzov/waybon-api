using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        // ===================================
        // Table
        // ===================================

        builder.ToTable("session");


        // ===================================
        // Id
        // ===================================

        builder.HasKey(session => session.Id);


        // ===================================
        // UserId
        // ===================================

        builder.HasIndex
        (
            session => session.UserId
        )
        .IsUnique();

        builder.HasOne<User>()
        .WithOne()
        .HasForeignKey<Session>(session => session.UserId)
        .OnDelete(DeleteBehavior.Cascade);


        // ===================================
        // TokenHash
        // ===================================

        builder.Property
        (
            session => session.TokenHash
        )
        .IsRequired()
        .HasMaxLength(64);

        builder.HasIndex
        (
            session => session.TokenHash
        )
        .IsUnique();
    }
}