using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.BLL;
using Base.Contacts;
using Base.Contracts;

namespace App.BLL.Services;

public class ClientService : BaseEntityService<Client, DTO.DAL.Client, IClientRepository>,
    IClientService
{
    private readonly IAppUOW _uow;

    public ClientService(IAppUOW uow, IMapper<DTO.DAL.Client, Client> mapper) : base(
        uow.ClientRepository, mapper)
    {
        _uow = uow;
    }

    public async Task<Client?> FindByEmailAsync(string email)
    {
        return Mapper.Map(await _uow.ClientRepository.FindByEmailAsync(email));
    }
}