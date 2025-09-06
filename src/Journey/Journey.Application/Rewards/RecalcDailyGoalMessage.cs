using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Rewards;
public sealed record RecalcDailyGoalMessage(string UserId, DateTimeOffset DayUtc);


