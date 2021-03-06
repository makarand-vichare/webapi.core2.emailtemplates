﻿using Net.Core.IRepositories.Identity;
using Net.Core.IRepositories.Queues;
using System;
using System.Threading;
using System.Threading.Tasks;
using Net.Core.IRepositories.Localization;
using Net.Core.IRepositories.Location;
using Net.Core.EntityModels.Core;

namespace Net.Core.IRepositories.Core
{
    public interface IUnitOfWork : IDisposable
    {
        #region Methods
        int Commit();
        Task<int> CommitAsync();
        Task<int> CommitAsync(CancellationToken cancellationToken);
        #endregion
        IExternalLoginRepository ExternalLoginRepository { get; set; }
        IRoleRepository RoleRepository { get; set; }
        IUserRepository UserRepository { get; set; }
        IUserRoleRepository UserRoleRepository { get; set; }
        IClaimRepository ClaimRepository { get; set; }

        IRefreshTokenRepository RefreshTokenRepository { get; set; }
        IKeyGroupRepository KeyGroupRepository { get; set; }
        ILocalizationKeyRepository LocalizationKeyRepository { get; set; }
        ICountryRepository CountryRepository { get; set; }
        ICityRepository CityRepository { get; set; }

        IEmailQueueRepository EmailQueueRepository { get; set; }
        IPdfQueueRepository PdfQueueRepository { get; set; }
        IRequestQueueRepository RequestQueueRepository { get; set; }
        IClientRepository ClientRepository { get; set; }
        IBaseRepository<T> SetDbContext<T>(IBaseRepository<T> repository) where T : BaseEntity;
        IIdentityBaseRepository<T> SetDbContext<T>(IIdentityBaseRepository<T> repository) where T : IdentityColumnEntity;
    }
}
