using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarInsuranceBot.Core.Interfaces.Repositories
{
    public interface ICrudRepository<TEntity, TId>
    {
        public Task<TEntity> CreateAsync(TEntity entity);
        public Task<TEntity?> GetByIdAsync(TId id);
        public Task<TEntity> UpdateAsync(TEntity entity);
        public Task DeleteAsync(TEntity entity);
        public Task DeleteByIdAsync(TId id);
    }
}
