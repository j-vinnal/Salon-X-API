using App.DTO.BLL;
using AutoMapper;
using Base.DAL;

namespace App.Public.Mappers;

public class CompanyWithServicesMapper : BaseMapper<CompanyWithServices, DTO.Public.v1.CompanyWithServices>
{
    public CompanyWithServicesMapper(IMapper mapper) : base(mapper)
    {
    }
}