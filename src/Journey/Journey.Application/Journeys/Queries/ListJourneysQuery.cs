using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Journeys.Dto;
using CleanArchitecture.Domain.Journeys;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.Journeys.Queries;
public record ListJourneysQuery(int Page = 1, int PageSize = 20) : IRequest<PagedList<JourneyDto>>;
