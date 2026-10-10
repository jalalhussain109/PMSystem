using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PMS.Application.Common;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;

namespace PMS.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IIdentityUserRepository _userRepository;
        private readonly AutoMapper.IMapper _mapper;

        public UserService(IIdentityUserRepository userRepository, AutoMapper.IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<UserDto>> CreateAsync(CreateUserDto dto)
        {
            // check existing
            var exists = await _userRepository.GetByUserNameAsync(dto.UserName);
            if (exists != null)
                return ServiceResult<UserDto>.Failure("UserName already exists");

            var user = new ApplicationUser
            {
                UserName = dto.UserName.Trim(),
                DisplayName = dto.DisplayName?.Trim(),
                Email = dto.Email?.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userRepository.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return ServiceResult<UserDto>.Failure(errors);
            }

            var dtoOut = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.Success(dtoOut);
        }

        public async Task<ServiceResult<UserDto>> GetByIdAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return ServiceResult<UserDto>.Missing();
            return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(user));
        }

        public async Task<ServiceResult<List<UserDto>>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var list = users.Select(u => _mapper.Map<UserDto>(u)).ToList();
            return ServiceResult<List<UserDto>>.Success(list);
        }

        public async Task<ServiceResult<UserDto>> UpdateAsync(long id, UpdateUserDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return ServiceResult<UserDto>.Missing();

            user.UserName = dto.UserName.Trim();
            user.DisplayName = dto.DisplayName?.Trim();
            user.Email = dto.Email?.Trim();
            user.PhoneNumber = dto.PhoneNumber?.Trim();
            user.IsActive = dto.IsActive;
            user.ModifiedDate = DateTime.UtcNow;

            var result = await _userRepository.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToArray();
                return ServiceResult<UserDto>.Failure(errors);
            }

            return ServiceResult<UserDto>.Success(_mapper.Map<UserDto>(user));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return ServiceResult<bool>.Missing();

            var result = await _userRepository.DeleteAsync(user);
            if (!result.Succeeded)
                return ServiceResult<bool>.Failure(result.Errors.Select(e => e.Description).ToArray());

            return ServiceResult<bool>.Success(true);
        }
    }
}
