using CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;

public class UserStatusAuditConfiguration : IEntityTypeConfiguration<UserStatusAudit>
{
    public void Configure(EntityTypeBuilder<UserStatusAudit> builder)
    {
        builder.ToTable("UserStatusAudits");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(128);
            
        builder.Property(x => x.AdminUserId)
            .IsRequired()
            .HasMaxLength(128);
            
        builder.Property(x => x.PreviousStatus)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(x => x.NewStatus)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(x => x.ChangedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
            
        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.HasOne(x => x.User)
            .WithMany(x => x.StatusAudits)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ChangedAt);
    }
}

