using AutoMapper;
using Base.DAL;

namespace App.Public.Mappers;

public class MonthlyRevenueMapper : BaseMapper<App.DTO.BLL.MonthlyRevenue, App.DTO.Public.v1.MonthlyRevenue>
{
    public MonthlyRevenueMapper(IMapper mapper) : base(mapper)
    {
    }
}