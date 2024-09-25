using App.DTO.DAL;
using AutoMapper;
using Base.DAL;

namespace App.DAL.EF.Mappers;


public class MonthlyRevenueMapper : BaseMapper<MonthlyRevenue, DTO.DAL.MonthlyRevenue>
{
    public MonthlyRevenueMapper(IMapper mapper) : base(mapper)
    {
    }
}