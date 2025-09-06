using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Common;

namespace CleanArchitecture.Infrastructure.Persistence.Outbox;
public class OutboxMessage: BaseAuditableEntity
{
    public Guid Id { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public string Type { get; set; } = default!;
    public string Payload { get; set; } = default!;
    public string? TraceId { get; set; }
    public int Attempts { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
