using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;


namespace HR.LeaveManagement.Api.Models
{
    public class CustomValidationProblemDetails : ProblemDetails
    {
        public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
    }
}