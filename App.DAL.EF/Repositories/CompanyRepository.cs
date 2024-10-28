using App.Contracts.DAL.Repositories;
using App.DTO.DAL;
using Base.Contacts;
using Base.Contracts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;
using Company = App.Domain.Company;

namespace App.DAL.EF.Repositories;

public class CompanyRepository : EFBaseRepository<Company, DTO.DAL.Company, AppDbContext>, ICompanyRepository
{
    public CompanyRepository(AppDbContext dataContext, IMapper<Company, DTO.DAL.Company> mapper) : base(dataContext,
        mapper)
    {
    }
    
    
    public async Task<CompanyWithServices?> GetByPublicUrlWithServicesAsync(string publicUrl)
    {
        return await RepositoryDbSet
            .Where(p => p.PublicUrl.ToLower() == publicUrl.ToLower())
            .Include(p => p.Services)
            .Select(s => new CompanyWithServices
            {
                Id = s.Id,
                CompanyName = s.CompanyName,
                CompanyLogoPath = s.CompanyLogoPath,
                PublicUrl = s.PublicUrl,
                Services = s.Services!.Select(service => new Service
                {
                    Id = service.Id,
                    ServiceName = service.ServiceName,
                    Description = service.Description,
                    Price = service.Price,
                    Duration = service.Duration,
                    Status = service.Status,
                    CompanyId = service.CompanyId
                }).OrderBy(s => s.ServiceName).ToList()
            })
            .FirstOrDefaultAsync();
    }
}