using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Interfaces;

namespace Application.UnitTests.Support;
public sealed class TestClock : IDateTime
{
    public TestClock() : this(System.DateTimeOffset.UtcNow) { }
    public TestClock(System.DateTimeOffset now) { Now = now; }
    public System.DateTimeOffset Now { get; }
}