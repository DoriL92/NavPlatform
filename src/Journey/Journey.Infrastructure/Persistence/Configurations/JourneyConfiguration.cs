using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Journeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public class JourneyConfiguration : IEntityTypeConfiguration<JourneyEntity>
{
    public void Configure(EntityTypeBuilder<JourneyEntity> b)
    {
        b.ToTable("Journeys");
        b.HasKey(x => x.Id);

        b.Property(x => x.OwnerUserId).IsRequired().HasMaxLength(200);
        b.HasIndex(x => x.OwnerUserId);
        b.Property(x => x.OwnerEmail)
         .HasColumnName("OwnerEmail")
         .HasMaxLength(256);
        b.Property(x => x.StartLocation).IsRequired().HasMaxLength(200);
        b.Property(x => x.ArrivalLocation).IsRequired().HasMaxLength(200);
        b.Property(x => x.StartTime).IsRequired();
        b.Property(x => x.ArrivalTime).IsRequired();
        b.Property(x => x.IsDailyGoalAchieved).IsRequired();
        b.Property(x => x.IsDeleted).HasDefaultValue(false);

        // timestamps (optional defaults at DB)
        b.Property(x => x.CreatedAt)
         .HasDefaultValueSql("SYSUTCDATETIME()")
         .IsRequired();
        b.Property(x => x.UpdatedAt);

        // enum as string (optional)
        // b.Property(x => x.TransportType).HasConversion<string>().HasMaxLength(32).IsRequired();

        // value object
        b.OwnsOne(x => x.DistanceKm, o =>
        {
            o.Property(p => p.Value)
             .HasColumnName("DistanceKm")
             .HasPrecision(5, 2)
             .IsRequired();
        });
        b.Navigation(x => x.DistanceKm).IsRequired();

        // soft-delete filter
        b.HasQueryFilter(x => !x.IsDeleted);

        // indexes
        b.HasIndex(x => new { x.OwnerUserId, x.StartTime })
         .HasDatabaseName("IX_Journeys_Owner_StartTime");

        // constraints
        b.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_Journeys_Distance_Positive", "[DistanceKm] >= 0");
            tb.HasCheckConstraint("CK_Journeys_Time_Order", "[ArrivalTime] >= [StartTime]");
        });



       // b.HasOne(j => j.Owner)
       //.WithMany(u => u.OwnedJourneys)                
       //.HasForeignKey(j => j.OwnerUserId)
       //.OnDelete(DeleteBehavior.Restrict)            
       //.HasConstraintName("FK_Journeys_Users_OwnerUserId");
    }
}
