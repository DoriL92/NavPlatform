using AutoMapper;
using CleanArchitecture.Application.Users.Queries;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.Users.Mappings;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}


