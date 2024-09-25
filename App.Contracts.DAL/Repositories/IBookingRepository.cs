using App.DTO.DAL;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface IBookingRepository : IBaseRepository<Booking>, IBookingRepositoryCustom<Booking>
{
    // Define additional methods if needed
    
    Task<decimal> CalculateTotalTurnoverAsync(Guid userId, Guid? serviceId = null);
    Task<List<MonthlyRevenue>> CalculateMonthlyTurnoverAsync(Guid userId, Guid? serviceId = null);
}

public interface IBookingRepositoryCustom<TEntity>
{
    // Define additional methods if needed
    
    Task<IEnumerable<TEntity>> GetAllAsync(Guid clientId, bool noTracking = true);
    
   
   
}