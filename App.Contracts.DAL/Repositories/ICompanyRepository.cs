using App.DTO.DAL;
using Base.Contracts.DAL;

namespace App.Contracts.DAL.Repositories;

public interface ICompanyRepository : IBaseRepository<Company>, ICompanyRepositoryCustom<Company>
{
// add here custom methods for repo only
    Task<CompanyWithServices?> GetByPublicUrlWithServicesAsync(string publicUrl);
}

public interface ICompanyRepositoryCustom<TEntity>
{
    //add here shared methods between repo and service
}