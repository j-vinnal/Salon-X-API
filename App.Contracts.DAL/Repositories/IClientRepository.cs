using App.DTO.DAL;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface IClientRepository : IBaseRepository<Client>, IClientRepositoryCustom<Client>
{
    // Define additional methods if needed
}

public interface IClientRepositoryCustom<TEntity>
{
    // Define additional methods if needed


    Task<TEntity?> FindByEmailAsync(string email);
}