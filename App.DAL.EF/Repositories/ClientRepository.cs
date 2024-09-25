using App.Contracts.DAL.Repositories;
using App.Domain;
using Base.Contacts;
using Base.DAL.EF;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF.Repositories;

public class ClientRepository : EFBaseRepository<Client, DTO.DAL.Client, AppDbContext>, IClientRepository
{
    public ClientRepository(AppDbContext dataContext, IMapper<Client, DTO.DAL.Client> mapper) : base(dataContext,
        mapper)
    {
    }


    // Implement necessary methods here

    public async Task<DTO.DAL.Client?> FindByEmailAsync(string email)
    {
        var client = await RepositoryDbSet
            .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());

        return client == null ? null : Mapper.Map(client);
    }
}