using Domain.DataTransferObjects;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface IClassItemRepository
    {
        Task<Result<ClassItem>> Create(ClassItem classCard);
        Task<Result<bool>> Delete(Guid id);
        Result<ClassItem> Get(Guid id);
        Task<Result<List<ClassItem>>> GetAll();
        Result<bool> IsClassItemNameTaken(string name, Guid? id, Guid classId);
        Task<Result<ClassItem>> Update(ClassItem classCard);
    }
}
