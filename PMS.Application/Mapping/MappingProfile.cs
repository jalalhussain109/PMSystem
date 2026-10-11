using AutoMapper;
using PMS.Application.DTOs;
using PMS.Application.DTOs.ProjectMembers;
using PMS.Domain.Entities;

namespace PMS.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Project -> ProjectDto mapping is handled in service due to members being managed via repository
            CreateMap<TaskItem, TaskDto>()
                // map constructor params for positional record TaskDto
                .ForCtorParam("Status", opt => opt.MapFrom(src => src.Status.ToString()))
                .ForCtorParam("ProjectName", opt => opt.MapFrom(src => src.Project != null ? src.Project.ProjectName : null))
                .ForCtorParam("AssigneeName", opt => opt.MapFrom(src => src.AssignedUser != null ? (src.AssignedUser.DisplayName ?? src.AssignedUser.UserName) : null));
            
            CreateMap<ApplicationUser, UserDto>();
            CreateMap<ProjectMember, ProjectMemberDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));
            CreateMap<ApplicationUser, AuthResultDto>()
               .ForCtorParam("UserId", opt => opt.MapFrom(src => src.Id))
               .ForCtorParam("UserName", opt => opt.MapFrom(src => src.UserName ?? string.Empty))
               .ForCtorParam("DisplayName", opt => opt.MapFrom(src => src.DisplayName ?? string.Empty))
               .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email))
               .ForCtorParam("IsActive", opt => opt.MapFrom(src => src.IsActive)) ;
        }
    }
}
