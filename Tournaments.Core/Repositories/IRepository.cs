namespace Tournaments.Core.Repositories;

public interface IRepository<T> where T : IBaseEntity
{
    public Task<IEnumerable<T>> GetAllAsync();
    public Task<T?> GetAsync(int id);
    public Task<bool> AnyAsync(int id);
    public Task<T?> AddAsync(T entity);
    public Task<T?> UpdateAsync(T entity);
    public Task<T?> RemoveAsync(int entityId);
}