﻿using Net.Core.DomainServices.Core;
using Net.Core.EntityModels.Identity;
using Net.Core.IDomainServices.AutoMapper;
using Net.Core.ViewModels.Identity.WebApi;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Net.Core.IDomainServices.IdentityStores;

namespace Net.Core.DomainServices.IdentityStores
{
    public class RefreshTokenService : BaseService<RefreshToken, RefreshTokenViewModel> , IRefreshTokenService
    {
        public async Task<bool> AddRefreshToken(RefreshTokenViewModel token)
        {
            var existingToken = UnitOfWork.RefreshTokenRepository.Get(r => r.Subject == token.Subject && r.ClientId == token.ClientId);
            if (existingToken != null)
            {
                var result = await RemoveRefreshToken(existingToken.TokenId);
            }

            var tokenEntity = token.ToEntityModel<RefreshToken, RefreshTokenViewModel>(Mapper);
            UnitOfWork.RefreshTokenRepository.Add(tokenEntity);
            return await UnitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var refreshToken = await UnitOfWork.RefreshTokenRepository.FindByTokenIdAsync(refreshTokenId);
            if (refreshToken != null)
            {
                UnitOfWork.RefreshTokenRepository.Delete(refreshToken);
                return await UnitOfWork.CommitAsync() > 0;
            }

            return false;
        }

        public async Task<bool> RemoveRefreshToken(RefreshTokenViewModel refreshToken)
        {
            var tokenEntity = refreshToken.ToEntityModel<RefreshToken, RefreshTokenViewModel>(Mapper);
            UnitOfWork.RefreshTokenRepository.Delete(tokenEntity);
            return await UnitOfWork.CommitAsync() > 0;
        }

        public async Task<RefreshTokenViewModel> FindRefreshToken(string refreshTokenId)
        {
            var refreshToken = await UnitOfWork.RefreshTokenRepository.FindByTokenIdAsync(refreshTokenId);
            var tokenViewModel = refreshToken.ToViewModel<RefreshToken, RefreshTokenViewModel>(Mapper);
            return tokenViewModel;
        }

        public List<RefreshTokenViewModel> GetAllRefreshTokens()
        {
            return UnitOfWork.RefreshTokenRepository
                .GetAll()
                .ToViewModel<RefreshToken, RefreshTokenViewModel>(Mapper)
                .ToList();
        }
    }
}
