﻿using System.ComponentModel.DataAnnotations;
using Base.Contracts.Domain;
using Base.Domain;
using Microsoft.AspNetCore.Identity;

namespace Base.Tests.Domain;

public class TestEntity : EntityId, IDomainAppUser<IdentityUser<Guid>>, IDomainAppUserId<Guid>
{
    [MaxLength(128)] public string Value { get; set; } = default!;

    public Guid AppUserId { get; set; }
    public IdentityUser<Guid>? User { get; set; }
}