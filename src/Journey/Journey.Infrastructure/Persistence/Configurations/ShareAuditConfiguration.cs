using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public class ShareAuditConfiguration : IEntityTypeConfiguration<JourneyShareAudit>
{
    public void Configure(EntityTypeBuilder<JourneyShareAudit> b)
    {
        b.ToTable("ShareAudits");
        b.HasKey(x => x.Id);
        b.Property(x => x.Action).IsRequired().HasMaxLength(64);
    }
}
