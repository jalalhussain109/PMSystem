namespace PMS.Application.Interfaces;

public interface IEffectivePermissionService
{
    Task<IReadOnlyCollection<string>> GetEffectivePermissionsAsync(long userId);
}