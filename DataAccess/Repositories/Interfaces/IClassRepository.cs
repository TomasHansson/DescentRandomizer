using Domain.DataTransferObjects;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface IClassRepository
    {
        Task<Result<Class>> Create(Class newClass);
        Task<Result<bool>> Delete(Guid id);
        Result<Class> Get(Guid id);
        Task<Result<List<Class>>> GetAll(bool includeClassCards = true);
        Result<bool> IsClassNameTaken(string name, Guid? id);
        Task<Result<Class>> Update(Class classToUpdate);
    }
}