using Base.Contracts;
using Base.Contracts;

namespace App.DTO.BLL;

public class CompanyWithServices : IEntityId
{
    public string CompanyName { get; set; } = default!;

    public string? CompanyLogoPath { get; set; }

    public string PublicUrl { get; set; } = default!;

    public List<Service> Services { get; set; } = new();

    public Guid Id { get; set; }
}