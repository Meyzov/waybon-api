using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");
        
        builder.HasKey(role => role.Id);

        builder.Property
        (
            role => role.Name
        )
        .IsRequired()
        .HasMaxLength(25);

        builder.HasIndex
        (
            role => role.Name
        )
        .IsUnique();

        builder.Property
        (
            role => role.CreatedAt
        )
        .HasColumnType("timestamptz");

        builder.Property
        (
            role => role.UpdatedAt
        )
        .HasColumnType("timestamptz");
    }
}