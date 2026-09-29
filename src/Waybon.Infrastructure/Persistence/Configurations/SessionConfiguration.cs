using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    // ===================================
    // Constants
    // ===================================

    private const string TableName = "session";


    // ===================================
    // Configure
    // ===================================

    public void Configure(EntityTypeBuilder<Session> builder)
    {
        // ===================================
        // Table
        // ===================================

        builder.ToTable(TableName);


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
        .HasMaxLength(Session.TokenHashLength);

        builder.HasIndex
        (
            session => session.TokenHash
        )
        .IsUnique();
    }
}