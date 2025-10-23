// ConfigurationReader.Persistence/Configurations/ConfigurationItemConfiguration.cs
using ConfigurationReader.Domain.Entities;
using ConfigurationReader.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConfigurationReader.Persistence.Configurations;

public class ConfigurationItemConfiguration : IEntityTypeConfiguration<ConfigurationItem>
{
    public void Configure(EntityTypeBuilder<ConfigurationItem> builder)
    {
        builder.ToTable("ConfigurationItems");

        // Primary Key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),  
                v => Enum.Parse<ConfigurationType>(v))  
            .HasMaxLength(20);

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(5000);

        builder.Property(x => x.ApplicationName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Concurrency token
        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // Indexes
        builder.HasIndex(x => new { x.ApplicationName, x.Name })
            .IsUnique()
            .HasDatabaseName("IX_ApplicationName_Name");

        builder.HasIndex(x => new { x.ApplicationName, x.IsActive })
            .HasDatabaseName("IX_ApplicationName_IsActive");

        builder.HasIndex(x => x.UpdatedAt)
            .HasDatabaseName("IX_UpdatedAt");
    }
}