using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace CleanArchitecture.Application.Journeys.Commands;
public sealed record UnshareByEmailCommand(int JourneyId, string[] Emails) : IRequest;

