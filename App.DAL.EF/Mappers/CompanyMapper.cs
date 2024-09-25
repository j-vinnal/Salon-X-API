using App.Domain;
using AutoMapper;
using Base.DAL;

namespace App.DAL.EF.Mappers;

public class CompanyMapper : BaseMapper<App.Domain.Company, App.DTO.DAL.Company>
{
    public CompanyMapper(IMapper mapper) : base(mapper)
    {
    }

    /*
    public App.Public.v1.Provider? MapWithCount(Provider entity)
    {
        var res = Mapper.Map<App.Public.v1.Provider>(entity);
        res.BrandsCount = entity.Brands?.Count ?? 0;
        return res;
    }
    */
}