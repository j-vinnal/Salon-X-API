using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface IBookingService : IBaseRepository<Booking>, IBookingRepositoryCustom<Booking>
{
    // Define additional methods if needed
    Task<decimal> CalculateTotalTurnoverAsync(Guid appUserId, Guid? serviceId = null);
    Task<List<MonthlyRevenue?>> CalculateMonthlyTurnoverAsync(Guid appUserId, Guid? serviceId = null);

    new Booking Add(Booking entity);

    new Booking Update(Booking entity);
}