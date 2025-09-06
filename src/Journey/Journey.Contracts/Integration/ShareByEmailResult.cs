using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain.Events;
public sealed record ShareByEmailResult(
    bool Success,
    int ShareCount
);
