using AutoMapper;
using Base.DAL;

namespace App.BLL.Mappers;




public class MonthlyRevenueMapper : BaseMapper<DTO.DAL.MonthlyRevenue, DTO.BLL.MonthlyRevenue>
{
    public MonthlyRevenueMapper(IMapper mapper) : base(mapper)
    {
    }
}