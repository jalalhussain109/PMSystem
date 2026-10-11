using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMS.Application.Common;
using PMS.Application.DTOs.ProjectMembers;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;

namespace PMS.Application.Services
{
    public class ProjectMemberService : IProjectMemberService
    {
        private readonly IProjectMemberRepository _repository;
        private readonly AutoMapper.IMapper _mapper;

        public ProjectMemberService(IProjectMemberRepository repository, AutoMapper.IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<ProjectMemberDto>> CreateAsync(CreateProjectMemberDto dto)
        {
            var member = new ProjectMember
            {
                ProjectId = dto.ProjectId,
                UserId = dto.UserId,
                Role = dto.Role ?? string.Empty,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repository.AddAsync(member);
            return ServiceResult<ProjectMemberDto>.Success(_mapper.Map<ProjectMemberDto>(created));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(long id)
        {
            var deleted = await _repository.DeleteAsync(id);
            return deleted ? ServiceResult<bool>.Success(true) : ServiceResult<bool>.Failure("Delete failed");
        }

        public async Task<ServiceResult<ProjectMemberDto>> GetByIdAsync(long id)
        {
            var member = await _repository.GetByIdAsync(id);
            if (member == null) return ServiceResult<ProjectMemberDto>.Missing();
            return ServiceResult<ProjectMemberDto>.Success(_mapper.Map<ProjectMemberDto>(member));
        }

        public async Task<ServiceResult<IEnumerable<ProjectMemberDto>>> ListAsync()
        {
            var list = await _repository.ListAsync();
            var mapped = list.Select(m => _mapper.Map<ProjectMemberDto>(m));
            return ServiceResult<IEnumerable<ProjectMemberDto>>.Success(mapped);
        }

        public async Task<ServiceResult<ProjectMemberDto>> UpdateAsync(long id, UpdateProjectMemberDto dto)
        {
            var member = await _repository.GetByIdAsync(id);
            if (member == null) return ServiceResult<ProjectMemberDto>.Missing();

            member.ProjectId = dto.ProjectId;
            member.UserId = dto.UserId;
            member.Role = dto.Role ?? string.Empty;
            member.IsActive = dto.IsActive;
            member.ModifiedDate = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(member);
            return ServiceResult<ProjectMemberDto>.Success(_mapper.Map<ProjectMemberDto>(updated));
        }
    }
}
