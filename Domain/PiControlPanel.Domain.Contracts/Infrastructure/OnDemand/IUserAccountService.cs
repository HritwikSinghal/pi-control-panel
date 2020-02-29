﻿namespace PiControlPanel.Domain.Contracts.Infrastructure.OnDemand
{
    using PiControlPanel.Domain.Models;
    using System.Threading.Tasks;

    public interface IUserAccountService
    {
        Task<bool> ValidateAsync(UserAccount userAccount);

        Task<bool> IsSuperUserAsync(UserAccount userAccount);
    }
}