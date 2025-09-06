using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("OutboxMessages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasMaxLength(200);
        b.Property(x => x.Payload).HasColumnType("nvarchar(max)");
        b.Property(x => x.TraceId).HasMaxLength(64);
        b.HasIndex(x => new { x.ProcessedOnUtc, x.OccurredOnUtc });
    }
}
