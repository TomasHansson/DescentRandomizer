using Domain.DataTransferObjects;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.Interfaces
{
    public interface IHeroesRepository
    {
        Task<Result<Hero>> Create(Hero hero);
        Task<Result<bool>> Delete(Guid id);
        Result<Hero> Get(Guid id);
        Task<Result<List<Hero>>> GetAll();
        Result<bool> IsHeroNameTaken(string name, Guid? id);
        Task<Result<Hero>> Update(Hero hero);
    }
}
