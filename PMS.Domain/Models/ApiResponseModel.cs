using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Models
{
    public class ApiLogResponseModel
    {
        public long Id { get; set; }
        public long ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? Title { get; set; }
        public string? Message { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? Summary { get; set; }
    }
}
