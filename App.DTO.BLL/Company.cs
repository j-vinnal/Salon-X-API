using Base.Contacts;

namespace App.DTO.BLL;

public class Company : IEntityId
{
    public string CompanyName { get; set; } = default!;

    public string? CompanyLogoPath { get; set; }

    public string PublicUrl { get; set; } = default!;

    public Guid AppUserId { get; set; }
    public Guid Id { get; set; }
}