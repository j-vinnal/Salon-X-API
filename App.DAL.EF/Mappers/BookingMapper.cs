using App.Domain;
using AutoMapper;
using Base.DAL;

namespace App.DAL.EF.Mappers;

public class BookingMapper : BaseMapper<Booking, DTO.DAL.Booking>
{
    public BookingMapper(IMapper mapper) : base(mapper)
    {
    }
}