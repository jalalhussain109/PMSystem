using AutoMapper;
using PMS.Application.DTOs;
using PMS.Domain.Entities;

namespace PMS.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Project, ProjectDto>();
            CreateMap<TaskItem, TaskDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<ApplicationUser, UserDto>();
            CreateMap<ApplicationUser, AuthResultDto>()
               .ForCtorParam("UserId", opt => opt.MapFrom(src => src.Id))
               .ForCtorParam("UserName", opt => opt.MapFrom(src => src.UserName ?? string.Empty))
               .ForCtorParam("DisplayName", opt => opt.MapFrom(src => src.DisplayName ?? string.Empty))
               .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email))
               .ForCtorParam("IsActive", opt => opt.MapFrom(src => src.IsActive)) ;
        }
    }
}
