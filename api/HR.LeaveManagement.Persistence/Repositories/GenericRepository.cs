using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.LeaveManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;
using HR.LeaveManagement.Persistence.DatabaseContext;
using HR.LeaveManagement.Application.Contracts.Persistence;


namespace HR.LeaveManagement.Persistence.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        protected readonly HrDatabaseContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(HrDatabaseContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        
        public async Task<IReadOnlyList<T>> GetAsync()
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}