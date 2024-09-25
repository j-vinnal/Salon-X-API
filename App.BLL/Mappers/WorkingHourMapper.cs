using App.DTO.DAL;
using AutoMapper;
using Base.DAL;

namespace App.BLL.Mappers;

public class WorkingHourMapper : BaseMapper<WorkingHour, DTO.BLL.WorkingHour>
{
    public WorkingHourMapper(IMapper mapper) : base(mapper)
    {
    }
}