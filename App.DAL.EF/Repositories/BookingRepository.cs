using App.Contracts.DAL.Repositories;
using App.DTO.DAL;
using Base.Contacts;
using Base.Contracts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Booking = App.Domain.Booking;

namespace App.DAL.EF.Repositories;

public class BookingRepository : EFBaseRepository<Booking, DTO.DAL.Booking, AppDbContext>, IBookingRepository
{
    public BookingRepository(AppDbContext dataContext, IMapper<Booking, DTO.DAL.Booking> mapper) : base(dataContext,
        mapper)
    {
    }

    public override async Task<IEnumerable<DTO.DAL.Booking>> GetAllAsync(Guid userId = default, bool noTracking = true)
    {
        var query = RepositoryDbSet
            .Include(e => e.Client)
            .Include(e => e.Service)
            .Where(e => e.Service!.Company!.AppUserId == userId)
            .Select(s => new DTO.DAL.Booking
            {
                Id = s.Id,
                BookingDate = s.BookingDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                Status = s.Status,
                ClientName = s.Client!.FirstName + " " + s.Client.LastName,
                ClientId = s.Client.Id,
                ServiceName = s.Service!.ServiceName,
                ServiceId = s.Service.Id
            });

        if (noTracking) query = query.AsNoTracking();

        return await query.OrderBy(p => p.BookingDate).ThenBy(p => p.StartTime).ThenBy(p => p.ClientName).ToListAsync();
    }

    public async Task<decimal> CalculateTotalTurnoverAsync(Guid userId, Guid? serviceId = null)
    {
        var bookings = RepositoryDbSet
            .Include(b => b.Service)
            .ThenInclude(b => b!.Company)
            .Where(b => b.Service!.Company!.AppUserId == userId &&
                        (!serviceId.HasValue || b.ServiceId == serviceId.Value));
        return await bookings.SumAsync(b => b.Service!.Price);
    }

    public async Task<List<MonthlyRevenue>> CalculateMonthlyTurnoverAsync(Guid userId, Guid? serviceId = null)
    {
        var bookings = RepositoryDbSet
            .Include(b => b.Service)
            .ThenInclude(s => s!.Company)
            .Where(b => b.Service!.Company!.AppUserId == userId &&
                        (!serviceId.HasValue || b.ServiceId == serviceId.Value));


        var monthlyRevenues = bookings
            .GroupBy(b => b.BookingDate.Month)
            .AsEnumerable()
            .Select(g => new MonthlyRevenue
            {
                Month = new DateTime(1, g.Key, 1).ToString("MMM"),
                Revenue = g.Sum(b => b.Service!.Price)
            }).ToList();

        return await Task.FromResult(monthlyRevenues);
    }
}