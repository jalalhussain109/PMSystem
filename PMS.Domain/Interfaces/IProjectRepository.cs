using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Domain.Entities;

namespace PMS.Domain.Interfaces
{
    public interface IProjectRepository : IGenericRepository<Project, long>
    {
   
    }
}
