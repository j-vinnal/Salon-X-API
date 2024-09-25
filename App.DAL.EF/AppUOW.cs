using App.Contracts.DAL;
using App.Contracts.DAL.Repositories;
using App.DAL.EF.Mappers;
using App.DAL.EF.Repositories;
using AutoMapper;
using Base.DAL.EF;

namespace App.DAL.EF;

public class AppUOW : EFBaseUOW<AppDbContext>, IAppUOW
{
    private readonly IMapper _mapper;
    private IBookingRepository? _bookingRepository;
    private IClientRepository? _clientRepository;
    private ICompanyRepository? _companyRepository;


    private IServiceRepository? _serviceRepository;
    private IWorkingHourRepository? _workingHourRepository;


    public AppUOW(AppDbContext uowDbContext, IMapper mapper) : base(uowDbContext)
    {
        _mapper = mapper;
    }


    //lazy loading, initialise only when needed
    public IWorkingHourRepository WorkingHourRepository =>
        _workingHourRepository ??= new WorkingHourRepository(UowDbContext, new WorkingHourMapper(_mapper));

    public IServiceRepository ServiceRepository =>
        _serviceRepository ??= new ServiceRepository(UowDbContext, new ServiceMapper(_mapper));

    public ICompanyRepository CompanyRepository =>
        _companyRepository ??= new CompanyRepository(UowDbContext, new CompanyMapper(_mapper));

    public IClientRepository ClientRepository =>
        _clientRepository ??= new ClientRepository(UowDbContext, new ClientMapper(_mapper));

    public IBookingRepository BookingRepository =>
        _bookingRepository ??= new BookingRepository(UowDbContext, new BookingMapper(_mapper));
}