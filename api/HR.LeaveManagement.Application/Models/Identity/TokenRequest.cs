using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace HR.LeaveManagement.Application.Models.Identity
{
    public class TokenRequest
    {
        [Required]
        public string JwtToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}