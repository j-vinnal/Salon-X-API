using App.Contracts.DAL.Repositories;
using Base.Contracts.DAL;

namespace App.Contracts.DAL;

public interface IAppUOW : IBaseUOW
{
    //list of your repositories


    IServiceRepository ServiceRepository { get; }

    ICompanyRepository CompanyRepository { get; }

    IWorkingHourRepository WorkingHourRepository { get; }

    IClientRepository ClientRepository { get; }

    IBookingRepository BookingRepository { get; }
}