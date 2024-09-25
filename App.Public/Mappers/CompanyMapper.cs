using App.DTO.BLL;
using AutoMapper;
using Base.DAL;

namespace App.Public.Mappers;

public class CompanyMapper : BaseMapper<App.DTO.BLL.Company, App.DTO.Public.v1.Company>
{
    public CompanyMapper(IMapper mapper) : base(mapper)
    {
    }
}