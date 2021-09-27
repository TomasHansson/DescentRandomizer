using Domain.DataTransferObjects;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface IClassCardRepository
    {
        Task<Result<ClassCard>> Create(ClassCard classCard);
        Task<Result<bool>> Delete(Guid id);
        Result<ClassCard> Get(Guid id);
        Task<Result<List<ClassCard>>> GetAll();
        Result<bool> IsClassCardNameTaken(string name, Guid? id, Guid classId);
        Task<Result<ClassCard>> Update(ClassCard classCard);
    }
}