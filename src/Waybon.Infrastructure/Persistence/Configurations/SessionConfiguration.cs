using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("session");

        builder.HasKey(session => session.Id);

        builder.HasIndex
        (
            session => session.UserId
        )
        .IsUnique();

        builder.HasOne<User>()
        .WithOne()
        .HasForeignKey<Session>(session => session.UserId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.Property
        (
            session => session.TokenHash
        )
        .IsRequired()
        .HasMaxLength(1024);

        builder.Property
        (
            session => session.CreatedAt
        )
        .HasColumnType("timestamptz");

        builder.Property
        (
            session => session.UpdatedAt
        )
        .HasColumnType("timestamptz");
    }
}