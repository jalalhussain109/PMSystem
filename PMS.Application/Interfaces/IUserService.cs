using PMS.Application.Common;
using PMS.Application.DTOs;
using PMS.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces
{
    public interface IUserService 
    {
        Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto);

        Task<ServiceResult<UserDto>> GetByIdAsync(long id);

        Task<ServiceResult<List<UserDto>>> GetAllAsync();

        Task<ServiceResult<UserDto>> UpdateAsync( long id, UpdateUserDto dto);

        Task<ServiceResult<bool>> DeleteAsync(long id);
    }
}
