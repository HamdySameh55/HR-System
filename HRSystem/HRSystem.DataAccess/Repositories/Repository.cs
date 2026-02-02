using Microsoft.EntityFrameworkCore;
using HRSystem.Core.Interfaces;

namespace HRSystem.DataAccess.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly HRDbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(HRDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
        => await _set.ToListAsync();

    public async Task<T?> GetByIdAsync(int id)
        => await _set.FindAsync(id);

    public async Task<T> AddAsync(T entity)
    {
        _set.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _set.FindAsync(id);
        if (entity is null) return false;

        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _set.FindAsync(id) is not null;
}