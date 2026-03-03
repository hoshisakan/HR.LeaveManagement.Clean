using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Domain;
using Microsoft.EntityFrameworkCore;
using HR.LeaveManagement.Persistence.DatabaseContext;


namespace HR.LeaveManagement.Persistence.Repositories
{
    public class LeaveAllocationRepository : GenericRepository<LeaveAllocation>, ILeaveAllocationRepository
    {
        public LeaveAllocationRepository(HrDatabaseContext context) : base(context)
        {
        }

        public async Task<List<LeaveAllocation>> GetLeaveAllocationsWithDetails()
        {
            var leaveAllocations = await _dbSet.Include(q => q.LeaveType).ToListAsync();
            return leaveAllocations;
        }

        public async Task<List<LeaveAllocation>> GetLeaveAllocationsWithDetails(string userId)
        {
            var leaveAllocations = await _dbSet.Include(q => q.LeaveType).Where(q => q.EmployeeId == userId).ToListAsync();
            return leaveAllocations;
        }

        public async Task<LeaveAllocation> GetLeaveAllocationWithDetails(int id)
        {
            var leaveAllocation = await _dbSet.Include(q => q.LeaveType).FirstOrDefaultAsync(q => q.Id == id);
            return leaveAllocation;
        }

        public async Task<bool> AllocationExists(string userId, int leaveTypeId, int period)
        {
            return await _dbSet.AnyAsync(q => q.EmployeeId == userId && q.LeaveTypeId == leaveTypeId && q.Period == period);
        }

        public async Task AddAllocations(List<LeaveAllocation> allocations)
        {
            await _dbSet.AddRangeAsync(allocations);
            await _context.SaveChangesAsync();
        }

        public async Task<LeaveAllocation> GetUserAllocations(string userId, int leaveTypeId)
        {
            var leaveAllocation = await _dbSet.Include(q => q.LeaveType).FirstOrDefaultAsync(q => q.EmployeeId == userId && q.LeaveTypeId == leaveTypeId);
            return leaveAllocation;
        }

        public async Task<bool> LeaveAllocationMustExist(string userId, int leaveTypeId, int period)
        {
            return await _dbSet.AnyAsync(q => q.EmployeeId == userId && q.LeaveTypeId == leaveTypeId && q.Period == period);
        }
    }
}