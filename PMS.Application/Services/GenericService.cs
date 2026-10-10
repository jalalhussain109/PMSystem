using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Application.Interfaces;
using PMS.Domain.Interfaces;

namespace PMS.Application.Services
{
    public class GenericService<TEntity, TKey> : IGenericService<TEntity, TKey>
        where TEntity : class
    {
        private readonly IGenericRepository<TEntity, TKey> _repository;

        public GenericService(IGenericRepository<TEntity, TKey> repository)
        {
            _repository = repository;
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<bool> DeleteAsync(TKey id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<TEntity?> GetByIdAsync(TKey id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            return await _repository.UpdateAsync(entity);
        }
    }
}
