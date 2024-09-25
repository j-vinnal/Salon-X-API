using App.DTO.DAL;
using AutoMapper;
using Base.DAL;

namespace App.BLL.Mappers;

public class CompanyWithServicesMapper : BaseMapper<CompanyWithServices, DTO.BLL.CompanyWithServices>
{
    public CompanyWithServicesMapper(IMapper mapper) : base(mapper)
    {
    }
}