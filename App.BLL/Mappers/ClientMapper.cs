using AutoMapper;
using Base.DAL;

namespace App.BLL.Mappers;


public class ClientMapper : BaseMapper<App.DTO.DAL.Client, App.DTO.BLL.Client>
{
    public ClientMapper(IMapper mapper) : base(mapper)
    {
    }
}