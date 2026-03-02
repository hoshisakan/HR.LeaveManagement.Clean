using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using HR.LeaveManagement.Persistence.DatabaseContext;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Persistence.Repositories;


namespace HR.LeaveManagement.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add the DbContext
            services.AddDbContext<HrDatabaseContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("HrDatabaseConnectionString")));

            // Add the GenericRepository implementation for all the entities
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Add the repositories for the entities
            services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
            services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
            services.AddScoped<ILeaveAllocationRepository, LeaveAllocationRepository>();

            return services;
        }
    }
}