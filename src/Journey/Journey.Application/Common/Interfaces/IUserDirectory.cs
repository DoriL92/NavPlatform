using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Common.Interfaces;
public interface IUserDirectory
{
    Task<IReadOnlyDictionary<string, string>> ResolveUserIdsByEmailAsync(
        IEnumerable<string> emails, CancellationToken ct);
}
