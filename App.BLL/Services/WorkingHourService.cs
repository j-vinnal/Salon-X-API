using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.BLL;
using Base.Contacts;

namespace App.BLL.Services;

public class WorkingHourService : BaseEntityService<WorkingHour, App.DTO.DAL.WorkingHour, IWorkingHourRepository>,
    IWorkingHourService
{
    private readonly IAppUOW _uow;


    public WorkingHourService(IAppUOW uow, IMapper<App.DTO.DAL.WorkingHour, WorkingHour> mapper) : base(
        uow.WorkingHourRepository, mapper)
    {
        _uow = uow;
    }
}