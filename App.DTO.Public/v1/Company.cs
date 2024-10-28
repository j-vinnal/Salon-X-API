using System.ComponentModel.DataAnnotations;
using Base.Contracts;
using Attribute = Base.Resources.Attribute;

namespace App.DTO.Public.v1;

public class Company : IEntityId
{
    //[MinLength(2)]
    [MinLength(2, ErrorMessageResourceType = typeof(Attribute), ErrorMessageResourceName = "ValueMinLength")]
    [Display(ResourceType = typeof(Resources.Domain.Company), Name = "CompanyName")]
    public string CompanyName { get; set; } = default!;

    public string? CompanyLogoPath { get; set; }

    public string PublicUrl { get; set; } = default!;
    public Guid Id { get; set; }
}