using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Domain;
public class JourneyShare
{
    public int Id { get; set; }
    public int JourneyId { get; set; }
    public string TargetUserId { get; set; } = default!;  
    public string GrantedByUserId { get; set; } = default!;
    public DateTimeOffset GrantedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }


}