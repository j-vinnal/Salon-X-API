using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.BLL;
using Base.Contacts;
using Base.Contracts;
using CompanyWithServices = App.DTO.DAL.CompanyWithServices;

namespace App.BLL.Services;

public class CompanyService : BaseEntityService<Company, DTO.DAL.Company, ICompanyRepository>,
    ICompanyService
{
    private readonly IMapper<CompanyWithServices, DTO.BLL.CompanyWithServices> _companyWithServicesMapper;
    private readonly IAppUOW _uow;


    public CompanyService(IAppUOW uow, IMapper<DTO.DAL.Company, Company> mapper,
        IMapper<CompanyWithServices, DTO.BLL.CompanyWithServices> companyWithServicesMapper) : base(
        uow.CompanyRepository, mapper)
    {
        _uow = uow;
        _companyWithServicesMapper = companyWithServicesMapper;
    }


    public async Task<DTO.BLL.CompanyWithServices?> GetByPublicUrlWithServicesAsync(string publicUrl)
    {
        return _companyWithServicesMapper.Map(await _uow.CompanyRepository.GetByPublicUrlWithServicesAsync(publicUrl));
    }
    
}