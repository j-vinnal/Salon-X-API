using App.DTO.DAL;
using AutoMapper;
using Base.DAL;

namespace App.BLL.Mappers;

public class ServiceMapper : BaseMapper<Service, DTO.BLL.Service>
{
    public ServiceMapper(IMapper mapper) : base(mapper)
    {
    }
}