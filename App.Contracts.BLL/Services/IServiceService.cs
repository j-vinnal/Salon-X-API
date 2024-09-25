﻿using App.Contracts.DAL.Repositories;
using App.DTO.BLL;
using Base.Contracts.DAL;

namespace App.Contracts.BLL.Services;

public interface IServiceService : IBaseRepository<Service>, IServiceRepositoryCustom<Service>
{
}