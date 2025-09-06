using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Common.Interfaces;

public interface IApplicationDbContext
{


    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    DbSet<T> EntitySet<T>() where T : class;
}
