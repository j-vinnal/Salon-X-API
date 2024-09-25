using App.DTO.DAL;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface IServiceRepository : IBaseRepository<Service>, IServiceRepositoryCustom<Service>
{
}

public interface IServiceRepositoryCustom<TEntity>
{
    Task<bool> ExistsAsync(Guid id, Guid? userId = default);
    Task<IEnumerable<TEntity>> GetAllAsync(Guid userId = default, bool noTracking = true);
    Task<TEntity?> FindAsync(Guid id, Guid? userId = default, bool noTracking = true);
}