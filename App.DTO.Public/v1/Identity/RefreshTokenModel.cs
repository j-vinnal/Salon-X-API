﻿namespace App.DTO.Public.v1.Identity;

public class RefreshTokenModel
{
    public string Jwt { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}