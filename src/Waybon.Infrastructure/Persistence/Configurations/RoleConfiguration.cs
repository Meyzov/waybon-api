using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // ===================================
        // Table
        // ===================================

        builder.ToTable("role");


        // ===================================
        // Id
        // ===================================

        builder.HasKey(role => role.Id);


        // ===================================
        // Name
        // ===================================

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
        

        // ===================================
        // IsDefault
        // ===================================

        builder.HasIndex
        (
            role => role.IsDefault
        )
        .IsUnique()
        .HasFilter("is_default = true");
    }
}