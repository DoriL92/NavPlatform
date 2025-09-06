using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Journeys.Events;

namespace CleanArchitecture.Domain.Common;
public interface IHasDomainEvents
{
    List<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
