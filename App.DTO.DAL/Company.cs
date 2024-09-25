using Base.Contacts;

namespace App.DTO.DAL;

public class Company : IEntityId
{


    public Guid Id { get; set; }
    public string CompanyName { get; set; } = default!;

    public string? CompanyLogoPath { get; set; }
    public string PublicUrl { get; set; } = default!;

    public Guid AppUserId { get; set; }
}