﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Net.Core.EntityModels.Identity;
using Net.Core.IDomainServices.AutoMapper;
using Net.Core.IRepositories.Core;
using Net.Core.ViewModels.Identity.WebApi;
using AutoMapper;

namespace Net.Core.DomainServices.IdentityStores
{
    public class CustomUserStore : IUserLoginStore<IdentityUserViewModel>, 
        IUserClaimStore<IdentityUserViewModel>, 
        IUserRoleStore<IdentityUserViewModel>, 
        IUserPasswordStore<IdentityUserViewModel>, 
        IUserSecurityStampStore<IdentityUserViewModel>, 
        IUserStore<IdentityUserViewModel>,
        IUserEmailStore<IdentityUserViewModel>, IDisposable
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CustomUserStore(IUnitOfWork _unitOfWork, IMapper _mapper)
        {
            unitOfWork = _unitOfWork;
            mapper = _mapper;
        }

        #region IUserStore<IdentityUserViewModel, long> Members

        public Task<string> GetUserIdAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            //var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            //var userId = (model != null) ? model.Id.ToString() : string.Empty;

            return Task.FromResult<string>(viewModel.Id.ToString());
        }

        public Task<string> GetUserNameAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            //var model = unitOfWork.UserRepository.FindByUserName(viewModel.UserName);
            //var userName = (model != null) ? model.UserName : string.Empty;
            return Task.FromResult<string>(viewModel.UserName);
        }

        public Task SetUserNameAsync(IdentityUserViewModel viewModel, string userName, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentException("user");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");
            model.UserName = userName;
            unitOfWork.UserRepository.Update(model);
            return unitOfWork.CommitAsync();
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            return Task.FromResult<string>(model.UserName);
        }

        public Task SetNormalizedUserNameAsync(IdentityUserViewModel viewModel, string normalizedName, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentException("user");

            //var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            //if (model == null)
            //    throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");
            viewModel.UserName = normalizedName;
            //unitOfWork.UserRepository.Update(model);
            return Task.FromResult(0);
        }

        public Task<IdentityResult> CreateAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");

            var model = GetUserModel(viewModel);
            unitOfWork.UserRepository.Add(model);
            var result = unitOfWork.CommitAsync().Result;
            if(result > 0)
            {
                viewModel.Id = model.Id;
            }
            return result > 0 ? Task.FromResult(IdentityResult.Success) : Task.FromResult(new IdentityResult { });
        }

        public Task<IdentityResult> UpdateAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentException("user");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            unitOfWork.UserRepository.Update(model);
            var result = unitOfWork.CommitAsync().Result;
            return result > 0 ? Task.FromResult(IdentityResult.Success) : Task.FromResult(new IdentityResult { });
        }

        public Task<IdentityResult> DeleteAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");

            var model = GetUserModel(viewModel);

            unitOfWork.UserRepository.Delete(model);
            var result = unitOfWork.CommitAsync().Result;
            return result > 0 ? Task.FromResult(IdentityResult.Success) : Task.FromResult(new IdentityResult { });
        }

        public Task<IdentityUserViewModel> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var model = unitOfWork.UserRepository.FindById(userId);
            return Task.FromResult<IdentityUserViewModel>(GetIdentityUserViewModel(model));
        }

        public Task<IdentityUserViewModel> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var model = unitOfWork.UserRepository.FindByUserName(normalizedUserName);
            return Task.FromResult<IdentityUserViewModel>(GetIdentityUserViewModel(model));
        }

        #endregion

        #region IUserClaimStore<IdentityUserViewModel, long> Members

        public Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            return Task.FromResult<IList<System.Security.Claims.Claim>>(model.Claims.Select(x => new System.Security.Claims.Claim(x.ClaimType, x.ClaimValue)).ToList());
        }

        public Task AddClaimsAsync(IdentityUserViewModel viewModel, IEnumerable<System.Security.Claims.Claim> claims, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            if (claims == null)
                throw new ArgumentNullException("claim");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            foreach (var claim in claims)
            {
                var claimEntityModel = new EntityModels.Identity.Claim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    User = model
                };
                model.Claims.Add(claimEntityModel);
            }

            unitOfWork.UserRepository.Update(model);
            return unitOfWork.CommitAsync();
        }

        public Task ReplaceClaimAsync(IdentityUserViewModel viewModel, System.Security.Claims.Claim claim, System.Security.Claims.Claim newClaim, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            if (claim == null || newClaim == null)
                throw new ArgumentNullException("claim");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            var claimEntityModel = new EntityModels.Identity.Claim
            {
                ClaimType = newClaim.Type,
                ClaimValue = newClaim.Value,
                User = model
            };

            var existingClaims = model.Claims.Where(o => o.ClaimType == claim.Type && o.ClaimValue == claim.Value).ToList();
            if (existingClaims != null && existingClaims.Count >0)
            {
                foreach (var oldClaim in existingClaims)
                {
                    model.Claims.Remove(oldClaim);
                }
            }

            model.Claims.Add(claimEntityModel);

            unitOfWork.UserRepository.Update(model);
            return unitOfWork.CommitAsync();
        }

        public Task RemoveClaimsAsync(IdentityUserViewModel viewModel, IEnumerable<System.Security.Claims.Claim> claims, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            if (claims == null)
                throw new ArgumentNullException("claim");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            foreach (var claim in claims)
            {
                var claimEntityModel = model.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
                model.Claims.Remove(claimEntityModel);
            }

            unitOfWork.UserRepository.Update(model);
            return unitOfWork.CommitAsync();
        }

        public Task<IList<IdentityUserViewModel>> GetUsersForClaimAsync(System.Security.Claims.Claim claim, CancellationToken cancellationToken)
        {
            if (claim == null)
                throw new ArgumentNullException("user");

            var ids = unitOfWork.ClaimRepository.GetUserIdsByClaim(claim);
            if (ids == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            var users = unitOfWork.UserRepository.GetMany(o => ids.Contains(o.Id)).ToViewModel<User,IdentityUserViewModel>(mapper).ToList();
            return Task.FromResult<IList<IdentityUserViewModel>>(users);
        }

        #endregion

        #region IUserLoginStore<IdentityUserViewModel, long> Members

        public Task AddLoginAsync(IdentityUserViewModel viewModel, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            var externalLoginEntityModel = new ExternalLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                User = model
            };
            model.Logins.Add(externalLoginEntityModel);

            unitOfWork.UserRepository.Update(model);
            return unitOfWork.CommitAsync();
        }

        public Task RemoveLoginAsync(IdentityUserViewModel viewModel, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            if (loginProvider == null)
                throw new ArgumentNullException("login");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            var loginModel = model.Logins.FirstOrDefault(x => x.LoginProvider == loginProvider && x.ProviderKey == providerKey);
            model.Logins.Remove(loginModel);

            unitOfWork.UserRepository.Update(model);
            return unitOfWork.CommitAsync();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            return Task.FromResult<IList<UserLoginInfo>>(model.Logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey,x.User.UserName)).ToList());
        }

        public Task<IdentityUserViewModel> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var identityUser = default(IdentityUserViewModel);

            var externalLoginEntityModel = unitOfWork.ExternalLoginRepository.GetByProviderAndKey(loginProvider, providerKey);
            if (externalLoginEntityModel != null)
            {
                var user = unitOfWork.UserRepository.FindById(externalLoginEntityModel.UserId);
                identityUser = GetIdentityUserViewModel(user);
            }

            return Task.FromResult<IdentityUserViewModel>(identityUser);
        }

        #endregion

        #region IUserRoleStore<IdentityUserViewModel, long> Members

        public Task AddToRoleAsync(IdentityUserViewModel viewModel, string roleName, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: roleName.");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");
            var roleEntityModel = unitOfWork.RoleRepository.FindByName(roleName);
            if (roleEntityModel == null)
                throw new ArgumentException("roleName does not correspond to a Role entity.", "roleName");

            var userRole = new UserRole { UserId = viewModel.Id, RoleId = roleEntityModel.Id };
            unitOfWork.UserRoleRepository.Add(userRole);

            return unitOfWork.CommitAsync();
        }

        public Task RemoveFromRoleAsync(IdentityUserViewModel viewModel, string roleName, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: role.");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");
            var ids = model.UserRoles.Select(o => o.RoleId);
            var r = unitOfWork.RoleRepository.Get(x => x.Name == roleName);
            var userRole = model.UserRoles.FirstOrDefault(o => o.RoleId == r.Id);
            model.UserRoles.Remove(userRole);

            unitOfWork.UserRepository.Update(model);
            return unitOfWork.CommitAsync();
        }

        public Task<IList<string>> GetRolesAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");

            var userEntity = unitOfWork.UserRepository.FindByEmail(viewModel.Email);
            if (userEntity == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            var ids = unitOfWork.UserRoleRepository.GetMany(o => o.UserId == userEntity.Id).Select(o => o.RoleId).ToList();
            var list = unitOfWork.RoleRepository.GetMany(o => ids.Contains(o.Id));
            return Task.FromResult<IList<string>>(list.Select(x => x.Name).ToList());
        }

        public Task<bool> IsInRoleAsync(IdentityUserViewModel viewModel, string roleName, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: role.");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");

            var list = unitOfWork.RoleRepository.GetMany(o => o.Name == roleName).Select(s=>s.Id).ToList();

            var roleExists = unitOfWork.UserRoleRepository.Contains(o => o.UserId == viewModel.Id && list.Contains(o.RoleId));

            return Task.FromResult<bool>(roleExists);
        }

        public Task<IList<IdentityUserViewModel>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var model = unitOfWork.RoleRepository.FindByName(roleName);
            if (model == null)
                throw new ArgumentException("IdentityRoleViewModel does not correspond to a Role entity.", "roleName");

            var ids = model.UserRoles.Select(o => o.UserId);
            IList<IdentityUserViewModel> list = unitOfWork.UserRepository.GetMany(o => ids.Contains(o.Id)).ToViewModel<User,IdentityUserViewModel>(mapper).ToList();
            return Task.FromResult(list);
        }

        #endregion

        #region IUserPasswordStore<IdentityUserViewModel, long> Members
        public Task<string> GetPasswordHashAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<string>(viewModel.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<bool>(!string.IsNullOrWhiteSpace(viewModel.PasswordHash));
        }

        public Task SetPasswordHashAsync(IdentityUserViewModel viewModel, string passwordHash, CancellationToken cancellationToken)
        {
            viewModel.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        #endregion

        #region IUserSecurityStampStore<IdentityUserViewModel, long> Members
        public Task<string> GetSecurityStampAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<string>(viewModel.SecurityStamp);
        }

        public Task SetSecurityStampAsync(IdentityUserViewModel viewModel, string stamp, CancellationToken cancellationToken)
        {
            viewModel.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        #endregion

        #region IUserEmailStore<IdentityUserViewModel, long> Members

        public Task SetEmailAsync(IdentityUserViewModel viewModel, string email, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentException("user");

            var model = unitOfWork.UserRepository.FindById(viewModel.Id);
            if (model == null)
                throw new ArgumentException("IdentityUserViewModel does not correspond to a User entity.", "user");
            model.Email = email;
            unitOfWork.UserRepository.Update(model);
            return unitOfWork.CommitAsync();
        }

        public Task<string> GetEmailAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(viewModel.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            return Task.FromResult<bool>(true);
        }

        public Task SetEmailConfirmedAsync(IdentityUserViewModel viewModel, bool confirmed, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentException("user");

            viewModel.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<IdentityUserViewModel> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var model = unitOfWork.UserRepository.FindByEmail(normalizedEmail);
            return Task.FromResult<IdentityUserViewModel>(GetIdentityUserViewModel(model));

        }

        public Task<string> GetNormalizedEmailAsync(IdentityUserViewModel viewModel, CancellationToken cancellationToken)
        {
            var model = unitOfWork.UserRepository.FindByEmail(viewModel.Email);
            return Task.FromResult<string>(model.Email);
        }

        public Task SetNormalizedEmailAsync(IdentityUserViewModel viewModel, string normalizedEmail, CancellationToken cancellationToken)
        {
            if (viewModel == null)
                throw new ArgumentException("user");

            viewModel.Email = normalizedEmail;
            return Task.FromResult(0);
        }

        #endregion

        #region Private Methods
        private User GetUserModel(IdentityUserViewModel viewModel)
        {
            if (viewModel == null)
                return null;

            var model = viewModel.ToEntityModel<User, IdentityUserViewModel>(mapper);
            return model;
        }

        private IdentityUserViewModel GetIdentityUserViewModel(User model)
        {
            if (model == null)
                return null;

            var viewModel = model.ToViewModel<User, IdentityUserViewModel>(mapper);

            if (model.UserRoles != null)
            {
                viewModel.RoleName = model.UserRoles.FirstOrDefault().Role.Name;
            }
            return viewModel;
        }

        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UserStore() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}
