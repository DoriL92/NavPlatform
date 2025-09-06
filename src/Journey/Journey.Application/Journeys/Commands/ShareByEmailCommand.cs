using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Domain.Events;
using MediatR;

namespace CleanArchitecture.Application.Journeys.Commands;
public sealed record ShareByEmailCommand(int JourneyId, string[] Emails) :  IRequest<ShareByEmailResult>;
