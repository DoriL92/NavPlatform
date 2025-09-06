using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace CleanArchitecture.Domain.Common;
public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredOn { get; }
}
