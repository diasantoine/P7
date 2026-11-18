using Dot.Net.WebApi.Data;
using Microsoft.EntityFrameworkCore;

namespace Dot.Net.WebApi.Repositories
{
    // One repository for every entity: the same five operations, whatever the table.
    public class DataRepository<T> : IDataRepository<T> where T : class
    {
        private readonly LocalDbContext _dbContext;

        public DataRepository(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<T>> GetAllAsync()
        {
            return _dbContext.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _dbContext.Set<T>().Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
