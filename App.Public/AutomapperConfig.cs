using App.DTO.DAL;
using AutoMapper;
using Booking = App.Domain.Booking;
using Client = App.Domain.Client;
using Company = App.Domain.Company;
using Service = App.Domain.Service;
using WorkingHour = App.Domain.WorkingHour;

namespace App.Public;

public class AutomapperConfig : Profile
{
    //BrandsCount works without additional conf also
    public AutomapperConfig()
    {
        /*
        CreateMap<Provider, App.Public.v1.Provider>()
            .ForMember(
                dest => dest.BrandsCount,
                options =>
                    options.MapFrom(src => src.Brands!.Count)
            );
        // .ReverseMap();
*/

        //Company

        CreateMap<Company, DTO.DAL.Company>().ReverseMap();

        CreateMap<DTO.DAL.Company, DTO.BLL.Company>().ReverseMap();

        CreateMap<DTO.BLL.Company, DTO.Public.v1.Company>().ReverseMap();


        //CompanyWithServices

        CreateMap<CompanyWithServices, DTO.BLL.CompanyWithServices>().ReverseMap();

        CreateMap<DTO.BLL.CompanyWithServices, DTO.Public.v1.CompanyWithServices>().ReverseMap();


        //Service

        CreateMap<Service, DTO.DAL.Service>().ReverseMap();

        CreateMap<DTO.DAL.Service, DTO.BLL.Service>().ReverseMap();

        CreateMap<DTO.BLL.Service, DTO.Public.v1.Service>().ReverseMap();

        //WorkingHour
        CreateMap<WorkingHour, DTO.DAL.WorkingHour>().ReverseMap();

        CreateMap<DTO.DAL.WorkingHour, DTO.BLL.WorkingHour>().ReverseMap();

        CreateMap<DTO.BLL.WorkingHour, DTO.Public.v1.WorkingHour>().ReverseMap();

        //Booking
        CreateMap<Booking, DTO.DAL.Booking>().ReverseMap();

        CreateMap<DTO.DAL.Booking, DTO.BLL.Booking>().ReverseMap();

        CreateMap<DTO.BLL.Booking, DTO.Public.v1.Booking>().ReverseMap();

        //Client
        CreateMap<Client, DTO.DAL.Client>().ReverseMap();

        CreateMap<DTO.DAL.Client, DTO.BLL.Client>().ReverseMap();

        CreateMap<DTO.BLL.Client, DTO.Public.v1.Client>().ReverseMap();

        //MonthlyRevenue
        CreateMap<MonthlyRevenue, DTO.BLL.MonthlyRevenue>().ReverseMap();
        CreateMap<DTO.BLL.MonthlyRevenue, DTO.Public.v1.MonthlyRevenue>().ReverseMap();
    }
}