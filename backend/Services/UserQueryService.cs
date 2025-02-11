using Backend.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class UserQueryService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserQueryService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApplicationUser?> FindUserAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return null;  
            }
            return user;
        }
    }
}