using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Persistence.Rewards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public sealed class DailyGoalConfiguration : IEntityTypeConfiguration<DailyGoal>
{
    public void Configure(EntityTypeBuilder<DailyGoal> b)
    {
        b.ToTable("DailyGoals");
        b.HasKey(x => new { x.OwnerUserId, x.DayUtc });

        b.Property(x => x.OwnerUserId)
            .HasMaxLength(128)
            .IsRequired();

        b.Property(x => x.DayUtc).HasColumnType("date");

        b.Property(x => x.AchievedJourneyId).IsRequired();
        b.Property(x => x.AchievedAtUtc).IsRequired();
    }
}
