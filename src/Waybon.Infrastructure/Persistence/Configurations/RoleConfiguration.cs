using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waybon.Domain.Entities;

namespace Waybon.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    // ===================================
    // Constants
    // ===================================

    private const string TableName = "role";
    private const string DefaultRoleFilter = "is_default = true";


    // ===================================
    // Configure
    // ===================================

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        // ===================================
        // Table
        // ===================================

        builder.ToTable(TableName);


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
        .HasMaxLength(Role.NameMaxLength);

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
        .HasFilter(DefaultRoleFilter);
    }
}