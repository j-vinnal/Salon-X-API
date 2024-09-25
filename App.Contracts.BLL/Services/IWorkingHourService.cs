using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface IWorkingHourService : IBaseRepository<WorkingHour>, IWorkingHourRepositoryCustom<WorkingHour>
{
}