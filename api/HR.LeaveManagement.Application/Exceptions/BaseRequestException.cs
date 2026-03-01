using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;


namespace HR.LeaveManagement.Application.Exceptions
{
    public class BaseRequestException : Exception
    {
        public BaseRequestException(string message) : base(message) { }

        public BaseRequestException(string message, ValidationResult validationResult) : base(message)
        {
            ValidationErrors = new();

            foreach (var error in validationResult.Errors)
            {
                ValidationErrors.Add(error.ErrorMessage);
            }
        }

        public List<string> ValidationErrors { get; set; } = new List<string>();
    }
}