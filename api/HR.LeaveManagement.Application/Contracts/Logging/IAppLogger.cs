using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


namespace HR.LeaveManagement.Application.Contracts.Logging
{
    public interface IAppLogger<T>
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, params object[] args);
        void LogCritical(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogTrace(string message, params object[] args);
    }
}