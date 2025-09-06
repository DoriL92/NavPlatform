using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Interfaces;

namespace Application.UnitTests.Support;
public sealed class TestCurrentUser : ICurrentUser
{
    public TestCurrentUser(string? id) { UserId = id; }
    public string? UserId { get; }
    public string? Email => null;
    public bool IsAuthenticated => false;

    string? ICurrentUser.Name => throw new NotImplementedException();

    IEnumerable<string> ICurrentUser.RolesOrPermissions => throw new NotImplementedException();
}
