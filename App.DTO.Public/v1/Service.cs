using App.Domain.Enums;
using Base.Contracts;

namespace App.DTO.Public.v1;

public class Service : IEntityId
{
    public string ServiceName { get; set; } = default!;
    public string Description { get; set; } = default!;

    public decimal Price { get; set; }

    public int Duration { get; set; }


    public EServiceStatus Status { get; set; } = EServiceStatus.Active;

    public Guid CompanyId { get; set; }
    public Guid Id { get; set; }
}