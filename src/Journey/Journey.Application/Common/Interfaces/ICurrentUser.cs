namespace CleanArchitecture.Application.Common.Interfaces;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }          
    string? Name { get; }
    string? Email { get; }
    IEnumerable<string> RolesOrPermissions { get; }

}
