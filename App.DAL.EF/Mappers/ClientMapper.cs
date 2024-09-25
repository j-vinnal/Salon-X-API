using App.Domain;
using AutoMapper;
using Base.DAL;

namespace App.DAL.EF.Mappers;

public class ClientMapper : BaseMapper<Client, DTO.DAL.Client>
{
    public ClientMapper(IMapper mapper) : base(mapper)
    {
    }
}