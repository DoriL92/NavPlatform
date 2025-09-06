namespace CleanArchitecture.Application.Common.Interfaces;

public interface IDateTime
{
    DateTimeOffset Now { get; }
}
