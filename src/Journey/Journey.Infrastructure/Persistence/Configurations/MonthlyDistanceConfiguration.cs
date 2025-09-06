using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public sealed class MonthlyDistanceConfiguration : IEntityTypeConfiguration<MonthlyDistance>
{
    public void Configure(EntityTypeBuilder<MonthlyDistance> b)
    {
        b.ToTable("MonthlyDistances");
        b.HasKey(x => new { x.UserId, x.Year, x.Month });
        b.Property(x => x.TotalDistanceKm).HasPrecision(12, 2);
        b.HasIndex(x => x.UserId);
    }
}
