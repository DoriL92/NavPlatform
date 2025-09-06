namespace CleanArchitecture.Domain.Common;

public interface ISoftDeleted
{
    bool IsDeleted { get; set; }
}


