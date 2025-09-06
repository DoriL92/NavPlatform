using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public class JourneyPublicLinkConfiguration : IEntityTypeConfiguration<JourneyPublicLink>
{
    public void Configure(EntityTypeBuilder<JourneyPublicLink> b)
    {
        b.ToTable("JourneyPublicLinks");
        b.HasKey(x => x.Id);

        b.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(128);

        // Token must be globally UNIQUE
        b.HasIndex(x => x.Token).IsUnique();
    }
}
