using PMS.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.Application.Interfaces
{
    public interface IGenericService<TEntity, TKey> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> AddAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(TKey id);

    }
}
