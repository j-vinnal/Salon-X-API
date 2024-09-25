using Base.Contacts;
using Base.Contracts.Domain;
using Base.Domain;

namespace App.Domain.Identity;

public class AppRefreshToken : BaseRefreshToken, IEntityId, IDomainAppUser<AppUser>
{
    public Guid AppUserId { get; set; }
    public AppUser? User { get; set; }
}