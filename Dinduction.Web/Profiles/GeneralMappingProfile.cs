// Dinduction.Web/Profiles/GeneralMappingProfile.cs
using AutoMapper;
using Dinduction.Domain.Entities;
using Dinduction.Web.Models;

namespace Dinduction.Web.Profiles;

public class GeneralMappingProfile : Profile
{
    public GeneralMappingProfile()
    {
        CreateMap<Role, RoleVM>();
        CreateMap<RoleVM, Role>();

        CreateMap<User, UserVM>();
        CreateMap<UserVM, User>();

        CreateMap<Section, SectionVM>();
        CreateMap<SectionVM, Section>();
    }
}