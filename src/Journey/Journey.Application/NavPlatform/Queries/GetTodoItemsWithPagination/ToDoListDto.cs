using CleanArchitecture.Application.Common.Mappings;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.ValueObjects;

namespace CleanArchitecture.Application.TodoItems.Queries.GetTodoItemsWithPagination;

public class ToDoListDto 
{
    public Guid Id { get; init; }

    public Colour Colour { get; init; }

    public string? Title { get; init; }


}
