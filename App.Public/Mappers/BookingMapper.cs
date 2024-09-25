using AutoMapper;
using Base.DAL;

namespace App.Public.Mappers;



public class BookingMapper : BaseMapper<App.DTO.BLL.Booking, DTO.Public.v1.Booking>
{
    public BookingMapper(IMapper mapper) : base(mapper)
    {
    }
}