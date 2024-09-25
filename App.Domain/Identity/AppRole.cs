using Base.Contacts;
using Microsoft.AspNetCore.Identity;

namespace App.Domain.Identity;

public class AppRole : IdentityRole<Guid>, IEntityId
{
}