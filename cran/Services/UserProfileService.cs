﻿using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using cran.Data;
using cran.Model.Dto;
using cran.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace cran.Services
{
    public class UserProfileService : CraniumService, IUserProfileService
    {
        public UserProfileService(ApplicationDbContext context, IDbLogService dbLogService, IPrincipal principal) : base(context, dbLogService, principal)
        {
        }

        public async Task CreateUserAsync(UserInfoDto info)
        {
            CranUser user = await _context.CranUsers.Where(x => x.UserId == info.Name).SingleOrDefaultAsync();
            if(user == null)
            {
                user = new CranUser
                {
                    UserId = info.Name,
                    IsAnonymous = info.IsAnonymous,
                };
                _context.CranUsers.Add(user);
                await SaveChangesAsync();
            }         
        }

        public async Task<UserInfoDto> GetUserInfoAsync()
        {
            CranUser user = await GetCranUserAsync();
            return ToProfileDto(user);
        }

        private UserInfoDto ToProfileDto(CranUser user)
        {
            return new UserInfoDto
            {
                Name = user.UserId,
                IsAnonymous = user.IsAnonymous,
            };
        }
    }
}
