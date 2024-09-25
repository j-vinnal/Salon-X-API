using AutoMapper;
using Base.DAL;

namespace App.Public.Mappers;

public class ClientMapper : BaseMapper<App.DTO.BLL.Client, DTO.Public.v1.Client>
{
    public ClientMapper(IMapper mapper) : base(mapper)
    {
    }
}