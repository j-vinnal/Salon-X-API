using App.DTO.DAL;
using AutoMapper;
using Base.DAL;

namespace App.BLL.Mappers;

public class CompanyMapper : BaseMapper<App.DTO.DAL.Company, App.DTO.BLL.Company>
{
    public CompanyMapper(IMapper mapper) : base(mapper)
    {
    }
}