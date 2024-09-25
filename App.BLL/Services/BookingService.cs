using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.BLL;
using Base.Contacts;
using MonthlyRevenue = App.DTO.DAL.MonthlyRevenue;

namespace App.BLL.Services;

public class BookingService : BaseEntityService<Booking, DTO.DAL.Booking, IBookingRepository>,
    IBookingService
{
    private readonly IMapper<MonthlyRevenue, DTO.BLL.MonthlyRevenue> _monthlyRevenueMapper;
    private readonly IAppUOW _uow;

    public BookingService(IAppUOW uow, IMapper<DTO.DAL.Booking, Booking> mapper,
        IMapper<MonthlyRevenue, DTO.BLL.MonthlyRevenue> monthlyRevenueMapper) : base(
        uow.BookingRepository, mapper)
    {
        _uow = uow;
        _monthlyRevenueMapper = monthlyRevenueMapper;
    }

    public async Task<decimal> CalculateTotalTurnoverAsync(Guid appUserId, Guid? serviceId = null)
    {
        var totalTurnover = await _uow.BookingRepository.CalculateTotalTurnoverAsync(appUserId, serviceId);
        return totalTurnover;
    }

    public async Task<List<DTO.BLL.MonthlyRevenue?>> CalculateMonthlyTurnoverAsync(Guid appUserId,
        Guid? serviceId = null)
    {
        var dalMonthlyRevenues = await _uow.BookingRepository.CalculateMonthlyTurnoverAsync(appUserId, serviceId);
        var bllMonthlyRevenues = dalMonthlyRevenues.Select(mr => _monthlyRevenueMapper.Map(mr)).ToList();
        return bllMonthlyRevenues;
    }


    public override Booking Add(Booking entity)
    {
        var service = _uow.ServiceRepository.FindAsync(entity.ServiceId).Result;
        if (service == null) throw new ArgumentException("Invalid ServiceId");

        entity.EndTime = entity.StartTime.Add(TimeSpan.FromMinutes(service.Duration));
        return base.Add(entity);
    }

    public override Booking Update(Booking entity)
    {
        var service = _uow.ServiceRepository.FindAsync(entity.ServiceId).Result;
        if (service == null) throw new ArgumentException("Invalid ServiceId");

        entity.EndTime = entity.StartTime.Add(TimeSpan.FromMinutes(service.Duration));
        return base.Update(entity);
    }
}