using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Journeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public class JourneyShareConfiguration : IEntityTypeConfiguration<JourneyShare>
{
    public void Configure(EntityTypeBuilder<JourneyShare> b)
    {
        b.ToTable("JourneyShares");
        b.HasKey(x => x.Id);

        b.Property(x => x.TargetUserId).IsRequired().HasMaxLength(128);
        b.Property(x => x.GrantedByUserId).IsRequired().HasMaxLength(128);

        b.HasOne<JourneyEntity>()                // no back nav on JourneyShare (optional)
            .WithMany(j => j.Shares)                // JourneyEntity.Shares
            .HasForeignKey(s => s.JourneyId)
            .OnDelete(DeleteBehavior.Cascade);
        // UNIQUE composite index on (JourneyId, TargetUserId) for **active** shares only
        // (allows historical rows once revoked)
        b.HasIndex(x => new { x.JourneyId, x.TargetUserId })
         .IsUnique()
         .HasFilter("[RevokedAt] IS NULL"); // SQL Server filter syntax
    }
}
