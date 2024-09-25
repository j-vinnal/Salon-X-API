using App.Contracts.DAL.Repositories;
using App.Domain;
using Base.Contacts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class WorkingHourRepository : EFBaseRepository<WorkingHour, DTO.DAL.WorkingHour, AppDbContext>,
    IWorkingHourRepository
{
    public WorkingHourRepository(AppDbContext dataContext, IMapper<WorkingHour, DTO.DAL.WorkingHour> mapper) : base(
        dataContext, mapper)
    {
    }

    public override async Task<bool> ExistsAsync(Guid id, Guid? userId = default)
    {
        return await RepositoryDbSet
            .Include(e => e.Company)
            .AnyAsync(e => e.Id == id && e.Company!.AppUserId == userId);
    }

    public override async Task<IEnumerable<DTO.DAL.WorkingHour>> GetAllAsync(Guid userId = default, bool noTracking = true)
    {
        var query = RepositoryDbSet
            .Include(e => e.Company)
            .Where(e => e.Company!.AppUserId == userId)
            .Select(wh => new DTO.DAL.WorkingHour
            {
                Id = wh.Id,
                DayOfWeek = wh.DayOfWeek,
                StartTime = wh.StartTime,
                EndTime = wh.EndTime,
                IsActive = wh.IsActive,
                CompanyId = wh.Company!.Id
            });

        if (noTracking) query = query.AsNoTracking();

        return await query.OrderBy(e => e.DayOfWeek).ToListAsync();
    }

    public override async Task<DTO.DAL.WorkingHour?> FindAsync(Guid id, Guid? userId = default, bool noTracking = true)
    {
        var query = RepositoryDbSet
            .Include(e => e.Company)
            .Where(e => e.Id == id);

        if (userId != null) query = query.Where(e => e.Company!.AppUserId == userId);

        if (noTracking) query = query.AsNoTracking();

        var entity = await query.FirstOrDefaultAsync();
        return Mapper.Map(entity);
    }
}