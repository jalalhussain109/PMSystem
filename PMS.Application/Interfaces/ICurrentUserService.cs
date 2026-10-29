using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.Interfaces
{
    public interface ICurrentUserService
    {
        long? UserId { get; }
    }
}
