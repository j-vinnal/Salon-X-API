using AutoMapper;
using Base.DAL;

namespace App.BLL.Mappers;


public class BookingMapper : BaseMapper<App.DTO.DAL.Booking, App.DTO.BLL.Booking>
{
    public BookingMapper(IMapper mapper) : base(mapper)
    {
    }
}