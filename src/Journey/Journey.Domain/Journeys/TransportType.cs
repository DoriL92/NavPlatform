using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Journeys;
public enum TransportType : byte
{
    Any = 0,
    Car = 1,
    Bus = 2,
    Train = 3,
    Ferry = 4,
    Plane = 5,
    Bike = 6,
    Walk = 7
}
