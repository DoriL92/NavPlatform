using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;
using CleanArchitecture.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("User");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasMaxLength(128);
        b.Property(x => x.Email).HasMaxLength(256);
        b.Property(x => x.Name).HasMaxLength(256);
        b.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(CleanArchitecture.Domain.Enums.UserStatus.Active);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

    }

}
