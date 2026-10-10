using Microsoft.AspNetCore.Identity;
using PMS.Domain.Entities;

namespace PMS.Domain.Interfaces
{
    public interface IIdentityUserRepository
    {
        Task<IReadOnlyList<ApplicationUser>> GetAllAsync();

        Task<ApplicationUser?> GetByIdAsync(long id);

        Task<ApplicationUser?> GetByUserNameAsync(string userName);

        Task<IdentityResult> CreateAsync( ApplicationUser user, string password);

        Task<IdentityResult> UpdateAsync(ApplicationUser user);

        Task<IdentityResult> DeleteAsync(ApplicationUser user);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    }
}
