using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface IClientService : IBaseRepository<Client>, IClientRepositoryCustom<Client>
{
    // Define additional methods if needed
}