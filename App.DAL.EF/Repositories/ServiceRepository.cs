using App.Contracts.DAL.Repositories;
using App.Domain;
using Base.Contacts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class ServiceRepository : EFBaseRepository<Service, DTO.DAL.Service, AppDbContext>, IServiceRepository
{
    public ServiceRepository(AppDbContext dataContext, IMapper<Service, DTO.DAL.Service> mapper) : base(dataContext, mapper)
    {
    }

    public override async Task<bool> ExistsAsync(Guid id, Guid? userId = default)
    {
        return await RepositoryDbSet
            .Include(e => e.Company)
            .AnyAsync(e => e.Id == id && e.Company!.AppUserId == userId);
    }

    public override async Task<IEnumerable<DTO.DAL.Service>> GetAllAsync(Guid userId = default, bool noTracking = true)
    {
        var query = RepositoryDbSet
            .Include(e => e.Company)
            .Where(e => e.Company!.AppUserId == userId)
            .Select(s => new DTO.DAL.Service
            {
                Id = s.Id,
                ServiceName = s.ServiceName,
                Description = s.Description,
                Price = s.Price,
                Duration = s.Duration,
                Status = s.Status,
                CompanyId = s.Company!.Id
            });

        if (noTracking) query = query.AsNoTracking();

        return await query.OrderBy(p => p.ServiceName).ToListAsync();
    }

    public override async Task<DTO.DAL.Service?> FindAsync(Guid id, Guid? userId = default, bool noTracking = true)
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