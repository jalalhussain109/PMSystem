using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs
{
    public class CreateProjectMemberDto
    {
        public long ProjectId { get; set; }
        public long UserId { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
