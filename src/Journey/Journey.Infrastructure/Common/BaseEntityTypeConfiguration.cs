using System.Reflection;
using CleanArchitecture.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Common;
internal class BaseEntityTypeConfiguration
{
}
public abstract class BaseEntityTypeConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public void Configure(EntityTypeBuilder<T> builder)
    {

        builder.HasKey(x => x.Id);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);

        var type = typeof(BaseEntity);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var propertyInfo = type.GetProperty(property.Name);

            var propertyType = propertyInfo?.PropertyType;
            if (propertyType == typeof(string))
                builder.Property(property.Name).HasMaxLength(500);

            if (property.Name == nameof(BaseAuditableEntity.CreatedDate))
                builder.Property(property.Name).HasDefaultValueSql("getdate()");

            if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                builder.Property(property.Name).HasPrecision(18, 2);

        }

        ConfigureOtherProperties(builder);
    }

    protected abstract void ConfigureOtherProperties(EntityTypeBuilder<T> builder);
}