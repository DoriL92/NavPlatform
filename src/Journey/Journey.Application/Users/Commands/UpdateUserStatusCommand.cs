using CleanArchitecture.Domain.Enums;
using MediatR;

namespace CleanArchitecture.Application.Users.Commands;

public record UpdateUserStatusCommand(string UserId, string Status) : IRequest;

