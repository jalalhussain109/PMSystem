using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs
{
    public class UserPermissionsDto
    {
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = [];

        public List<string> Permissions { get; set; } = [];
    }
}
