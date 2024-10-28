using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.BLL;
using Base.Contacts;
using Base.Contracts;

namespace App.BLL.Services;

public class ServiceService : BaseEntityService<Service, DTO.DAL.Service, IServiceRepository>,
    IServiceService
{
    private readonly IAppUOW _uow;

    public ServiceService(IAppUOW uow, IMapper<DTO.DAL.Service, Service> mapper) : base(uow.ServiceRepository, mapper)
    {
        _uow = uow;
    }
}