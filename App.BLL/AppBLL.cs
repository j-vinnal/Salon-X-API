using App.BLL.Mappers;
using App.BLL.Services;
using App.Contracts.BLL;
using App.Contracts.BLL.Services;
using App.Contracts.DAL;
using AutoMapper;
using Base.BLL;

namespace App.BLL;

public class AppBLL : BaseBLL<IAppUOW>, IAppBLL
{
    private readonly IMapper _mapper;
    private IBookingService? _bookingService;
    private IClientService? _clientService;
    private ICompanyService? _companyService;
    private IServiceService? _serviceService;
    private IWorkingHourService? _workingHourService;

    protected new IAppUOW Uow;

    public AppBLL(IAppUOW uow, IMapper mapper) : base(uow)
    {
        Uow = uow;
        _mapper = mapper;
    }

    public IServiceService ServiceService =>
        _serviceService ?? new ServiceService(Uow, new ServiceMapper(_mapper));

    public ICompanyService CompanyService =>
        _companyService ?? new CompanyService(Uow, new CompanyMapper(_mapper), new CompanyWithServicesMapper(_mapper));

    public IWorkingHourService WorkingHourService =>
        _workingHourService ?? new WorkingHourService(Uow, new WorkingHourMapper(_mapper));

    public IBookingService BookingService =>
        _bookingService ?? new BookingService(Uow, new BookingMapper(_mapper), new MonthlyRevenueMapper(_mapper));

    public IClientService ClientService =>
        _clientService ?? new ClientService(Uow, new ClientMapper(_mapper));
}