using App.DTO.BLL;
using AutoMapper;
using Base.DAL;

namespace App.Public.Mappers;

public class WorkingHourMapper : BaseMapper<WorkingHour, App.DTO.Public.v1.WorkingHour>
{
    public WorkingHourMapper(IMapper mapper) : base(mapper)
    {
    }
}

    
