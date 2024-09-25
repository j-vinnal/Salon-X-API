using App.DTO.BLL;
using AutoMapper;
using Base.DAL;

namespace App.Public.Mappers;

public class ServiceMapper : BaseMapper<Service, App.DTO.Public.v1.Service>
{
    public ServiceMapper(IMapper mapper) : base(mapper)
    {
    }
}