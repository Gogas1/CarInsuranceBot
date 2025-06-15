namespace CarInsuranceBot.Core.Interfaces.Repositories
{
    public interface ICrudRepository<TEntity, TId>
    {
        public Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken);
        public Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken);
        public Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken);
        public Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
        public Task DeleteByIdAsync(TId id, CancellationToken cancellationToken);
    }
}
