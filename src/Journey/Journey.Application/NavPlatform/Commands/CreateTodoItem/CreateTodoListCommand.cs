using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Application.TodoItems.Commands.CreateTodoItem;

public record CreateTodoListCommand : IRequest<Guid>
{
    public string? Title { get; init; }
}

public class CreateTodoListCommandHandler : IRequestHandler<CreateTodoListCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTodoListCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTodoListCommand request, CancellationToken cancellationToken)
    {


        //var exist=  await _context.EntitySet<TodoList>().AnyAsync(x=>x.Title==request.Title);
        //if(exist)
        //{
        //    throw new BadRequestException("Exist with some name!");
        //}
        //var toDoList = new TodoList()
        //{
        //    Colour = Colour.White,
        //    Title = request.Title

        //};

        //_context.EntitySet<TodoList>().Add(toDoList);
        //await _context.SaveChangesAsync(cancellationToken);
        return Guid.NewGuid();
    }
}
