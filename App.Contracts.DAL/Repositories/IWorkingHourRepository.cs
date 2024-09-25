using Base.Contracts.DAL;
using WorkingHour = App.DTO.DAL.WorkingHour;

namespace App.Contracts.DAL.Repositories;

public interface IWorkingHourRepository : IBaseRepository<WorkingHour>, IWorkingHourRepositoryCustom<WorkingHour>
{
}

public interface IWorkingHourRepositoryCustom<TEntity>
{
    Task<bool> ExistsAsync(Guid id, Guid? userId = default);
    Task<IEnumerable<TEntity>> GetAllAsync(Guid userId = default, bool noTracking = true);

    Task<TEntity?> FindAsync(Guid id, Guid? userId = default, bool noTracking = true);
}