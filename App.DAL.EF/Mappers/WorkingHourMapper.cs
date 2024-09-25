using App.Domain;
using AutoMapper;
using Base.DAL;

namespace App.DAL.EF.Mappers;

public class WorkingHourMapper : BaseMapper<WorkingHour, DTO.DAL.WorkingHour>
{
    public WorkingHourMapper(IMapper mapper) : base(mapper)
    {
    }
    
}