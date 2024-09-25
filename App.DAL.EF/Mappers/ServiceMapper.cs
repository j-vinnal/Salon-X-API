using App.Domain;
using AutoMapper;
using Base.DAL;

namespace App.DAL.EF.Mappers;

public class ServiceMapper : BaseMapper<Service, DTO.DAL.Service>
{
    public ServiceMapper(IMapper mapper) : base(mapper)
    {
    }
}