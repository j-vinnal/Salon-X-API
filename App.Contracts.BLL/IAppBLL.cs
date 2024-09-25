using App.Contracts.BLL.Services;
using Base.Contracts.BLL;

namespace App.Contracts.BLL;

public interface IAppBLL : IBaseBLL
{
    IServiceService ServiceService { get; }

    ICompanyService CompanyService { get; }
    
    IWorkingHourService WorkingHourService { get; }
    
    IBookingService BookingService { get; }
    
    IClientService ClientService { get; }
}