using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArchitecture.Infrastructure.Persistence.Configurations;
public class JourneyFavouriteConfiguration : IEntityTypeConfiguration<JourneyFavorite>
{
    public void Configure(EntityTypeBuilder<JourneyFavorite> b)
    {
        b.ToTable("JourneyFavourites");
        b.HasKey(x => x.Id);

        b.Property(x => x.JourneyId).IsRequired();
        b.Property(x => x.UserId).IsRequired().HasMaxLength(128);
        b.Property(x => x.CreatedAt).IsRequired();

        b.HasIndex(x => new { x.JourneyId, x.UserId }).IsUnique();
    }
}
